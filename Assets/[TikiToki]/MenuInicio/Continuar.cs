using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Necesario para el texto

public class Continuar : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    public string nombreEscenaHistoria = "Historia";
    public string prefijoNivel = "level";

    [Header("Referencias UI")]
    public TextMeshProUGUI textoBoton; // Arrastra aquí el texto del botón

    void Start()
    {
        ActualizarTextoBoton();
    }

    private void ActualizarTextoBoton()
    {
        if (textoBoton == null) return;

        // Si el nivel máximo es 0, es que nunca ha empezado
        int maxNivelSuperado = PlayerPrefs.GetInt("NivelMaximo", 0);

        if (maxNivelSuperado == 0)
        {
            textoBoton.text = "Empezar";
        }
        else
        {
            textoBoton.text = "Continuar";
        }
    }

    public void ClickEnJugar()
    {
        int maxNivelSuperado = PlayerPrefs.GetInt("NivelMaximo", 0);
        int totalDeNiveles = ContarNivelesEnBuild();

        if (maxNivelSuperado == 0)
        {
            CargarEscenaSegura(nombreEscenaHistoria);
        }
        else
        {
            // En lugar de + 1, cargamos el nivel que el Game Manager guardó
            // Si el Game Manager guardó "1", volvemos al "level1"
            CargarEscenaSegura(prefijoNivel + maxNivelSuperado);
        }
    }

    private void CargarEscenaSegura(string nombre)
    {
        if (Application.CanStreamedLevelBeLoaded(nombre))
        {
            SceneManager.LoadScene(nombre);
        }
        else
        {
            Debug.LogError($"Error: La escena '{nombre}' no está en Build Settings.");
        }
    }

    private int ContarNivelesEnBuild()
    {
        int contador = 0;
        int escenasEnBuild = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < escenasEnBuild; i++)
        {
            string ruta = SceneUtility.GetScenePathByBuildIndex(i);
            string nombreEscena = System.IO.Path.GetFileNameWithoutExtension(ruta);

            if (nombreEscena.StartsWith(prefijoNivel))
            {
                contador++;
            }
        }
        return contador;
    }
}