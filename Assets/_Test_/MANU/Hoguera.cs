using UnityEngine;

public class Hoguera : MonoBehaviour
{
    [Header("Estado")]
    public bool estaEncendida = false;
    public bool tieneMadera = false;
    public bool tieneHojas = false;

    [Header("Referencias Visuales")]
    public GameObject efectosFuego; // El fuego (partículas/luz)
    public GameObject modeloMadera; // Troncos visuales en la base
    public GameObject modeloHojas;  // Hojas visuales en la base

    void Start()
    {
        ActualizarVisuales();
        if (Barra.Instance != null) Barra.Instance.RegistrarHoguera(this);
    }

    public void ActualizarVisuales()
    {
        if (efectosFuego != null) efectosFuego.SetActive(estaEncendida);
        if (modeloMadera != null) modeloMadera.SetActive(tieneMadera);
        if (modeloHojas != null) modeloHojas.SetActive(tieneHojas);
    }

    // Método para intentar encenderla
    public void IntentarEncender()
    {
        if (estaEncendida) return;

        if (tieneMadera && tieneHojas)
        {
            estaEncendida = true;
            ActualizarVisuales();
            Debug.Log("<color=orange>HOGUERA:</color> ¡Encendida!");

            if (Barra.Instance != null)
                Barra.Instance.RecalcularTasaDeCambio();
        }
        else
        {
            Debug.Log("<color=yellow>HOGUERA:</color> Faltan ingredientes. Madera: " + tieneMadera + " Hojas: " + tieneHojas);
        }
    }
}