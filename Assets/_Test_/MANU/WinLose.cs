using UnityEngine;
using UnityEngine.UI;

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
    public Text textoTiempo;
    public Slider sliderVida;
    public GameObject panelVictoria;
    public GameObject panelDerrota;

    [Header("Referencias UI: Barra Dual (Progreso)")]
    public Image barraIzquierda; // Fill Origin: Right
    public Image barraDerecha;   // Fill Origin: Left

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
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        if (sliderVida != null)
        {
            sliderVida.maxValue = vidasMaximas;
            sliderVida.value = vidasActuales;
        }

        // Inicializamos las barras de progreso al centro (50%)
        ActualizarVisualBarraDual(50f);
        ActualizarUI();
    }

    private void Update()
    {
        if (juegoTerminado) return;

        // Sincronizar slider de vida por si cambias el valor en el Inspector
        if (sliderVida != null && sliderVida.value != vidasActuales)
            sliderVida.value = vidasActuales;

        if (vidasActuales <= 0)
        {
            FinalizarPartida(false, "Vidas agotadas.");
            return;
        }

        // Gestión del Cronómetro
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

    // --- MÉTODOS PÚBLICOS ---

    public void ModificarVidas(int cantidad)
    {
        if (juegoTerminado) return;
        vidasActuales += cantidad;
        vidasActuales = Mathf.Clamp(vidasActuales, 0, vidasMaximas);
    }

    public void ValidarProgreso(float valorBarra)
    {
        if (juegoTerminado) return;

        // Actualizar visual de la barra dual (0-100)
        ActualizarVisualBarraDual(valorBarra);

        if (valorBarra >= 100f) FinalizarPartida(true, "¡Luz completa!");
        else if (valorBarra <= 0f) FinalizarPartida(false, "Oscuridad total.");
    }

    // --- LÓGICA INTERNA ---

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

        if (victoria)
        {
            if (panelVictoria != null) panelVictoria.SetActive(true);
        }
        else
        {
            if (panelDerrota != null) panelDerrota.SetActive(true);
        }
        Debug.Log(mensaje);
    }
}