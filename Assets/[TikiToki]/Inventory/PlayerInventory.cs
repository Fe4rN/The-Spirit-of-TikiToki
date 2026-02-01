using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;
    }

    [Header("Animaciones")]
    public Animator playerAnimator;

    public static Action<string> OnItemCollected;
    public static Action<string> OnBonfireMaterialAdded;
    public static Action OnItemDropped;

    [Header("Configuración")]
    public InventorySlot[] slots = new InventorySlot[2];
    public int activeSlotIndex = 0;

    [Header("Referencias UI")]
    public Image[] slotBackgrounds;
    public Image[] iconSlots;
    public TextMeshProUGUI[] stackTexts;

    [Header("Visualización en Mano")]
    public Transform holdPoint;
    private GameObject _currentHeldObject;

    [Header("Interacción")]
    public float interactionDistance = 1.5f;
    public LayerMask interactionLayer;

    private WorldItem _lastTargetedItem;
    private Hoguera _hogueraSiendoEncendida;
    private bool _bloquearEncendidoHastaSoltar = false;

    private Color frameColor = new Color(0.494f, 0.494f, 0.494f, 1f);
    private Color selectedColor = new Color(1f, 0.92f, 0.016f, 1f);

    [Header("Sonidos")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip dropSound;

    void Start() { UpdateUI(); }

    void Update()
    {
        ScanForHighlight();

        if (Input.GetKeyDown(KeyCode.Alpha1)) { activeSlotIndex = 0; UpdateUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { activeSlotIndex = 1; UpdateUI(); }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _bloquearEncendidoHastaSoltar = false;
            HandleSpaceAction();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (!_bloquearEncendidoHastaSoltar) HandleHoldAction();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _bloquearEncendidoHastaSoltar = false;
            if (_hogueraSiendoEncendida != null)
            {
                _hogueraSiendoEncendida.DetenerEncendido();
                _hogueraSiendoEncendida = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) { DropItem(); }
    }

    void ScanForHighlight()
    {
        WorldItem currentItem = GetItemInFront();
        bool canHighlight = false;

        if (currentItem != null)
        {
            // PRIORIDAD 1: OBJETOS RECOGIBLES (Hojas, madera suelta, etc.)
            if (currentItem.itemData != null)
            {
                canHighlight = true;
            }
            // PRIORIDAD 2: ÁRBOLES (Solo si tenemos el hacha)
            else if (currentItem.CompareTag("Tree"))
            {
                if (slots[activeSlotIndex].item != null && slots[activeSlotIndex].item.itemName.ToLower() == "axe")
                    canHighlight = true;
            }
            // PRIORIDAD 3: HOGUERA
            else if (currentItem.CompareTag("Bonfire"))
            {
                Hoguera hoguera = currentItem.GetComponentInParent<Hoguera>();
                if (hoguera != null && !hoguera.estaEncendida)
                {
                    if (hoguera.tieneMadera && hoguera.tieneHojas) canHighlight = true;
                    else if (slots[activeSlotIndex].item != null)
                    {
                        string nombreEnMano = slots[activeSlotIndex].item.itemName.ToLower();
                        if (nombreEnMano == "woodpile" && !hoguera.tieneMadera) canHighlight = true;
                        if (nombreEnMano == "leavespile" && !hoguera.tieneHojas) canHighlight = true;
                    }
                }
            }
        }

        // Aplicar el resaltado sin duplicaciones
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

    WorldItem GetItemInFront()
    {
        Collider[] closeColliders = Physics.OverlapSphere(transform.position, interactionDistance, interactionLayer);
        WorldItem bestItem = null;
        float closestAngle = 180f;

        foreach (Collider col in closeColliders)
        {

            WorldItem item = col.GetComponentInParent<WorldItem>();
            if (item == null)
            {
                Debug.Log($"<color=orange>AVISO:</color> Detectado collider {col.name} pero no tiene WorldItem en padres.");
                continue;
            }

            Vector3 directionToItem = (item.transform.position - transform.position).normalized;
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
            Debug.DrawLine(transform.position + Vector3.up, bestItem.transform.position, Color.green);

        return bestItem;
    }

    void HandleSpaceAction()
    {
        WorldItem itemInFront = GetItemInFront();
        InventorySlot currentSlot = slots[activeSlotIndex];

        // 1. ANIMACIÓN (Si es hacha)
        if (currentSlot.item != null && currentSlot.item.itemName.ToLower() == "axe")
        {
            if (playerAnimator != null) playerAnimator.SetTrigger("Chop");
        }

        if (itemInFront == null) return;

        // 2. INTERACCIÓN HOGUERA
        if (itemInFront.CompareTag("Bonfire"))
        {
            Hoguera hoguera = itemInFront.GetComponentInParent<Hoguera>();
            if (hoguera != null && currentSlot.item != null)
            {
                string hand = currentSlot.item.itemName.ToLower();
                if (hand == "woodpile" && !hoguera.tieneMadera)
                {
                    hoguera.tieneMadera = true;
                    OnBonfireMaterialAdded?.Invoke("woodPile");
                    ConsumeSlot(currentSlot, hoguera);
                    return;
                }
                if (hand == "leavespile" && !hoguera.tieneHojas)
                {
                    hoguera.tieneHojas = true;
                    OnBonfireMaterialAdded?.Invoke("leavesPile");
                    ConsumeSlot(currentSlot, hoguera);
                    return;
                }
            }
        }

        // 3. RECOGIDA DE OBJETOS (Aquí se arregla lo de las hojas)
        if (itemInFront.itemData != null)
        {
            ItemData data = itemInFront.itemData;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == data && slots[i].count < data.maxStack)
                {
                    slots[i].count++;
                    FinishPickup(itemInFront);
                    return;
                }
            }
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == null)
                {
                    slots[i].item = data;
                    slots[i].count = 1;
                    FinishPickup(itemInFront);
                    return;
                }
            }
        }
    }

    void ConsumeSlot(InventorySlot slot, Hoguera h)
    {
        _bloquearEncendidoHastaSoltar = true;
        slot.count--;
        if (slot.count <= 0) slot.item = null;
        h.ActualizarVisuales();
        UpdateUI();
    }

    // ESTE MÉTODO ES EL QUE GOLPEA AL ÁRBOL
    public void EjecutarGolpeTala()
    {
        WorldItem itemInFront = GetItemInFront();
        if (itemInFront != null && itemInFront.CompareTag("Tree"))
        {
            Tree tree = itemInFront.GetComponent<Tree>();
            if (tree != null) tree.TakeHit(); // <-- Mira el comentario abajo sobre el tamaño
        }
    }

    void FinishPickup(WorldItem item)
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

    public void HandleHoldAction()
    {
        WorldItem target = GetItemInFront();
        if (target != null && target.CompareTag("Bonfire"))
        {
            Hoguera h = target.GetComponentInParent<Hoguera>();
            if (h != null && !h.estaEncendida && h.tieneMadera && h.tieneHojas)
            {
                _hogueraSiendoEncendida = h;
                h.IntentarEncender(Time.deltaTime);
            }
        }
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
            foreach (Rigidbody rb in rbs) { rb.isKinematic = false; rb.useGravity = true; }
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