using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuOpciones : MonoBehaviour
{
    [Header("Paneles (Pueden estar inactivos)")]
    public GameObject panelOpciones;

    [Header("Audio")]
    public AudioMixer masterMixer;
    public Slider sliderMaster, sliderMusica, sliderSFX;

    [Header("Video UI")]
    public TMP_Dropdown dropdownResoluciones;
    public Toggle togglePantallaCompleta;

    private Resolution[] resoluciones;
    private List<Resolution> resolucionesUnicas;

    void Awake()
    {
        // 1. Cargamos las resoluciones antes que nada
        ConfigurarResoluciones();

        // 2. Aplicamos los ajustes al arrancar el juego, 
        // esté el panel abierto o no.
        AplicarAjustesIniciales();
    }

    void OnEnable()
    {
        // 3. Cada vez que el panel se active, refrescamos los sliders/toggles
        // para que coincidan con lo guardado.
        RefrescarInterfazUI();
    }

    void AplicarAjustesIniciales()
    {
        // Audio
        CambiarVolumenMaster(PlayerPrefs.GetFloat("VolMaster", 0.75f));
        CambiarVolumenMusica(PlayerPrefs.GetFloat("VolMusica", 0.75f));
        CambiarVolumenSFX(PlayerPrefs.GetFloat("VolSFX", 0.75f));

        // Pantalla Completa
        bool esFull = PlayerPrefs.GetInt("PantallaCompleta", 1) == 1;

        // Resolución
        int indexRes = PlayerPrefs.GetInt("ResolucionIndex", -1);

        if (indexRes == -1) // Si es la primera vez, buscar la del monitor
        {
            indexRes = BuscarIndiceResolucionActual();
        }

        if (indexRes < resolucionesUnicas.Count)
        {
            Resolution res = resolucionesUnicas[indexRes];
            // Usamos FullScreenWindow para evitar que se vea borroso o parpadee
            Screen.SetResolution(res.width, res.height, esFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
    }

    void RefrescarInterfazUI()
    {
        if (sliderMaster) sliderMaster.value = PlayerPrefs.GetFloat("VolMaster", 0.75f);
        if (sliderMusica) sliderMusica.value = PlayerPrefs.GetFloat("VolMusica", 0.75f);
        if (sliderSFX) sliderSFX.value = PlayerPrefs.GetFloat("VolSFX", 0.75f);

        if (togglePantallaCompleta) togglePantallaCompleta.isOn = PlayerPrefs.GetInt("PantallaCompleta", 1) == 1;

        if (dropdownResoluciones)
        {
            dropdownResoluciones.value = PlayerPrefs.GetInt("ResolucionIndex", BuscarIndiceResolucionActual());
            dropdownResoluciones.RefreshShownValue();
        }
    }

    // --- LÓGICA DE RESOLUCIÓN MEJORADA ---
    void ConfigurarResoluciones()
    {
        resoluciones = Screen.resolutions;
        resolucionesUnicas = new List<Resolution>();
        HashSet<string> resolucionesSet = new HashSet<string>();
        List<string> opciones = new List<string>();

        // Invertimos el bucle para que las resoluciones más altas salgan primero
        for (int i = resoluciones.Length - 1; i >= 0; i--)
        {
            // Filtro para no pasar de 1080p si quieres mantener rendimiento
            if (resoluciones[i].width > 1920 || resoluciones[i].height > 1080) continue;

            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;

            if (!resolucionesSet.Contains(opcion))
            {
                resolucionesSet.Add(opcion);
                resolucionesUnicas.Add(resoluciones[i]);
                opciones.Add(opcion);
            }
        }

        // Volvemos a poner el orden normal (de menor a mayor)
        resolucionesUnicas.Reverse();
        opciones.Reverse();

        if (dropdownResoluciones)
        {
            dropdownResoluciones.ClearOptions();
            dropdownResoluciones.AddOptions(opciones);
        }
    }

    int BuscarIndiceResolucionActual()
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
        if (indice >= 0 && indice < resolucionesUnicas.Count)
        {
            Resolution res = resolucionesUnicas[indice];
            bool esFull = togglePantallaCompleta.isOn;

            Screen.SetResolution(res.width, res.height, esFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

            PlayerPrefs.SetInt("ResolucionIndex", indice);
            PlayerPrefs.Save();
        }
    }

    public void CambiarPantallaCompleta(bool esCompleta)
    {
        // Aplicamos el modo sin bordes para máxima compatibilidad
        Screen.fullScreenMode = esCompleta ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreen = esCompleta;

        PlayerPrefs.SetInt("PantallaCompleta", esCompleta ? 1 : 0);
        PlayerPrefs.Save();
    }

    // --- AUDIO (Sin cambios, es correcto) ---
    public void CambiarVolumenMaster(float v) { AplicarVolumen("MasterVol", "VolMaster", v); }
    public void CambiarVolumenMusica(float v) { AplicarVolumen("MusicVol", "VolMusica", v); }
    public void CambiarVolumenSFX(float v) { AplicarVolumen("SFXVol", "VolSFX", v); }

    private void AplicarVolumen(string parameter, string prefKey, float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;
        if (masterMixer != null) masterMixer.SetFloat(parameter, db);
        PlayerPrefs.SetFloat(prefKey, valor);
    }

    public void AbrirOpciones() => panelOpciones.SetActive(true);
    public void CerrarOpciones() => panelOpciones.SetActive(false);
}