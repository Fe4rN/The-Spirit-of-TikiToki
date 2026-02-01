using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuOpciones : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelOpciones;

    [Header("Audio Sliders")]
    public AudioMixer masterMixer;
    public Slider sliderMaster;
    public Slider sliderMusica;
    public Slider sliderSFX;

    [Header("Visualización")]
    public TMP_Dropdown dropdownResoluciones;
    public Toggle togglePantallaCompleta;

    // Lógica de resoluciones del script Ajustes
    private Resolution[] resoluciones;
    private List<Resolution> resolucionesUnicas;

    void Start()
    {
        // 1. Configuramos las resoluciones primero
        ConfigurarResoluciones();
        // 2. Cargamos el resto de preferencias
        CargarPreferencias();
    }

    void CargarPreferencias()
    {
        // Cargar Volumenes
        float vMaster = PlayerPrefs.GetFloat("VolMaster", 0.75f);
        float vMusica = PlayerPrefs.GetFloat("VolMusica", 0.75f);
        float vSFX = PlayerPrefs.GetFloat("VolSFX", 0.75f);

        sliderMaster.value = vMaster;
        sliderMusica.value = vMusica;
        sliderSFX.value = vSFX;

        CambiarVolumenMaster(vMaster);
        CambiarVolumenMusica(vMusica);
        CambiarVolumenSFX(vSFX);

        // Cargar Pantalla Completa
        bool fullScreenPref = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = fullScreenPref;
        togglePantallaCompleta.isOn = fullScreenPref;

        // Cargar índice de resolución guardado
        int resGuardada = PlayerPrefs.GetInt("ResolucionIndex", dropdownResoluciones.value);
        if (resGuardada < resolucionesUnicas.Count)
        {
            dropdownResoluciones.value = resGuardada;
            dropdownResoluciones.RefreshShownValue();
            // Aplicamos la resolución cargada
            CambiarResolucion(resGuardada);
        }

        Debug.Log("<color=cyan>Preferencias de audio y video cargadas con éxito.</color>");
    }

    // --- LÓGICA DE AUDIO ---
    public void CambiarVolumenMaster(float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;
        if (masterMixer != null) masterMixer.SetFloat("MasterVol", db);
        PlayerPrefs.SetFloat("VolMaster", valor);
    }

    public void CambiarVolumenMusica(float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;
        if (masterMixer != null) masterMixer.SetFloat("MusicVol", db);
        PlayerPrefs.SetFloat("VolMusica", valor);
    }

    public void CambiarVolumenSFX(float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;
        if (masterMixer != null) masterMixer.SetFloat("SFXVol", db);
        PlayerPrefs.SetFloat("VolSFX", valor);
    }

    // --- LÓGICA DE RESOLUCIÓN (IMPORTADA DE AJUSTES) ---
    void ConfigurarResoluciones()
    {
        resoluciones = Screen.resolutions;
        dropdownResoluciones.ClearOptions();

        List<string> opciones = new List<string>();
        resolucionesUnicas = new List<Resolution>();
        HashSet<string> resolucionesSet = new HashSet<string>();

        int indiceResolucionActual = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            // Filtro: No más de 1080p (como en tu script de Ajustes)
            if (resoluciones[i].width > 1920 || resoluciones[i].height > 1080)
                continue;

            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;

            // Evitar duplicados (mismas resoluciones con distintos Hz)
            if (resolucionesSet.Contains(opcion))
                continue;

            resolucionesSet.Add(opcion);
            opciones.Add(opcion);
            resolucionesUnicas.Add(resoluciones[i]);

            // Detectar cuál es la resolución actual del monitor
            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                indiceResolucionActual = opciones.Count - 1;
            }
        }

        dropdownResoluciones.AddOptions(opciones);
        dropdownResoluciones.value = indiceResolucionActual;
        dropdownResoluciones.RefreshShownValue();
    }

    public void CambiarResolucion(int indice)
    {
        if (indice >= 0 && indice < resolucionesUnicas.Count)
        {
            Resolution res = resolucionesUnicas[indice];
            Screen.SetResolution(res.width, res.height, togglePantallaCompleta.isOn);
            PlayerPrefs.SetInt("ResolucionIndex", indice);
            PlayerPrefs.Save();
            Debug.Log($"<color=yellow>Video:</color> Resolución: {res.width}x{res.height}");
        }
    }

    public void CambiarPantallaCompleta(bool esCompleta)
    {
        Screen.fullScreen = esCompleta;
        PlayerPrefs.SetInt("PantallaCompleta", esCompleta ? 1 : 0);
        PlayerPrefs.Save();

        // Al cambiar a pantalla completa, reaplicamos la resolución actual para evitar estiramientos
        CambiarResolucion(dropdownResoluciones.value);
    }

    // --- ABRIR Y CERRAR ---
    public void AbrirOpciones() => panelOpciones.SetActive(true);
    public void CerrarOpciones() => panelOpciones.SetActive(false);
}