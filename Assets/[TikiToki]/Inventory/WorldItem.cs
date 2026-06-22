using UnityEngine;
using System.Collections;
using TikiToki.Gameplay;

namespace TikiToki.Inventory
{
    public class WorldItem : MonoBehaviour, IInteractable
    {
        public ItemData itemData;

        [Header("Smart Despawn Settings")]
        public bool canDespawn = true;
        public float lifeTime = 20f;
        public float shrinkDuration = 1.5f;

        [Header("Anti-Fall Fix")]
        public float minY = -1f;
        public Vector3 respawnPosition = new Vector3(0f, 1f, 0f);

        private float _timer;
        private bool _isShrinking = false;
        private PlayerInventory _cachedInv;

        [Header("Visual Settings")]
        private MeshRenderer[] renderers;
        [ColorUsage(true, true)] public Color highlightColor = new Color(0.5f, 0.5f, 0.5f);
        private Vector3 _originalScale;

        // --- IInteractable Implementation ---
        public string InteractionPrompt => itemData != null ? itemData.itemName : "Item";

        public bool CanInteract(PlayerInventory inventory)
        {
            if (itemData == null) return false;

            for (int i = 0; i < inventory.slots.Length; i++)
            {
                if (inventory.slots[i].item == itemData && inventory.slots[i].count < itemData.maxStack)
                {
                    return true;
                }
                if (inventory.slots[i].item == null)
                {
                    return true;
                }
            }
            return false;
        }

        public void Interact(PlayerInventory inventory)
        {
            if (itemData == null) return;

            for (int i = 0; i < inventory.slots.Length; i++)
            {
                if (inventory.slots[i].item == itemData && inventory.slots[i].count < itemData.maxStack)
                {
                    inventory.slots[i].count++;
                    inventory.FinishPickup(this);
                    return;
                }
            }
            for (int i = 0; i < inventory.slots.Length; i++)
            {
                if (inventory.slots[i].item == null)
                {
                    inventory.slots[i].item = itemData;
                    inventory.slots[i].count = 1;
                    inventory.FinishPickup(this);
                    return;
                }
            }
        }

        void Awake()
        {
            renderers = GetComponentsInChildren<MeshRenderer>();
            _originalScale = transform.localScale;
        }

        void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) _cachedInv = player.GetComponent<PlayerInventory>();
        }

        void Update()
        {
            if (transform.position.y < minY)
            {
                transform.position = respawnPosition;
            }

            if (!canDespawn || _isShrinking || itemData == null) return;

            if (IsItemSurplus())
            {
                _timer += Time.deltaTime;
                if (_timer >= (lifeTime - shrinkDuration))
                {
                    _isShrinking = true;
                    StartCoroutine(ShrinkAndDestroy());
                }
            }
            else
            {
                _timer = 0;
            }
        }

        bool IsItemSurplus()
        {
            if (_cachedInv == null) return false;

            bool playerAlreadyHasThisItem = false;
            bool hasSpaceInExistingStack = false;
            bool hasEmptySlot = false;

            foreach (var slot in _cachedInv.slots)
            {
                if (slot.item == null) hasEmptySlot = true;
                else if (slot.item == itemData)
                {
                    playerAlreadyHasThisItem = true;
                    if (slot.count < itemData.maxStack) hasSpaceInExistingStack = true;
                }
            }

            if (hasEmptySlot) return false;
            if (hasSpaceInExistingStack) return false;
            if (!playerAlreadyHasThisItem) return false;

            return true;
        }

        IEnumerator ShrinkAndDestroy()
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0;
            while (elapsed < shrinkDuration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / shrinkDuration);
                yield return null;
            }
            Destroy(gameObject);
        }

        public void SetHighlight(bool state)
        {
            if (_isShrinking) return;

            foreach (var ren in renderers)
            {
                Material[] mats = ren.materials;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (state)
                    {
                        mats[i].EnableKeyword("_EMISSION");
                        mats[i].SetColor("_EmissionColor", highlightColor);
                    }
                    else
                    {
                        mats[i].SetColor("_EmissionColor", Color.black);
                    }
                }

                ren.materials = mats;
            }

            if (!CompareTag("Tree"))
            {
                transform.localScale = state ? _originalScale * 1.1f : _originalScale;
            }
        }
    }
}
