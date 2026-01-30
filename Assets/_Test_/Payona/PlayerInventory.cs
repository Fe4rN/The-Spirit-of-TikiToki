using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;
    }

    [Header("Configuración")]
    public InventorySlot[] slots = new InventorySlot[2];
    public int activeSlotIndex = 0;

    [Header("Referencias UI")]
    public Image[] iconSlots;
    public TextMeshProUGUI[] stackTexts;
    public GameObject[] selectors;

    [Header("Visualización en Mano")]
    public Transform holdPoint; // Arrastra aquí un objeto vacío hijo de las manos del jugador
    private GameObject _currentHeldObject;

    [Header("Interacción")]
    public float interactionDistance = 1.2f;
    public LayerMask interactionLayer;
    public float raySpread = 15f;

    private WorldItem _lastTargetedItem;

    void Start() { UpdateUI(); }

    void Update()
    {
        // --- VISUALIZACIÓN CONSTANTE DE 5 RAYOS ---
        Vector3 origin = transform.position + Vector3.up * -0.9f;
        Debug.DrawRay(origin, transform.forward * interactionDistance, Color.cyan);
        Debug.DrawRay(origin, (Quaternion.Euler(0, -raySpread, 0) * transform.forward) * interactionDistance, Color.cyan);
        Debug.DrawRay(origin, (Quaternion.Euler(0, raySpread, 0) * transform.forward) * interactionDistance, Color.cyan);
        Debug.DrawRay(origin, (Quaternion.Euler(0, -raySpread * 2f, 0) * transform.forward) * interactionDistance, Color.cyan);
        Debug.DrawRay(origin, (Quaternion.Euler(0, raySpread * 2f, 0) * transform.forward) * interactionDistance, Color.cyan);

        ScanForHighlight();

        if (Input.GetKeyDown(KeyCode.Alpha1)) { activeSlotIndex = 0; UpdateUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { activeSlotIndex = 1; UpdateUI(); }

        if (Input.GetKeyDown(KeyCode.Space)) { HandleSpaceAction(); }
    }

    void ScanForHighlight()
    {
        WorldItem currentItem = GetItemInFront();
        bool canHighlight = false;

        if (currentItem != null)
        {
            // 1. Si es un ÁRBOL, solo permitimos el highlight si tenemos el hacha
            if (currentItem.CompareTag("Tree"))
            {
                if (slots[activeSlotIndex].item != null && slots[activeSlotIndex].item.itemName == "Axe")
                {
                    canHighlight = true;
                }
            }
            // 2. Si es un OBJETO (madera, hacha en el suelo, etc), siempre resaltamos para poder recogerlo
            else if (currentItem.itemData != null)
            {
                canHighlight = true;
            }
        }

        // Aplicar el highlight según el resultado de arriba
        if (canHighlight)
        {
            if (currentItem != _lastTargetedItem)
            {
                if (_lastTargetedItem != null) _lastTargetedItem.SetHighlight(false);
                currentItem.SetHighlight(true);
                _lastTargetedItem = currentItem;
            }
        }
        else
        {
            // Si no se puede resaltar (miramos un árbol sin hacha o no miramos nada)
            if (_lastTargetedItem != null)
            {
                _lastTargetedItem.SetHighlight(false);
                _lastTargetedItem = null;
            }
        }
    }

    WorldItem GetItemInFront()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * -0.9f;
        Vector3[] directions = {
            transform.forward,
            Quaternion.Euler(0, -raySpread, 0) * transform.forward,
            Quaternion.Euler(0, raySpread, 0) * transform.forward,
            Quaternion.Euler(0, -raySpread * 2f, 0) * transform.forward,
            Quaternion.Euler(0, raySpread * 2f, 0) * transform.forward
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(origin, dir, out hit, interactionDistance, interactionLayer))
            {
                return hit.collider.GetComponent<WorldItem>();
            }
        }
        return null;
    }

    void HandleSpaceAction()
    {
        WorldItem itemInFront = GetItemInFront();

        // --- LÓGICA DE RECOGIDA (Busca en todo el inventario) ---
        if (itemInFront != null && itemInFront.itemData != null)
        {
            ItemData data = itemInFront.itemData;

            // 1. Intentar sumar a un stack existente
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == data && slots[i].count < data.maxStack)
                {
                    slots[i].count++;
                    FinishPickup(itemInFront);
                    return;
                }
            }

            // 2. Si no hay stack, buscar el PRIMER hueco vacío disponible
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

            Debug.Log("<color=yellow>SISTEMA:</color> Inventario totalmente lleno.");
            return;
        }

        // --- CASO B: INTERACTUAR (Árbol + Hacha) ---
        if (itemInFront != null && itemInFront.CompareTag("Tree") && slots[activeSlotIndex].item != null && slots[activeSlotIndex].item.itemName == "Axe")
        {
            Tree tree = itemInFront.GetComponent<Tree>();
            if (tree != null)
            {
                Debug.Log("<color=red>ACCION:</color> Golpeando árbol.");
                tree.TakeHit();
                return;
            }
        }

        // --- CASO C: SOLTAR (Si no hay nada que recoger o interactuar delante) ---
        if (slots[activeSlotIndex].item != null)
        {
            DropItem();
        }
    }

    void FinishPickup(WorldItem item)
    {
        Debug.Log("<color=cyan>RECOGIDO:</color> " + item.itemData.itemName);
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
            Debug.Log("<color=orange>SOLTANDO:</color> " + currentSlot.item.itemName);
            Vector3 spawnPos = transform.position + (transform.forward * 1.2f);
            spawnPos.y = 0.1f;
            Instantiate(currentSlot.item.prefab, spawnPos, Quaternion.identity);

            currentSlot.count--;
            if (currentSlot.count <= 0) currentSlot.item = null;

            UpdateUI();
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            bool hasItem = slots[i].item != null;
            iconSlots[i].enabled = hasItem;

            if (hasItem)
            {
                iconSlots[i].sprite = slots[i].item.icon;
                stackTexts[i].text = slots[i].count > 1 ? slots[i].count.ToString() : "";
            }
            else
            {
                stackTexts[i].text = "";
            }

            if (selectors.Length > i) selectors[i].SetActive(i == activeSlotIndex);
        }

        UpdateHandVisual();
    }

    void UpdateHandVisual()
    {
        // 1. Limpiar objeto actual de la mano
        if (_currentHeldObject != null) Destroy(_currentHeldObject);

        // 2. Si el slot activo tiene un item, instanciarlo en el holdPoint
        ItemData currentItem = slots[activeSlotIndex].item;
        if (currentItem != null && currentItem.prefab != null && holdPoint != null)
        {
            _currentHeldObject = Instantiate(currentItem.prefab, holdPoint.position, holdPoint.rotation, holdPoint);

            // Limpieza del clon en mano para que no cause problemas:
            // Desactivamos su collider y Rigidbody si los tiene para que no empujen al jugador
            if (_currentHeldObject.GetComponent<Collider>()) _currentHeldObject.GetComponent<Collider>().enabled = false;
            if (_currentHeldObject.GetComponent<Rigidbody>()) _currentHeldObject.GetComponent<Rigidbody>().isKinematic = true;
            // Quitamos el script WorldItem para que no se auto-detecte
            if (_currentHeldObject.GetComponent<WorldItem>()) Destroy(_currentHeldObject.GetComponent<WorldItem>());
        }
    }
}