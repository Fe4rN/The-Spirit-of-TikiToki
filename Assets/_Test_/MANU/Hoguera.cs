using UnityEngine;

public class Hoguera : MonoBehaviour
{
    public bool estaEncendida = true;
    public GameObject efectosVisuales;

    void Start()
    {
        if (efectosVisuales != null) efectosVisuales.SetActive(estaEncendida);
        if (Barra.Instance != null) Barra.Instance.RegistrarHoguera(this);
    }

    // Este método se activa cuando cambias algo en el Inspector manualmente
    void OnValidate()
    {
        if (Application.isPlaying && Barra.Instance != null)
        {
            if (efectosVisuales != null) efectosVisuales.SetActive(estaEncendida);
            Barra.Instance.RecalcularTasaDeCambio();
        }
    }

    public void AlternarEstado()
    {
        estaEncendida = !estaEncendida;
        if (efectosVisuales != null) efectosVisuales.SetActive(estaEncendida);

        if (Barra.Instance != null)
            Barra.Instance.RecalcularTasaDeCambio();
    }
}