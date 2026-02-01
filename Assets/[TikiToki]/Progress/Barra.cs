using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Barra : MonoBehaviour
{
    public static Barra Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private Slider barraProgreso;

    [Header("Ajustes de Tiempo")]
    [Range(0.1f, 5f)] public float intervaloDeTiempo = 1.0f;
    private float cronometro = 0f;

    [Header("Tasas de Cambio (Puntos por Salto)")]
    public float puntosLentos = 2f;
    public float puntosMedios = 5f;
    public float puntosRapidos = 10f;

    [Header("Tasas de Subida (Más rápido)")]
    public float puntosLentosSubida = 4f;
    public float puntosMediaosSubida = 10f;
    public float puntosRapidosSubida = 20f;

    [Header("Estado del Progreso")]
    public float progresoActual = 50f;
    private float puntosAAplicar = 0f;

    private List<Hoguera> todasLasHogueras = new List<Hoguera>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (barraProgreso != null)
        {
            barraProgreso.minValue = 0;
            barraProgreso.maxValue = 100;
            barraProgreso.value = progresoActual;
        }
        // Forzamos un c�lculo inicial
        RecalcularTasaDeCambio();
    }

    private void Update()
    {
        cronometro += Time.deltaTime;

        if (cronometro >= intervaloDeTiempo)
        {
            AplicarCambioDePuntos();
            cronometro = 0f;
        }
    }

    public void RegistrarHoguera(Hoguera h)
    {
        if (!todasLasHogueras.Contains(h))
        {
            todasLasHogueras.Add(h);
        }
        RecalcularTasaDeCambio();
    }

    public void RecalcularTasaDeCambio()
    {
        int encendidas = 0;
        int apagadas = 0;

        foreach (var h in todasLasHogueras)
        {
            if (h == null) continue;
            if (h.estaEncendida) encendidas++; else apagadas++;
        }

        int diferencia = encendidas - apagadas;
        int diferenciaAbsoluta = Mathf.Abs(diferencia);

        // Asignación de puntos (diferentes según subida o bajada)
        if (diferencia > 0)
        {
            // SUBIDA: usa tasas rápidas
            if (diferenciaAbsoluta == 1) puntosAAplicar = puntosLentosSubida;
            else if (diferenciaAbsoluta == 3) puntosAAplicar = puntosMediaosSubida;
            else if (diferenciaAbsoluta == 5) puntosAAplicar = puntosRapidosSubida;
            else puntosAAplicar = 0;
        }
        else if (diferencia < 0)
        {
            // BAJADA: usa tasas normales (negativas)
            if (diferenciaAbsoluta == 1) puntosAAplicar = -puntosLentos;
            else if (diferenciaAbsoluta == 3) puntosAAplicar = -puntosMedios;
            else if (diferenciaAbsoluta == 5) puntosAAplicar = -puntosRapidos;
            else puntosAAplicar = 0;
        }
        else
        {
            // EQUILIBRIO: sin cambio
            puntosAAplicar = 0;
        }
    }

    private void AplicarCambioDePuntos()
    {
        if (puntosAAplicar == 0) return;

        progresoActual += puntosAAplicar;
        progresoActual = Mathf.Clamp(progresoActual, 0, 100);

        if (barraProgreso != null)
            barraProgreso.value = progresoActual;

        // Avisamos al �rbitro WinLose del nuevo valor de la barra
        if (WinLose.Instance != null)
        {
            WinLose.Instance.ValidarProgreso(progresoActual);
        }
    }
}