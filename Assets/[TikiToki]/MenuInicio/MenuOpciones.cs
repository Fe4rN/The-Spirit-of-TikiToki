using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuOpciones : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelOpciones;

    [Header("Audio")]
    public AudioMixer masterMixer;
    public Slider sliderMaster, sliderMusica, sliderSFX;

    [Header("Referencias UI Video/Ajustes")]
    public TMP_Dropdown resolucionDropdown;
    public Toggle pantallaCompletaToggle;

    private Resolution[] resoluciones;
    private List<Resolution> resolucionesUnicas;

    void Awake()
    {
        Application.targetFrameRate = 60;
        // 1. Cargamos la lista de resoluciones con el filtro que te funciona
        ConfigurarResoluciones();
    }

    void Start()
    {
        CargarAjustesGuardados();
    }

    // --- LÓGICA DE RESOLUCIÓN (BASADA EN TU SCRIPT 'AJUSTES') ---
    void ConfigurarResoluciones()
    {
        resoluciones = Screen.resolutions;
        resolucionDropdown.ClearOptions();

        List<string> opciones = new List<string>();
        resolucionesUnicas = new List<Resolution>();
        HashSet<string> resolucionesSet = new HashSet<string>();

        int resolucionActual = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            // Filtro de resolución máxima
            if (resoluciones[i].width > 1920 || resoluciones[i].height > 1080)
                continue;

            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;

            // Evitar duplicados por tasa de refresco (Hz)
            if (resolucionesSet.Contains(opcion))
                continue;

            resolucionesSet.Add(opcion);
            opciones.Add(opcion);
            resolucionesUnicas.Add(resoluciones[i]);

            // Detectar la resolución actual del sistema
            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                resolucionActual = opciones.Count - 1;
            }
        }

        resolucionDropdown.AddOptions(opciones);

        // Cargamos el índice guardado o el actual por defecto
        int indexGuardado = PlayerPrefs.GetInt("ResolucionIndex", resolucionActual);
        resolucionDropdown.value = indexGuardado;
        resolucionDropdown.RefreshShownValue();

        // Añadimos el listener por código para que sea más robusto
        resolucionDropdown.onValueChanged.AddListener(CambiarResolucion);
    }

    public void CambiarResolucion(int index)
    {
        if (index >= 0 && index < resolucionesUnicas.Count)
        {
            Resolution res = resolucionesUnicas[index];
            Screen.SetResolution(res.width, res.height, pantallaCompletaToggle.isOn);
            PlayerPrefs.SetInt("ResolucionIndex", index);
            PlayerPrefs.Save();
            Debug.Log($"Resolución aplicada: {res.width}x{res.height}");
        }
    }

    public void CambiarCalidad(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("CalidadIndex", index);
    }

    public void CambiarFPS(int index)
    {
        int fps = index switch { 0 => -1, 1 => 30, 2 => 60, 3 => 120, _ => -1 };
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("FPSIndex", index);
    }

    // --- AUDIO ---
    public void CambiarVolumenMaster(float v) => AplicarVolumen("MasterVol", "VolMaster", v);
    public void CambiarVolumenMusica(float v) => AplicarVolumen("MusicVol", "VolMusica", v);
    public void CambiarVolumenSFX(float v) => AplicarVolumen("SFXVol", "VolSFX", v);

    private void AplicarVolumen(string parameter, string prefKey, float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;
        if (masterMixer != null) masterMixer.SetFloat(parameter, db);
        PlayerPrefs.SetFloat(prefKey, valor);
    }

    // --- CARGA GENERAL ---
    void CargarAjustesGuardados()
    {
        // Pantalla completa
        bool esFull = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;
        pantallaCompletaToggle.isOn = esFull;
        Screen.fullScreen = esFull;
        pantallaCompletaToggle.onValueChanged.AddListener(CambiarPantallaCompleta);

        // Audio Sliders
        sliderMaster.value = PlayerPrefs.GetFloat("VolMaster", 0.75f);
        sliderMusica.value = PlayerPrefs.GetFloat("VolMusica", 0.75f);
        sliderSFX.value = PlayerPrefs.GetFloat("VolSFX", 0.75f);

        // Forzar aplicación de audio al inicio
        CambiarVolumenMaster(sliderMaster.value);
        CambiarVolumenMusica(sliderMusica.value);
        CambiarVolumenSFX(sliderSFX.value);

        // Aplicar resolución guardada
        CambiarResolucion(resolucionDropdown.value);
    }

    public void CambiarPantallaCompleta(bool esCompleta)
    {
        Screen.fullScreen = esCompleta;
        PlayerPrefs.SetInt("PantallaCompleta", esCompleta ? 1 : 0);
        // Al cambiar modo de pantalla, reaplicamos resolución para evitar borrosidad
        CambiarResolucion(resolucionDropdown.value);
    }

    public void AbrirOpciones() => panelOpciones.SetActive(true);
    public void CerrarOpciones() => panelOpciones.SetActive(false);
}