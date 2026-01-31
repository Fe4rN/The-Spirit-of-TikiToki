using System;
using UnityEngine;

public class Hoguera : MonoBehaviour
{
    [Header("Estado")]
    public bool estaEncendida = false;
    public bool tieneMadera = false;
    public bool tieneHojas = false;

    [Header("Progreso de Encendido")]
    public float progresoActual = 0f;
    public float tiempoNecesario = 2.5f;
    public float velocidadDescenso = 0.5f; // Velocidad a la que baja el progreso
    public ParticleSystem sistemaChispas;
    public TreeHealthBarTree barraProgreso;

    [Header("Modelos Unificados")]
    public GameObject modeloVacio;
    public GameObject modeloConMadera;
    public GameObject modeloConHojas;
    public GameObject modeloCompleto;

    [Header("Efectos")]
    public GameObject efectosFuego;

    public static Action OnBonfireLit;

    private bool _siendoEncendidaEsteFrame = false;

    void Start()
    {
        ActualizarVisuales();
        if (barraProgreso != null) barraProgreso.gameObject.SetActive(false);
        if (sistemaChispas != null) sistemaChispas.Stop();

        if (Barra.Instance != null) Barra.Instance.RegistrarHoguera(this);
    }

    void Update()
    {
        // Si ya está encendida, no hacemos nada
        if (estaEncendida) return;

        // LÓGICA DE DESCENSO GRADUAL
        // Si hay progreso pero NO estamos manteniendo el espacio (chispas apagadas)
        if (progresoActual > 0 && !_siendoEncendidaEsteFrame)
        {
            progresoActual -= Time.deltaTime * velocidadDescenso;

            if (progresoActual <= 0)
            {
                progresoActual = 0;
                if (barraProgreso != null) barraProgreso.gameObject.SetActive(false);
            }
            else
            {
                // Actualizamos la barra mientras baja
                if (barraProgreso != null)
                {
                    barraProgreso.gameObject.SetActive(true);
                    barraProgreso.SetHealth(progresoActual, tiempoNecesario);
                }
            }
        }

        // Reseteamos el flag para el siguiente frame
        _siendoEncendidaEsteFrame = false;
    }

    public void ActualizarVisuales()
    {
        if (modeloVacio != null) modeloVacio.SetActive(false);
        if (modeloConMadera != null) modeloConMadera.SetActive(false);
        if (modeloConHojas != null) modeloConHojas.SetActive(false);
        if (modeloCompleto != null) modeloCompleto.SetActive(false);

        if (tieneMadera && tieneHojas) modeloCompleto.SetActive(true);
        else if (tieneMadera) modeloConMadera.SetActive(true);
        else if (tieneHojas) modeloConHojas.SetActive(true);
        else modeloVacio.SetActive(true);

        if (efectosFuego != null) efectosFuego.SetActive(estaEncendida);
    }

    public void IntentarEncender(float incremento)
    {
        if (estaEncendida || !tieneMadera || !tieneHojas) return;

        _siendoEncendidaEsteFrame = true; // Avisamos al Update que estamos encendiendo

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
        // Ya no reseteamos progresoActual = 0 aquí.
        // El Update se encargará de bajarlo poco a poco.
        if (sistemaChispas != null) sistemaChispas.Stop();
        _siendoEncendidaEsteFrame = false;
    }

    void FinalizarEncendido()
    {
        estaEncendida = true;
        OnBonfireLit?.Invoke();
        progresoActual = tiempoNecesario;
        if (sistemaChispas != null) sistemaChispas.Stop();
        if (barraProgreso != null) barraProgreso.gameObject.SetActive(false);

        ActualizarVisuales();

        if (Barra.Instance != null)
            Barra.Instance.RecalcularTasaDeCambio();
    }
}