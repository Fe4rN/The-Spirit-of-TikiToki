using UnityEngine;
using UnityEngine.SceneManagement;

public class Continuar : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscenaTutorial = "Tutorial";
    public string prefijoNivel = "level";

    public void ClickEnJugar()
    {
        int maxNivelSuperado = PlayerPrefs.GetInt("NivelMaximo", 0);
        int totalDeNiveles = ContarNivelesEnBuild();

        if (maxNivelSuperado == 0)
        {
            // Verifica si la escena existe en el Build antes de cargar
            CargarEscenaSegura(nombreEscenaTutorial);
        }
        else if (maxNivelSuperado >= totalDeNiveles)
        {
            CargarEscenaSegura(prefijoNivel + totalDeNiveles);
        }
        else
        {
            CargarEscenaSegura(prefijoNivel + (maxNivelSuperado + 1));
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
            Debug.LogError($"Error: La escena '{nombre}' no está en Build Settings. Arrástrala a File > Build Settings.");
        }
    }

    private int ContarNivelesEnBuild()
    {
        int contador = 0;
        int escenasEnBuild = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < escenasEnBuild; i++)
        {
            // Obtenemos solo el nombre de la escena, no la ruta completa
            string ruta = SceneUtility.GetScenePathByBuildIndex(i);
            string nombreEscena = System.IO.Path.GetFileNameWithoutExtension(ruta);

            // Solo contamos si empieza exactamente con "level" (para no contar el Tutorial)
            if (nombreEscena.StartsWith(prefijoNivel))
            {
                contador++;
            }
        }
        return contador;
    }
}