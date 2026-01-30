using UnityEngine;
using UnityEngine.UI;

public class WinLose : MonoBehaviour
{
    public static WinLose Instance { get; private set; }

    [Header("Configuración de Partida")]
    public float tiempoMaximo = 120f;
    public int vidasIniciales = 5;

    [Header("Monitor de Estado (Solo lectura)")]
    // Al ponerlas públicas aparecen en el Inspector automáticamente
    public float tiempoRestante;
    public int vidasActuales;
    public float progresoDeBarraActual;
    public bool juegoTerminado = false;

    [Header("Referencias de UI (Canvas)")]
    public Text textoTiempo;
    public Text textoVidas;
    public GameObject panelVictoria;
    public GameObject panelDerrota;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Inicializamos valores
        vidasActuales = vidasIniciales;
        tiempoRestante = tiempoMaximo;
    }

    private void Start()
    {
        // Aseguramos que los paneles estén ocultos al empezar
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        ActualizarUI();
    }

    private void Update()
    {
        if (juegoTerminado) return;

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
        ActualizarUI();

        if (vidasActuales <= 0)
        {
            FinalizarPartida(false, "Vidas agotadas.");
        }
    }

    public void ValidarProgreso(float valorBarra)
    {
        if (juegoTerminado) return;

        // Actualizamos la variable para verla en el inspector
        progresoDeBarraActual = valorBarra;

        if (progresoDeBarraActual >= 100f) FinalizarPartida(true, "¡Luz completa!");
        else if (progresoDeBarraActual <= 0f) FinalizarPartida(false, "Oscuridad total.");
    }

    // --- LÓGICA DE FIN DE JUEGO ---

    private void FinalizarPartida(bool victoria, string mensaje)
    {
        if (juegoTerminado) return; // Evita que se dispare varias veces
        juegoTerminado = true;

        if (victoria)
        {
            Debug.Log($"<color=cyan><b>[WIN]:</b> {mensaje}</color>");
            if (panelVictoria != null) panelVictoria.SetActive(true);
        }
        else
        {
            Debug.Log($"<color=magenta><b>[LOSE]:</b> {mensaje}</color>");
            if (panelDerrota != null) panelDerrota.SetActive(true);
        }

        // Opcional: Pausar el juego al terminar
        // Time.timeScale = 0f; 
    }

    private void ActualizarUI()
    {
        if (textoTiempo != null) textoTiempo.text = "Tiempo: " + Mathf.Ceil(tiempoRestante).ToString();
        if (textoVidas != null) textoVidas.text = "Vidas: " + vidasActuales.ToString();
    }
}