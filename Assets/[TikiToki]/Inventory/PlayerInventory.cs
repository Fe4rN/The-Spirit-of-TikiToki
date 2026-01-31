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
    public Image[] slotBackgrounds;
    public Image[] iconSlots;
    public TextMeshProUGUI[] stackTexts;

    [Header("Visualización en Mano")]
    public Transform holdPoint;
    private GameObject _currentHeldObject;

    [Header("Interacción")]
    public float interactionDistance = 1.2f;
    public LayerMask interactionLayer;
    public float raySpread = 15f;

    private WorldItem _lastTargetedItem;
    private Hoguera _hogueraSiendoEncendida;
    private bool _bloquearEncendidoHastaSoltar = false;

    // --- COLORES ---
    private Color frameColor = new Color(0.494f, 0.494f, 0.494f, 1f); // Gris #7E7E7E
    private Color selectedColor = new Color(1f, 0.92f, 0.016f, 1f);   // Amarillo brillante

    void Start() { UpdateUI(); }

    void Update()
    {
        // --- VISUALIZACIÓN CONSTANTE DE RAYOS (No tocar) ---
        Vector3 origin = transform.position + Vector3.up * -0.95f;
        Debug.DrawRay(origin, transform.forward * interactionDistance, Color.cyan);
        Debug.DrawRay(origin, (Quaternion.Euler(0, -raySpread, 0) * transform.forward) * interactionDistance, Color.cyan);
        Debug.DrawRay(origin, (Quaternion.Euler(0, raySpread, 0) * transform.forward) * interactionDistance, Color.cyan);
        Debug.DrawRay(origin, (Quaternion.Euler(0, -raySpread * 2f, 0) * transform.forward) * interactionDistance, Color.cyan);
        Debug.DrawRay(origin, (Quaternion.Euler(0, raySpread * 2f, 0) * transform.forward) * interactionDistance, Color.cyan);

        ScanForHighlight();

        if (Input.GetKeyDown(KeyCode.Alpha1)) { activeSlotIndex = 0; UpdateUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { activeSlotIndex = 1; UpdateUI(); }

        // --- LÓGICA DE ESPACIO (Diferenciada) ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _bloquearEncendidoHastaSoltar = false;
            HandleSpaceAction(); // Solo para poner madera/hojas o recoger
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (!_bloquearEncendidoHastaSoltar)
            {
                HandleHoldAction();
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            _bloquearEncendidoHastaSoltar = false;

            if (_hogueraSiendoEncendida != null)
            {
                _hogueraSiendoEncendida.DetenerEncendido(); // Importante para apagar chispas al soltar
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
            // 1. ÁRBOLES: Requiere el hacha
            if (currentItem.CompareTag("Tree"))
            {
                if (slots[activeSlotIndex].item != null && slots[activeSlotIndex].item.itemName.ToLower() == "axe")
                {
                    canHighlight = true;
                }
            }
            // 2. HOGUERA: Comprobamos si el item en mano es uno de los materiales necesarios
            else if (currentItem.CompareTag("Bonfire"))
            {
                Hoguera hoguera = currentItem.GetComponentInParent<Hoguera>();
                if (hoguera != null && !hoguera.estaEncendida)
                {
                    // Si la hoguera ya tiene todo, resaltamos para indicar que se puede encender
                    if (hoguera.tieneMadera && hoguera.tieneHojas)
                    {
                        canHighlight = true;
                    }
                    else if (slots[activeSlotIndex].item != null)
                    {
                        string nombreEnMano = slots[activeSlotIndex].item.itemName.ToLower();

                        // Lista de materiales que la hoguera acepta
                        string[] materialesAceptados = { "woodpile", "leavespile" };

                        foreach (string mat in materialesAceptados)
                        {
                            if (nombreEnMano == mat)
                            {
                                // Verificamos cuál falta específicamente
                                if (mat == "woodpile" && !hoguera.tieneMadera) canHighlight = true;
                                if (mat == "leavespile" && !hoguera.tieneHojas) canHighlight = true;
                            }
                        }
                    }
                }
            }
            // 3. OBJETOS SUELTOS: Siempre se pueden resaltar para recoger
            else if (currentItem.itemData != null)
            {
                canHighlight = true;
            }
        }

        // --- APLICAR RESALTADO ---
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

        // --- APLICACIÓN DEL HIGHLIGHT ---
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
        Vector3 origin = transform.position + Vector3.up * -0.95f;
        Vector3[] directions = {
        transform.forward,
        Quaternion.Euler(0, -raySpread, 0) * transform.forward,
        Quaternion.Euler(0, raySpread, 0) * transform.forward,
        Quaternion.Euler(0, -raySpread * 2f, 0) * transform.forward,
        Quaternion.Euler(0, raySpread * 2f, 0) * transform.forward
    };

        foreach (Vector3 dir in directions)
        {
            // Añadimos QueryTriggerInteraction.Ignore para que no detecte triggers invisibles por error
            if (Physics.Raycast(origin, dir, out hit, interactionDistance, interactionLayer, QueryTriggerInteraction.Ignore))
            {
                // CAMBIO CLAVE: Buscamos el script en el objeto golpeado O en cualquier padre superior
                WorldItem item = hit.collider.GetComponentInParent<WorldItem>();

                if (item != null)
                {
                    Debug.DrawRay(origin, dir * interactionDistance, Color.red); // Rayo rojo si detecta item
                    return item;
                }
            }
        }
        return null;
    }

    void HandleHoldAction()
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
        else if (_hogueraSiendoEncendida != null)
        {
            // Si dejamos de mirar la hoguera mientras mantenemos espacio
            _hogueraSiendoEncendida.DetenerEncendido();
            _hogueraSiendoEncendida = null;
        }
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
                    _bloquearEncendidoHastaSoltar = true;
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
                    _bloquearEncendidoHastaSoltar = true;
                    currentSlot.count--;
                    if (currentSlot.count <= 0) currentSlot.item = null;
                    hoguera.ActualizarVisuales();
                    UpdateUI();
                    return; // Salimos
                }

                // C. Si no estás entregando nada útil, o la hoguera ya tiene lo que ofreces, intentamos encender.
                if (!hoguera.estaEncendida)
                {
                    Debug.Log("<color=orange>HOGUERA:</color> Mantén ESPACIO para intentar encender.");
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
            // Guardamos el nombre antes de que el slot pueda quedar nulo
            string nameToCheck = currentSlot.item.itemName;

            Debug.Log("<color=orange>SOLTANDO:</color> " + nameToCheck);
            Vector3 spawnPos = transform.position + (transform.forward * 1.2f);
            spawnPos.y = 1.1f; // Altura desde la que cae

            // Instanciamos el objeto y guardamos su referencia
            GameObject droppedObj = Instantiate(currentSlot.item.prefab, spawnPos, Quaternion.identity);

            // Buscamos todos los Rigidbodys en el objeto soltado (hijos incluidos)
            Rigidbody[] rbs = droppedObj.GetComponentsInChildren<Rigidbody>();

            if (nameToCheck == "Axe")
            {
                // Si es el hacha, activamos Kinematic (se queda quieta en el aire/suelo)
                foreach (Rigidbody rb in rbs)
                {
                    rb.isKinematic = false;
                }
            }
            else
            {
                // Si es madera o cualquier otra cosa, desactivamos Kinematic para que caiga
                foreach (Rigidbody rb in rbs)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }

            // Restamos la cantidad del inventario
            currentSlot.count--;
            if (currentSlot.count <= 0) currentSlot.item = null;

            UpdateUI();
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            // 1. EL RECUADRO: Cambia de color según si es el slot activo o no
            if (slotBackgrounds.Length > i)
            {
                slotBackgrounds[i].enabled = true;

                // Si el índice coincide con el slot activo, lo ponemos amarillo
                if (i == activeSlotIndex)
                {
                    slotBackgrounds[i].color = selectedColor;
                }
                else
                {
                    slotBackgrounds[i].color = frameColor;
                }
            }

            // 2. EL ICONO: Solo visible si hay item
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

            // --- AJUSTE DE ROTACIÓN ESPECÍFICA PARA EL HACHA ---
            if (currentItem.itemName == "Axe")
            {
                // Aplicamos la rotación local para que sea relativa a la mano
                _currentHeldObject.transform.localEulerAngles = new Vector3(0f, 0f, -41.772f);
            }

            // --- SOLUCIÓN PARA LA CAÍDA ---
            // Desactivamos TODOS los colliders que pueda tener el modelo (en el padre y en los hijos)
            Collider[] colliders = _currentHeldObject.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders) c.enabled = false;

            // Ponemos TODOS los Rigidbodys en modo Kinematic para que no les afecte la gravedad
            Rigidbody[] rbs = _currentHeldObject.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                rb.isKinematic = true;
                rb.useGravity = false; // Por seguridad extra, quitamos la gravedad
            }

            // Quitamos cualquier script de WorldItem de los hijos para que el raycast no se detecte a sí mismo
            WorldItem[] worldItems = _currentHeldObject.GetComponentsInChildren<WorldItem>();
            foreach (WorldItem wi in worldItems) Destroy(wi);
        }
    }
}