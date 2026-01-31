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

    private Resolution[] resoluciones;

    void Start()
    {
        ConfigurarResoluciones();
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

        // Aplicar al mixer al inicio
        CambiarVolumenMaster(vMaster);
        CambiarVolumenMusica(vMusica);
        CambiarVolumenSFX(vSFX);

        togglePantallaCompleta.isOn = Screen.fullScreen;
        Debug.Log("<color=cyan>Preferencias de audio y pantalla cargadas.</color>");
    }

    // --- ABRIR Y CERRAR ---
    public void AbrirOpciones() => panelOpciones.SetActive(true);
    public void CerrarOpciones() => panelOpciones.SetActive(false);

    // --- LÓGICA DE AUDIO CON DEPURACIÓN ---
    public void CambiarVolumenMaster(float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;
        masterMixer.SetFloat("MasterVol", db);
        PlayerPrefs.SetFloat("VolMaster", valor);
        Debug.Log($"<color=green>Audio:</color> Master cambiado a {valor:P0} ({db:F1} dB)");
    }

    public void CambiarVolumenMusica(float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;
        masterMixer.SetFloat("MusicVol", db);
        PlayerPrefs.SetFloat("VolMusica", valor);
        Debug.Log($"<color=green>Audio:</color> Música cambiada a {valor:P0} ({db:F1} dB)");
    }

    public void CambiarVolumenSFX(float valor)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, valor)) * 20;
        masterMixer.SetFloat("SFXVol", db);
        PlayerPrefs.SetFloat("VolSFX", valor);
        Debug.Log($"<color=green>Audio:</color> SFX cambiado a {valor:P0} ({db:F1} dB)");
    }

    // --- LÓGICA DE RESOLUCIÓN ---
    void ConfigurarResoluciones()
    {
        resoluciones = Screen.resolutions;
        dropdownResoluciones.ClearOptions();
        List<string> opciones = new List<string>();
        int indiceResolucionActual = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height + " @ " + resoluciones[i].refreshRateRatio.value.ToString("F0") + "Hz";
            opciones.Add(opcion);

            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                indiceResolucionActual = i;
            }
        }

        dropdownResoluciones.AddOptions(opciones);
        dropdownResoluciones.value = indiceResolucionActual;
        dropdownResoluciones.RefreshShownValue();
        Debug.Log($"<color=yellow>Video:</color> {resoluciones.Length} resoluciones detectadas.");
    }

    public void CambiarResolucion(int indiceResolucion)
    {
        Resolution resolucion = resoluciones[indiceResolucion];
        Screen.SetResolution(resolucion.width, resolucion.height, Screen.fullScreen);
        Debug.Log($"<color=yellow>Video:</color> Resolución cambiada a {resolucion.width}x{resolucion.height}");
    }

    public void CambiarPantallaCompleta(bool esCompleta)
    {
        Screen.fullScreen = esCompleta;
        Debug.Log($"<color=yellow>Video:</color> Pantalla completa: {esCompleta}");
    }
}