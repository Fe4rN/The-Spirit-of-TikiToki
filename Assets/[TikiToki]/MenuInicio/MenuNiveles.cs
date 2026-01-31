using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuNiveles : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelNiveles;
    public Transform contenedorBotones; // El que tiene el Grid Layout Group
    public GameObject prefabBoton;      // Prefab para los niveles numerados
    public Button botonTutorial;        // Arrastra aquí tu botón de tutorial

    [Header("Configuración")]
    public string nombreEscenaTutorial = "Tutorial";
    public string prefijoNivel = "level";

    void Start()
    {
        GenerarBotonesNiveles();
        ConfigurarBotonTutorial();
    }

    void ConfigurarBotonTutorial()
    {
        if (botonTutorial != null)
        {
            botonTutorial.onClick.RemoveAllListeners();
            botonTutorial.onClick.AddListener(() => SceneManager.LoadScene(nombreEscenaTutorial));

            // El tutorial siempre está disponible (puedes quitar esto si quieres bloquearlo)
            botonTutorial.interactable = true;
        }
    }

    void GenerarBotonesNiveles()
    {
        Debug.Log("<color=cyan>Generando botones...</color>");

        foreach (Transform hijo in contenedorBotones) Destroy(hijo.gameObject);

        int totalNiveles = ContarNivelesEnBuild();
        int progreso = PlayerPrefs.GetInt("NivelMaximo", 0);

        for (int i = 1; i <= totalNiveles; i++)
        {
            GameObject nuevoBotonObj = Instantiate(prefabBoton, contenedorBotones);
            nuevoBotonObj.name = "Boton_Level_" + i;

            // Asegurarnos de que el botón tenga escala 1 (a veces Instantiate la cambia)
            nuevoBotonObj.transform.localScale = Vector3.one;

            TextMeshProUGUI texto = nuevoBotonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (texto != null) texto.text = i.ToString();

            int indiceNivel = i;
            Button btn = nuevoBotonObj.GetComponent<Button>();
            btn.onClick.AddListener(() => SceneManager.LoadScene(prefijoNivel + indiceNivel));

            btn.interactable = (indiceNivel <= progreso + 1);
            Debug.Log($"Botón {i} creado en contenedor.");
        }

        // --- EL FIX PARA EL ORDEN ---
        // Forzamos al sistema de UI a reconstruir el layout inmediatamente
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenedorBotones.GetComponent<RectTransform>());
        Debug.Log("<color=cyan>Layout reconstruido.</color>");
    }

    private int ContarNivelesEnBuild()
    {
        int contador = 0;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string ruta = SceneUtility.GetScenePathByBuildIndex(i);
            string nombre = System.IO.Path.GetFileNameWithoutExtension(ruta);
            if (nombre.StartsWith(prefijoNivel)) contador++;
        }
        return contador;
    }

    public void AbrirMenu() => panelNiveles.SetActive(true);
    public void CerrarMenu() => panelNiveles.SetActive(false);
}