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
        // 1. Cargamos la lista de resoluciones
        ConfigurarResoluciones();
    }

    void Start()
    {
        CargarAjustesGuardados();
    }

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
            if (resoluciones[i].width > 1920 || resoluciones[i].height > 1080)
                continue;

            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;

            if (resolucionesSet.Contains(opcion))
                continue;

            resolucionesSet.Add(opcion);
            opciones.Add(opcion);
            resolucionesUnicas.Add(resoluciones[i]);

            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                resolucionActual = opciones.Count - 1;
            }
        }

        resolucionDropdown.AddOptions(opciones);

        // --- SOLUCIÓN AL NULL REFERENCE ---
        // 1. Quitamos cualquier listener previo (incluyendo los del Inspector)
        resolucionDropdown.onValueChanged.RemoveAllListeners();

        int indexGuardado = PlayerPrefs.GetInt("ResolucionIndex", resolucionActual);

        // 2. Ponemos el valor (ya no disparará nada porque no hay listeners)
        resolucionDropdown.value = indexGuardado;
        resolucionDropdown.RefreshShownValue();

        // 3. AHORA añadimos el listener para cuando el usuario lo mueva a mano
        resolucionDropdown.onValueChanged.AddListener(CambiarResolucion);
    }

    public void CambiarResolucion(int index)
    {
        // Seguro extra: si la lista no existe o el índice está mal, salimos
        if (resolucionesUnicas == null || index < 0 || index >= resolucionesUnicas.Count)
            return;

        Resolution res = resolucionesUnicas[index];

        // Seguro extra para el Toggle
        bool esFull = (pantallaCompletaToggle != null) ? pantallaCompletaToggle.isOn : Screen.fullScreen;

        Screen.SetResolution(res.width, res.height, esFull);
        PlayerPrefs.SetInt("ResolucionIndex", index);
        PlayerPrefs.Save();
        Debug.Log($"Resolución aplicada: {res.width}x{res.height}");
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
    public void CambiarVolumenMaster(float v) => AplicarVolumen("Master", "Master", v);
    public void CambiarVolumenMusica(float v) => AplicarVolumen("MusicVol", "Musica", v);
    public void CambiarVolumenSFX(float v) => AplicarVolumen("SFXVol", "SFX", v);

    private void AplicarVolumen(string parameter, string prefKey, float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;

        if (masterMixer != null) masterMixer.SetFloat(parameter, db);
        PlayerPrefs.SetFloat(prefKey, valor);
    }

    void CargarAjustesGuardados()
    {
        bool esFull = PlayerPrefs.GetInt("PantallaCompleta", Screen.fullScreen ? 1 : 0) == 1;

        if (pantallaCompletaToggle != null)
        {
            pantallaCompletaToggle.onValueChanged.RemoveAllListeners();
            pantallaCompletaToggle.isOn = esFull;
            pantallaCompletaToggle.onValueChanged.AddListener(CambiarPantallaCompleta);
        }

        Screen.fullScreen = esFull;

        float vMaster = PlayerPrefs.GetFloat("Master", 0.75f);
        float vMusica = PlayerPrefs.GetFloat("Musica", 0.75f);
        float vSFX = PlayerPrefs.GetFloat("SFX", 0.75f);

        // Aseguramos listeners para sliders
        if (sliderMaster) { sliderMaster.onValueChanged.RemoveAllListeners(); sliderMaster.onValueChanged.AddListener(CambiarVolumenMaster); sliderMaster.value = vMaster; }
        if (sliderMusica) { sliderMusica.onValueChanged.RemoveAllListeners(); sliderMusica.onValueChanged.AddListener(CambiarVolumenMusica); sliderMusica.value = vMusica; }
        if (sliderSFX) { sliderSFX.onValueChanged.RemoveAllListeners(); sliderSFX.onValueChanged.AddListener(CambiarVolumenSFX); sliderSFX.value = vSFX; }

        AplicarVolumen("Master", "Master", vMaster);
        AplicarVolumen("MusicVol", "Musica", vMusica);
        AplicarVolumen("SFXVol", "SFX", vSFX);

        // Aplicamos la resolución cargada al inicio
        CambiarResolucion(resolucionDropdown.value);
    }

    public void CambiarPantallaCompleta(bool esCompleta)
    {
        Screen.fullScreen = esCompleta;
        PlayerPrefs.SetInt("PantallaCompleta", esCompleta ? 1 : 0);
        CambiarResolucion(resolucionDropdown.value);
    }

    public void AbrirOpciones() => panelOpciones.SetActive(true);
    public void CerrarOpciones() => panelOpciones.SetActive(false);
}