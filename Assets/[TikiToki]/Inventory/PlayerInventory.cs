using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TikiToki.Gameplay;

namespace TikiToki.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        [System.Serializable]
        public class InventorySlot
        {
            public ItemData item;
            public int count;
        }

        [Header("Animations")]
        public Animator playerAnimator;

        public static Action<string> OnItemCollected;
        public static Action<string> OnBonfireMaterialAdded;
        public static Action OnItemDropped;

        [Header("Settings")]
        public InventorySlot[] slots = new InventorySlot[2];
        public int activeSlotIndex = 0;

        [Header("UI References")]
        public Image[] slotBackgrounds;
        public Image[] iconSlots;
        public TextMeshProUGUI[] stackTexts;

        [Header("Hand Visualization")]
        public Transform holdPoint;
        private GameObject _currentHeldObject;

        [Header("Interaction")]
        public float interactionDistance = 1.5f;
        public LayerMask interactionLayer;

        private IInteractable _lastTargetedItem;
        private bool _blockLightingUntilRelease = false;

        private Color frameColor = new Color(0.494f, 0.494f, 0.494f, 1f);
        private Color selectedColor = new Color(1f, 0.92f, 0.016f, 1f);

        [Header("Sounds")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip dropSound;

        // Exposed properties for interactables
        public TikiToki.Gameplay.Environment.Bonfire BonfireBeingLit { get; set; }

        void Start()
        {
            UpdateUI();
        }

        void Update()
        {
            ScanForHighlight();

            // Legacy Input System readings
            if (Input.GetKeyDown(KeyCode.Alpha1)) { activeSlotIndex = 0; UpdateUI(); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { activeSlotIndex = 1; UpdateUI(); }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _blockLightingUntilRelease = false;
                HandleSpaceAction();
            }

            if (Input.GetKey(KeyCode.Space))
            {
                if (!_blockLightingUntilRelease) HandleHoldAction();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                _blockLightingUntilRelease = false;
                IInteractable target = GetItemInFront();
                if (target is IHoldInteractable holdInteractable)
                {
                    holdInteractable.StopInteractHold(this);
                }
                
                // Safety fallback
                if (BonfireBeingLit != null)
                {
                    BonfireBeingLit.StopLighting();
                    BonfireBeingLit = null;
                }
            }

            if (Input.GetKeyDown(KeyCode.R)) { DropItem(); }
        }

        void ScanForHighlight()
        {
            IInteractable currentItem = GetItemInFront();
            bool canHighlight = false;

            if (currentItem != null)
            {
                canHighlight = currentItem.CanInteract(this);
            }

            if (canHighlight)
            {
                if (currentItem != _lastTargetedItem)
                {
                    if (_lastTargetedItem != null) _lastTargetedItem.SetHighlight(false);
                    currentItem.SetHighlight(true);
                    _lastTargetedItem = currentItem;
                }
            }
            else if (_lastTargetedItem != null)
            {
                _lastTargetedItem.SetHighlight(false);
                _lastTargetedItem = null;
            }
        }

        IInteractable GetItemInFront()
        {
            Collider[] closeColliders = Physics.OverlapSphere(transform.position, interactionDistance, interactionLayer);
            IInteractable bestItem = null;
            float closestAngle = 180f;

            foreach (Collider col in closeColliders)
            {
                IInteractable item = col.GetComponentInParent<IInteractable>();
                if (item == null)
                {
                    continue;
                }

                var interactableObj = item as MonoBehaviour;
                if (interactableObj == null) continue;

                Vector3 directionToItem = interactableObj.transform.position - transform.position;
                directionToItem.y = 0;
                directionToItem = directionToItem.normalized;
                float angle = Vector3.Angle(transform.forward, directionToItem);

                if (angle < 70f)
                {
                    if (angle < closestAngle)
                    {
                        closestAngle = angle;
                        bestItem = item;
                    }
                }
            }

            if (bestItem != null)
            {
                var interactableObj = bestItem as MonoBehaviour;
                if (interactableObj != null)
                {
                    Debug.DrawLine(transform.position + Vector3.up, interactableObj.transform.position, Color.green);
                }
            }

            return bestItem;
        }

        void HandleSpaceAction()
        {
            IInteractable itemInFront = GetItemInFront();
            InventorySlot currentSlot = slots[activeSlotIndex];

            // 1. ANIMATION (If it is an axe)
            if (currentSlot.item != null && currentSlot.item.itemName.ToLower() == "axe")
            {
                if (playerAnimator != null) playerAnimator.SetTrigger("Chop");
            }

            if (itemInFront == null) return;

            // If it is a tree, the swing hit is driven by the animation event "EjecutarGolpeTala"
            if (itemInFront is TikiToki.Gameplay.Environment.Tree)
            {
                return;
            }

            // 2. INTERACTION
            if (itemInFront.CanInteract(this))
            {
                itemInFront.Interact(this);
            }
        }

        void HandleHoldAction()
        {
            IInteractable target = GetItemInFront();
            if (target is IHoldInteractable holdInteractable)
            {
                if (holdInteractable.CanInteract(this))
                {
                    holdInteractable.InteractHold(this, Time.deltaTime);
                }
            }
        }

        public void ConsumeSlot(InventorySlot slot, TikiToki.Gameplay.Environment.Bonfire bonfire)
        {
            _blockLightingUntilRelease = true;
            slot.count--;
            if (slot.count <= 0) slot.item = null;
            bonfire.UpdateVisuals();
            UpdateUI();
        }

        public void EjecutarGolpeTala()
        {
            IInteractable itemInFront = GetItemInFront();
            if (itemInFront is TikiToki.Gameplay.Environment.Tree tree)
            {
                tree.TakeHit();
            }
        }

        public void FinishPickup(WorldItem item)
        {
            if (pickupSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play3DSound(pickupSound, transform.position);
            }

            OnItemCollected?.Invoke(item.itemData.itemName);
            item.SetHighlight(false);
            Destroy(item.gameObject);
            _lastTargetedItem = null;
            UpdateUI();
        }

        public void DropItem()
        {
            InventorySlot currentSlot = slots[activeSlotIndex];
            if (currentSlot.item != null)
            {
                if (dropSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play3DSound(dropSound, transform.position);
                }
                Vector3 spawnPos = transform.position + (transform.forward * 1.2f);
                spawnPos.y = 1.1f;
                GameObject droppedObj = Instantiate(currentSlot.item.prefab, spawnPos, Quaternion.identity);
                Rigidbody[] rbs = droppedObj.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rbs) 
                { 
                    rb.isKinematic = false; 
                    rb.useGravity = true; 
                }
                OnItemDropped?.Invoke();
                currentSlot.count--;
                if (currentSlot.count <= 0) currentSlot.item = null;
                UpdateUI();
            }
        }

        void UpdateUI()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slotBackgrounds.Length > i)
                    slotBackgrounds[i].color = (i == activeSlotIndex) ? selectedColor : frameColor;
                bool hasItem = slots[i].item != null;
                iconSlots[i].enabled = hasItem;
                if (hasItem)
                {
                    iconSlots[i].sprite = slots[i].item.icon;
                    stackTexts[i].text = slots[i].count > 1 ? slots[i].count.ToString() : "";
                }
                else stackTexts[i].text = "";
            }
            UpdateHandVisual();
        }

        void UpdateHandVisual()
        {
            if (_currentHeldObject != null) Destroy(_currentHeldObject);
            ItemData currentItem = slots[activeSlotIndex].item;
            if (currentItem != null && currentItem.prefab != null && holdPoint != null)
            {
                _currentHeldObject = Instantiate(currentItem.prefab, holdPoint.position, holdPoint.rotation, holdPoint);
                if (currentItem.itemName.ToLower() == "axe") _currentHeldObject.transform.localEulerAngles = new Vector3(0f, 0f, -41.772f);
                foreach (Collider c in _currentHeldObject.GetComponentsInChildren<Collider>()) c.enabled = false;
                foreach (Rigidbody rb in _currentHeldObject.GetComponentsInChildren<Rigidbody>()) rb.isKinematic = true;
                foreach (WorldItem wi in _currentHeldObject.GetComponentsInChildren<WorldItem>()) Destroy(wi);
            }
        }
    }
}
