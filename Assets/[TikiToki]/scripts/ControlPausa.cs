using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ControlPausa : MonoBehaviour
{
    [Header("Configuración")]
    public bool esMenuPrincipal = false;
    public string nombreEscenaMenuPrincipal = "MainMenu";

    [Header("Paneles de Menú")]
    public GameObject panelPausaPrincipal; // En el MainMenu, este puede ser el panel base o estar vacío
    public GameObject panelOpciones;
    public GameObject panelNiveles;

    [Header("Paneles de Fin de Juego")]
    public GameObject panelVictoria;
    public GameObject panelDerrota;

    private Stack<GameObject> historialMenus = new Stack<GameObject>();

    private void Awake()
    {
        // Solo pausamos el tiempo si NO es el menú principal
        Time.timeScale = 1f;
    }

    void Start()
    {
        InicializarTodo();
    }

    private void InicializarTodo()
    {
        if (panelPausaPrincipal) panelPausaPrincipal.SetActive(true);
        if (panelOpciones) panelOpciones.SetActive(true);
        if (panelNiveles) panelNiveles.SetActive(true);

        // Si es MainMenu, el panel principal suele estar activo, no lo metemos en la pila aún
        // para que ESC no lo "cierre" dejando la pantalla vacía.

        if (panelPausaPrincipal) panelPausaPrincipal.SetActive(esMenuPrincipal);
        if (panelOpciones) panelOpciones.SetActive(false);
        if (panelNiveles) panelNiveles.SetActive(false);

        historialMenus.Clear();
    }

    void Update()
    {
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
        else if (!esMenuPrincipal) // Solo abre pausa con ESC si estamos en el juego
        {
            PausarJuego();
            AbrirMenu(panelPausaPrincipal);
        }
    }

    public void AbrirMenu(GameObject menuAAbrir)
    {
        if (menuAAbrir == null) return;

        // Si es el menú principal y abrimos un submenú, ocultamos el fondo si quieres
        if (historialMenus.Count > 0)
        {
            historialMenus.Peek().SetActive(false);
        }
        else if (esMenuPrincipal && panelPausaPrincipal != null)
        {
            // Ocultamos el panel base del menú principal al entrar a submenús
            panelPausaPrincipal.SetActive(false);
        }

        menuAAbrir.SetActive(true);
        historialMenus.Push(menuAAbrir);
    }

    public void CerrarUltimoMenu()
    {
        if (historialMenus.Count == 0) return;

        GameObject menuCerrado = historialMenus.Pop();
        menuCerrado.SetActive(false);

        if (historialMenus.Count > 0)
        {
            historialMenus.Peek().SetActive(true);
        }
        else
        {
            if (esMenuPrincipal)
            {
                // Si volvemos al final de la pila en el MainMenu, reactivamos el panel base
                if (panelPausaPrincipal) panelPausaPrincipal.SetActive(true);
            }
            else
            {
                ReanudarJuego();
            }
        }
    }

    public void PausarJuego() { if (!esMenuPrincipal) Time.timeScale = 0f; }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f;
        if (panelPausaPrincipal && !esMenuPrincipal) panelPausaPrincipal.SetActive(false);
        if (panelOpciones) panelOpciones.SetActive(false);
        if (panelNiveles) panelNiveles.SetActive(false);
        historialMenus.Clear();
    }

    public void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }

    // Este método lo llamarás desde WinLose en lugar de hacer SetActive(true) allí
    public void ActivarPanelFin(GameObject panelFin)
    {
        if (panelFin == null) return;

        PausarJuego(); // Detenemos el tiempo

        // Si hubiera algún menú de pausa abierto, lo ocultamos
        if (historialMenus.Count > 0) historialMenus.Peek().SetActive(false);

        panelFin.SetActive(true);
        historialMenus.Push(panelFin); // Lo metemos en la pila para que Esc sepa que hay algo
    }

    // --- FUNCIONES DE NAVEGACIÓN ---

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        historialMenus.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SiguienteNivel()
    {
        Time.timeScale = 1f;
        historialMenus.Clear();

        int siguienteIndice = SceneManager.GetActiveScene().buildIndex + 1;
        if (siguienteIndice < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(siguienteIndice);
        }
        else
        {
            VolverAlMenuPrincipal();
        }
    }
}