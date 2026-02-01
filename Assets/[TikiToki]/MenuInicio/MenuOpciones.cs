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

    [Header("Video UI")]
    public TMP_Dropdown dropdownResoluciones;
    public Toggle togglePantallaCompleta;

    private List<Resolution> resolucionesUnicas;

    void Awake()
    {
        // 1. Esto DEBE ir en Awake para tener la lista lista antes que nada
        ConfigurarResoluciones();
    }

    void Start()
    {
        // 2. Aplicamos ajustes en Start para asegurar que la ventana de Unity ya responda
        AplicarAjustesIniciales();
    }

    void OnEnable()
    {
        // Refrescamos visualmente los sliders y el toggle cuando se abre el menú
        RefrescarInterfazUI();
    }

    void ConfigurarResoluciones()
    {
        Resolution[] todas = Screen.resolutions;
        resolucionesUnicas = new List<Resolution>();
        HashSet<string> registro = new HashSet<string>();
        List<string> opcionesVisibles = new List<string>();

        // Recorremos de mayor a menor para pillar siempre el refresco (Hz) más alto primero
        for (int i = todas.Length - 1; i >= 0; i--)
        {
            if (todas[i].width > 1920 || todas[i].height > 1080) continue;

            string llave = todas[i].width + "x" + todas[i].height;

            if (!registro.Contains(llave))
            {
                registro.Add(llave);
                resolucionesUnicas.Add(todas[i]);
            }
        }

        // Le damos la vuelta para que en el menú salgan de menor a mayor
        resolucionesUnicas.Reverse();

        foreach (var res in resolucionesUnicas)
        {
            opcionesVisibles.Add(res.width + " x " + res.height + " (" + (int)res.refreshRateRatio.value + "Hz)");
        }

        dropdownResoluciones.ClearOptions();
        dropdownResoluciones.AddOptions(opcionesVisibles);
    }

    void AplicarAjustesIniciales()
    {
        // Audio
        AplicarVolumen("MasterVol", "VolMaster", PlayerPrefs.GetFloat("VolMaster", 0.75f));
        AplicarVolumen("MusicVol", "VolMusica", PlayerPrefs.GetFloat("VolMusica", 0.75f));
        AplicarVolumen("SFXVol", "VolSFX", PlayerPrefs.GetFloat("VolSFX", 0.75f));

        // Pantalla y Resolución
        int indexGuardado = PlayerPrefs.GetInt("ResolucionIndex", -1);
        bool esFull = PlayerPrefs.GetInt("PantallaCompleta", 1) == 1;

        if (indexGuardado == -1) indexGuardado = BuscarIndiceActual();

        // Aplicar de golpe
        if (indexGuardado < resolucionesUnicas.Count)
        {
            Resolution res = resolucionesUnicas[indexGuardado];
            Screen.SetResolution(res.width, res.height, esFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
    }

    int BuscarIndiceActual()
    {
        for (int i = 0; i < resolucionesUnicas.Count; i++)
        {
            if (resolucionesUnicas[i].width == Screen.width && resolucionesUnicas[i].height == Screen.height)
                return i;
        }
        return resolucionesUnicas.Count - 1;
    }

    public void CambiarResolucion(int indice)
    {
        if (indice < 0 || indice >= resolucionesUnicas.Count) return;

        Resolution res = resolucionesUnicas[indice];
        // IMPORTANTE: Mantener el modo de pantalla que ya tiene el Toggle
        Screen.SetResolution(res.width, res.height, togglePantallaCompleta.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        PlayerPrefs.SetInt("ResolucionIndex", indice);
        PlayerPrefs.Save();
    }

    public void CambiarPantallaCompleta(bool esCompleta)
    {
        Screen.fullScreen = esCompleta;
        Screen.fullScreenMode = esCompleta ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        PlayerPrefs.SetInt("PantallaCompleta", esCompleta ? 1 : 0);
        PlayerPrefs.Save();

        // Pequeño truco: re-aplicar resolución para que no se vea borroso al cambiar el modo
        CambiarResolucion(dropdownResoluciones.value);
    }

    // --- INTERFAZ ---
    void RefrescarInterfazUI()
    {
        sliderMaster.value = PlayerPrefs.GetFloat("VolMaster", 0.75f);
        sliderMusica.value = PlayerPrefs.GetFloat("VolMusica", 0.75f);
        sliderSFX.value = PlayerPrefs.GetFloat("VolSFX", 0.75f);

        togglePantallaCompleta.isOn = PlayerPrefs.GetInt("PantallaCompleta", 1) == 1;

        dropdownResoluciones.value = PlayerPrefs.GetInt("ResolucionIndex", BuscarIndiceActual());
        dropdownResoluciones.RefreshShownValue();
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

    public void AbrirOpciones() => panelOpciones.SetActive(true);
    public void CerrarOpciones() => panelOpciones.SetActive(false);
}