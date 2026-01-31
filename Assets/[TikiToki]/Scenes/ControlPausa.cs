using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ControlPausa : MonoBehaviour
{
    [Header("Paneles de Menú")]
    public GameObject panelPausaPrincipal;
    public GameObject panelOpciones;
    public GameObject panelNiveles;

    private Stack<GameObject> historialMenus = new Stack<GameObject>();

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        // Forzamos la inicialización manual para que MenuNiveles y MenuOpciones carguen sus datos
        InicializarTodo();
    }

    private void InicializarTodo()
    {
        // 1. Los activamos para que sus métodos Start() se ejecuten con normalidad
        if (panelPausaPrincipal) panelPausaPrincipal.SetActive(true);
        if (panelOpciones) panelOpciones.SetActive(true);
        if (panelNiveles) panelNiveles.SetActive(true);

        // 2. Limpiamos la pila por si acaso y restauramos el tiempo
        historialMenus.Clear();
        Time.timeScale = 1f;

        // 3. Los ocultamos inmediatamente. Ahora ya están "despiertos" y con datos cargados.
        if (panelPausaPrincipal) panelPausaPrincipal.SetActive(false);
        if (panelOpciones) panelOpciones.SetActive(false);
        if (panelNiveles) panelNiveles.SetActive(false);
    }

    void Update()
    {
        // Solo permitimos pausa si el juego no ha terminado (WinLose)
        if (WinLose.Instance != null && WinLose.Instance.juegoTerminado) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GestionarTeclaEscape();
        }
    }

    public void GestionarTeclaEscape()
    {
        if (historialMenus.Count > 0)
        {
            CerrarUltimoMenu();
        }
        else
        {
            PausarJuego();
            AbrirMenu(panelPausaPrincipal);
        }
    }

    public void AbrirMenu(GameObject menuAAbrir)
    {
        if (menuAAbrir == null) return;

        // Ocultar el panel anterior si existe
        if (historialMenus.Count > 0)
        {
            historialMenus.Peek().SetActive(false);
        }

        // Mostrar el nuevo y registrar en la pila
        menuAAbrir.SetActive(true);
        historialMenus.Push(menuAAbrir);

        Debug.Log($"<color=cyan>Navegación:</color> Entrando en {menuAAbrir.name}.");
    }

    public void CerrarUltimoMenu()
    {
        if (historialMenus.Count == 0) return;

        GameObject menuCerrado = historialMenus.Pop();
        menuCerrado.SetActive(false);

        if (historialMenus.Count > 0)
        {
            // Reactivamos el anterior (ej: volver de Opciones a Pausa)
            historialMenus.Peek().SetActive(true);
        }
        else
        {
            // Volver al juego
            ReanudarJuego();
        }
    }

    public void PausarJuego()
    {
        Time.timeScale = 0f;
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f;

        if (panelPausaPrincipal) panelPausaPrincipal.SetActive(false);
        if (panelOpciones) panelOpciones.SetActive(false);
        if (panelNiveles) panelNiveles.SetActive(false);

        historialMenus.Clear();
    }

    public void VolverAlMenuPrincipal(string nombreEscenaMenuPrincipal)
    {
        // 1. IMPORTANTE: Reanudar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;

        // 2. Limpiar la pila por seguridad
        historialMenus.Clear();

        // 3. Cargar la escena
        Debug.Log("<color=orange>Cargando Menú Principal...</color>");
        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }
}