using UnityEngine;

public class Hoguera : MonoBehaviour
{
    [Header("Estado")]
    public bool estaEncendida = false;
    public bool tieneMadera = false;
    public bool tieneHojas = false;

    [Header("Progreso de Encendido")]
    public float progresoActual = 0f;
    public float tiempoNecesario = 2.5f; // Segundos manteniendo espacio
    public ParticleSystem sistemaChispas; // Arrastra aquí tus partículas
    public TreeHealthBarTree barraProgreso; // Reutilizamos el script de la barra del árbol

    [Header("Referencias Visuales")]
    public GameObject efectosFuego;
    public GameObject modeloMadera;
    public GameObject modeloHojas;

    void Start()
    {
        ActualizarVisuales();
        if (barraProgreso != null) barraProgreso.gameObject.SetActive(false);
        if (sistemaChispas != null) sistemaChispas.Stop();

        // ESTO ES LO QUE FALTA:
        if (Barra.Instance != null) Barra.Instance.RegistrarHoguera(this);
    }

    public void ActualizarVisuales()
    {
        if (efectosFuego != null) efectosFuego.SetActive(estaEncendida);
        if (modeloMadera != null) modeloMadera.SetActive(tieneMadera);
        if (modeloHojas != null) modeloHojas.SetActive(tieneHojas);
    }

    public void IntentarEncender(float incremento)
    {
        if (estaEncendida || !tieneMadera || !tieneHojas) return;

        // Activar efectos si no estaban ya
        if (barraProgreso != null && !barraProgreso.gameObject.activeSelf) barraProgreso.gameObject.SetActive(true);
        if (sistemaChispas != null && !sistemaChispas.isPlaying) sistemaChispas.Play();

        progresoActual += incremento;

        if (barraProgreso != null)
            barraProgreso.SetHealth(progresoActual, tiempoNecesario);

        if (progresoActual >= tiempoNecesario)
        {
            FinalizarEncendido();
        }
    }

    public void DetenerEncendido()
    {
        if (estaEncendida) return;

        progresoActual = 0f; // Resetear si sueltas (o puedes hacer que baje lento)
        if (barraProgreso != null) barraProgreso.gameObject.SetActive(false);
        if (sistemaChispas != null) sistemaChispas.Stop();
    }

    void FinalizarEncendido()
    {
        estaEncendida = true;
        if (sistemaChispas != null) sistemaChispas.Stop();
        if (barraProgreso != null) barraProgreso.gameObject.SetActive(false);

        ActualizarVisuales();

        if (Barra.Instance != null)
            Barra.Instance.RecalcularTasaDeCambio();
    }
}