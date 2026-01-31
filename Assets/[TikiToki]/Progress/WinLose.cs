using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinLose : MonoBehaviour
{
    public static WinLose Instance { get; private set; }

    [Header("Configuración de Partida")]
    public int vidasMaximas = 5;
    public float tiempoMaximo = 120f;

    [Header("Monitor de Estado (Inspector)")]
    public int vidasActuales;
    public float tiempoRestante;
    public bool juegoTerminado = false;

    [Header("Referencias UI: Tiempo y Vidas")]
    public TextMeshProUGUI textoTiempo;
    public Slider sliderVida;

    [Header("Referencias UI: Barra Dual (Progreso)")]
    public Image barraIzquierda; // Fill Origin: Right
    public Image barraDerecha;   // Fill Origin: Left

    [Header("Sistema de Navegación")]
    public ControlPausa scriptPausa; // Arrastra el objeto con el ControlPausa aquí

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        vidasActuales = vidasMaximas;
        tiempoRestante = tiempoMaximo;
        juegoTerminado = false;
    }

    private void Start()
    {
        if (scriptPausa != null)
        {
            if (scriptPausa.panelVictoria != null) scriptPausa.panelVictoria.SetActive(false);
            if (scriptPausa.panelDerrota != null) scriptPausa.panelDerrota.SetActive(false);
        }

        if (sliderVida != null)
        {
            sliderVida.maxValue = vidasMaximas;
            sliderVida.value = vidasActuales;
        }

        ActualizarVisualBarraDual(50f);
        ActualizarUI();
    }

    private void Update()
    {
        if (juegoTerminado) return;

        if (sliderVida != null && sliderVida.value != vidasActuales)
            sliderVida.value = vidasActuales;

        if (vidasActuales <= 0)
        {
            FinalizarPartida(false, "Vidas agotadas.");
            return;
        }

        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            ActualizarUI();
        }
        else
        {
            FinalizarPartida(false, "¡Tiempo agotado!");
        }
    }

    public void ModificarVidas(int cantidad)
    {
        if (juegoTerminado) return;
        vidasActuales += cantidad;
        vidasActuales = Mathf.Clamp(vidasActuales, 0, vidasMaximas);
    }

    public void ValidarProgreso(float valorBarra)
    {
        if (juegoTerminado) return;

        ActualizarVisualBarraDual(valorBarra);

        if (valorBarra >= 100f) FinalizarPartida(true, "¡Luz completa!");
        else if (valorBarra <= 0f) FinalizarPartida(false, "Oscuridad total.");
    }

    private void ActualizarVisualBarraDual(float valor)
    {
        if (barraIzquierda == null || barraDerecha == null) return;
        valor = Mathf.Clamp(valor, 0, 100);

        if (valor >= 50)
        {
            barraIzquierda.fillAmount = 0;
            barraDerecha.fillAmount = (valor - 50) / 50f;
        }
        else
        {
            barraDerecha.fillAmount = 0;
            barraIzquierda.fillAmount = 1f - (valor / 50f);
        }
    }

    private void ActualizarUI()
    {
        if (textoTiempo != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTiempo.text = string.Format("{0}:{1:00}", minutos, segundos);
        }
    }

    private void FinalizarPartida(bool victoria, string mensaje)
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        if (scriptPausa == null)
        {
            Debug.LogError("No hay referencia al script ControlPausa en WinLose.");
            return;
        }

        if (victoria)
        {
            // Usamos el método de ControlPausa para registrar el panel en la pila
            scriptPausa.ActivarPanelFin(scriptPausa.panelVictoria);
        }
        else
        {
            scriptPausa.ActivarPanelFin(scriptPausa.panelDerrota);
        }

        Debug.Log("<color=white>Estado Final: </color>" + mensaje);
    }
}