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
    public Transform holdPoint;
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

        // Selección de slots (1 y 2)
        if (Input.GetKeyDown(KeyCode.Alpha1)) { activeSlotIndex = 0; UpdateUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { activeSlotIndex = 1; UpdateUI(); }

        // INTERACCIÓN (Espacio)
        if (Input.GetKeyDown(KeyCode.Space)) { HandleSpaceAction(); }

        // SOLTAR (R)
        if (Input.GetKeyDown(KeyCode.R)) { DropItem(); }
    }

    void ScanForHighlight()
    {
        WorldItem currentItem = GetItemInFront();
        bool canHighlight = false;

        if (currentItem != null)
        {
            if (currentItem.CompareTag("Tree"))
            {
                if (slots[activeSlotIndex].item != null && slots[activeSlotIndex].item.itemName == "Axe")
                {
                    canHighlight = true;
                }
            }
            else if (currentItem.itemData != null)
            {
                canHighlight = true;
            }
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
        else
        {
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
                return hit.collider.GetComponentInParent<WorldItem>();
            }
        }
        return null;
    }

    void HandleSpaceAction()
    {
        WorldItem itemInFront = GetItemInFront();
        InventorySlot currentSlot = slots[activeSlotIndex];

        // --- DEBUG INICIAL: ¿Qué estamos mirando? ---
        if (itemInFront != null)
            Debug.Log("<color=white>RAYCAST HIT:</color> Detectado " + itemInFront.name + " con Tag: " + itemInFront.tag);
        else
            Debug.Log("<color=gray>RAYCAST HIT:</color> No detecto nada frente al jugador.");

        // --- 1. INTERACCIÓN CON HOGUERA ---
        if (itemInFront != null && itemInFront.CompareTag("Bonfire"))
        {
            Hoguera hoguera = itemInFront.GetComponentInParent<Hoguera>(); // Buscamos en el padre por si acaso
            if (hoguera != null)
            {
                string itemEnMano = (currentSlot.item != null) ? currentSlot.item.itemName : "Vacio";
                Debug.Log("<color=yellow>HOGUERA:</color> Mirando hoguera con " + itemEnMano + " en mano.");

                // A. Si tienes MADERA y la hoguera la necesita: se la damos.
                if (currentSlot.item != null && currentSlot.item.itemName == "woodPile" && !hoguera.tieneMadera)
                {
                    Debug.Log("<color=brown>HOGUERA:</color> Entregando madera.");
                    hoguera.tieneMadera = true;
                    currentSlot.count--;
                    if (currentSlot.count <= 0) currentSlot.item = null;
                    hoguera.ActualizarVisuales();
                    UpdateUI();
                    return; // Salimos para que no intente encenderla en el mismo frame
                }

                // B. Si tienes HOJAS y la hoguera las necesita: se la damos.
                if (currentSlot.item != null && currentSlot.item.itemName == "leavesPile" && !hoguera.tieneHojas)
                {
                    Debug.Log("<color=green>HOGUERA:</color> Entregando hojas.");
                    hoguera.tieneHojas = true;
                    currentSlot.count--;
                    if (currentSlot.count <= 0) currentSlot.item = null;
                    hoguera.ActualizarVisuales();
                    UpdateUI();
                    return; // Salimos
                }

                // C. Si no estás entregando nada útil, o la hoguera ya tiene lo que ofreces, intentamos encender.
                if (!hoguera.estaEncendida)
                {
                    Debug.Log("<color=orange>HOGUERA:</color> Intentando encender con lo que hay...");
                    hoguera.IntentarEncender();
                    return;
                }

                // Si ya está encendida, no hacemos nada más con la hoguera
                return;
            }
        }

        // --- 2. LÓGICA DE RECOGIDA ---
        if (itemInFront != null && itemInFront.itemData != null)
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
            return;
        }

        // --- 3. LÓGICA DE TALAR ---
        if (itemInFront != null && itemInFront.CompareTag("Tree"))
        {
            if (currentSlot.item != null && currentSlot.item.itemName == "Axe")
            {
                Tree tree = itemInFront.GetComponent<Tree>();
                if (tree != null) { tree.TakeHit(); return; }
            }
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
            spawnPos.y = 1.5f;
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
        if (_currentHeldObject != null) Destroy(_currentHeldObject);

        ItemData currentItem = slots[activeSlotIndex].item;
        if (currentItem != null && currentItem.prefab != null && holdPoint != null)
        {
            _currentHeldObject = Instantiate(currentItem.prefab, holdPoint.position, holdPoint.rotation, holdPoint);

            if (_currentHeldObject.GetComponent<Collider>()) _currentHeldObject.GetComponent<Collider>().enabled = false;
            if (_currentHeldObject.GetComponent<Rigidbody>()) _currentHeldObject.GetComponent<Rigidbody>().isKinematic = true;
            if (_currentHeldObject.GetComponent<WorldItem>()) Destroy(_currentHeldObject.GetComponent<WorldItem>());
        }
    }
}