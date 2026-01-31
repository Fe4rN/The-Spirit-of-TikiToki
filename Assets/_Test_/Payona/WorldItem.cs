using UnityEngine;
using System.Collections;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;

    [Header("Ajustes de Despawn Inteligente")]
    public bool canDespawn = true;
    public float lifeTime = 20f;
    public float shrinkDuration = 1.5f;

    private float _timer;
    private bool _isShrinking = false;
    private PlayerInventory _cachedInv;

    [Header("Ajustes Visuales")]
    private MeshRenderer[] renderers;
    [ColorUsage(true, true)] public Color highlightColor = new Color(0.5f, 0.5f, 0.5f);
    private Vector3 _originalScale;

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
        if (!canDespawn || _isShrinking || itemData == null) return;

        // --- NUEVA LÓGICA: ¿ESTE OBJETO REALMENTE SOBRA? ---
        if (IsItemSurplus())
        {
            // Si el jugador ya tiene el máximo de este item, corre el reloj
            _timer += Time.deltaTime;

            if (_timer >= (lifeTime - shrinkDuration))
            {
                _isShrinking = true;
                StartCoroutine(ShrinkAndDestroy());
            }
        }
        else
        {
            // Si el jugador NO tiene este item, o tiene hueco para él, NO desaparece
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
            if (slot.item == null)
            {
                hasEmptySlot = true;
            }
            else if (slot.item == itemData)
            {
                playerAlreadyHasThisItem = true;
                if (slot.count < itemData.maxStack)
                {
                    hasSpaceInExistingStack = true;
                }
            }
        }

        // LÓGICA DE DECISIÓN:
        // 1. Si hay un hueco vacío en el inventario -> NO SOBRA
        if (hasEmptySlot) return false;

        // 2. Si el jugador ya tiene el item pero hay hueco en el montón (stack) -> NO SOBRA
        if (hasSpaceInExistingStack) return false;

        // 3. SI EL JUGADOR NO TIENE ESTE ITEM EN ABSOLUTO -> NO SOBRA (Aunque esté lleno de otras cosas)
        if (!playerAlreadyHasThisItem) return false;

        // EN CUALQUIER OTRO CASO (Inventario lleno y ya tengo el máximo de este item) -> SOBRA
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
            if (state)
            {
                ren.material.EnableKeyword("_EMISSION");
                ren.material.SetColor("_EmissionColor", highlightColor);
            }
            else
            {
                ren.material.SetColor("_EmissionColor", Color.black);
            }
        }
        transform.localScale = state ? _originalScale * 1.1f : _originalScale;
    }
}