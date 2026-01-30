using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;
    private MeshRenderer[] renderers;
    private Vector3 originalScale;

    [Header("Ajustes Visuales")]
    [ColorUsage(true, true)]
    public Color highlightColor = new Color(0.5f, 0.5f, 0.5f);
    public float scaleMultiplier = 1.05f; // Se hace un 10% más grande

    void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        originalScale = transform.localScale;
    }

    public void SetHighlight(bool state)
    {
        // 1. Efecto de Brillo (Sin borrar texturas)
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

        // 2. Efecto de Escala (El "Pop")
        transform.localScale = state ? originalScale * scaleMultiplier : originalScale;
    }
}