using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Barra : MonoBehaviour
{
    public static Barra Instance { get; private set; }



    [Header("Ajustes de Tiempo")]
    [Range(0.1f, 5f)] public float intervaloDeTiempo = 1.0f;
    private float cronometro = 0f;

    [Header("Tasas de Cambio (Puntos por Salto)")]
    public float puntosLentos = 2f;
    public float puntosMedios = 5f;
    public float puntosRapidos = 10f;

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
        // Forzamos un cálculo inicial
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
            Debug.Log($"<color=cyan>Barra:</color> Hoguera registrada. Total: {todasLasHogueras.Count}");
        }
        RecalcularTasaDeCambio();
    }

    public void RecalcularTasaDeCambio()
    {
        int encendidas = 0;
        int apagadas = 0;

        // Log de seguridad: ¿Cuántas hogueras hay realmente en la lista?
        Debug.Log($"<color=orange>Barra Check:</color> Total en lista: {todasLasHogueras.Count}");

        foreach (var h in todasLasHogueras)
        {
            if (h == null) continue;
            if (h.estaEncendida) encendidas++; else apagadas++;
        }

        int diferencia = encendidas - apagadas;
        int diferenciaAbsoluta = Mathf.Abs(diferencia);

        // Asignación de puntos
        if (diferenciaAbsoluta == 1) puntosAAplicar = puntosLentos;
        else if (diferenciaAbsoluta == 3) puntosAAplicar = puntosMedios;
        else if (diferenciaAbsoluta == 5) puntosAAplicar = puntosRapidos;
        else puntosAAplicar = 0;

        // Dirección del cambio
        if (diferencia < 0) puntosAAplicar *= -1;

        Debug.Log($"<color=yellow>Resultado:</color> Diferencia {diferencia}. Puntos a aplicar ahora: {puntosAAplicar}");
    }

    private void AplicarCambioDePuntos()
    {
        if (puntosAAplicar == 0) return;

        progresoActual += puntosAAplicar;
        progresoActual = Mathf.Clamp(progresoActual, 0, 100);

        // Avisamos al árbitro WinLose del nuevo valor de la barra
        if (WinLose.Instance != null)
        {
            WinLose.Instance.ValidarProgreso(progresoActual);
        }
    }
}