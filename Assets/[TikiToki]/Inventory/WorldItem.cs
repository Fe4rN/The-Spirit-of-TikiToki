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

    // --- VERSIÓN CORREGIDA PARA MÚLTIPLES MATERIALES ---
    public void SetHighlight(bool state)
    {
        if (_isShrinking) return;

        foreach (var ren in renderers)
        {
            // Importante: ren.materials devuelve una COPIA del array. 
            // Debemos modificar la copia y volver a asignarla.
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
                    // Al apagar, solemos poner el color en negro
                    mats[i].SetColor("_EmissionColor", Color.black);
                    // Opcional: mats[i].DisableKeyword("_EMISSION");
                }
            }

            // RE-ASIGNACIÓN: Sin esto, los cambios no se ven en el modelo
            ren.materials = mats;
        }

        // Efecto visual de escala
        transform.localScale = state ? _originalScale * 1.1f : _originalScale;
    }
}