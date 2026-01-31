using UnityEngine;

public class ControlPausa : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelPausa;

    private bool estaPausado = false;

    void Start()
    {
        // Nos aseguramos de que el panel esté oculto al iniciar
        if (panelPausa != null)
            panelPausa.SetActive(false);
    }

    void Update()
    {
        // Detectar la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (estaPausado)
                ReanudarJuego();
            else
                PausarJuego();
        }
    }

    public void PausarJuego()
    {
        estaPausado = true;
        Time.timeScale = 0f; // Congela el tiempo (física, timers, Update dependientes de delta)
        if (panelPausa != null) panelPausa.SetActive(true);

        Debug.Log("<color=yellow>Juego Pausado</color>");
    }

    public void ReanudarJuego()
    {
        estaPausado = false;
        Time.timeScale = 1f; // Devuelve el tiempo a la normalidad
        if (panelPausa != null) panelPausa.SetActive(false);

        Debug.Log("<color=green>Juego Reanudado</color>");
    }

    // Método útil para el botón de "Salir" del menú
    public void SalirAlMenu(string nombreEscena)
    {
        Time.timeScale = 1f; // ¡Importante! Siempre resetear el tiempo antes de cambiar de escena
        UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscena);
    }
}