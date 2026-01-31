using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public Camera playerCamera;

    private int _currentStep = 0;

    // Lista de instrucciones - Puedes cambiar los textos aquí
    private string[] _instructions = {
        "Usa **WASD** para moverte",
        "Usa **Q** y **E** para el Zoom",
        "Busca el **Hacha** y recógela con ESPACIO",
        "Equipa el hacha y **TALA** un árbol",
        "Recoge la **Madera** que ha caído",
        "Lleva los materiales a la **Hoguera**",
        "Mantén ESPACIO para **ENCENDER** el fuego"
    };

    void OnEnable()
    {
        // Nos suscribimos a todos los avisos
        PlayerInventory.OnItemCollected += CheckItemPickup;
        PlayerInventory.OnBonfireMaterialAdded += CheckBonfireUpdate;
        Tree.OnTreeHit += SiguientePasoTalar;
        Hoguera.OnBonfireLit += SiguientePaso;
    }

    void OnDisable()
    {
        PlayerInventory.OnItemCollected -= CheckItemPickup;
        PlayerInventory.OnBonfireMaterialAdded -= CheckBonfireUpdate;
        Tree.OnTreeHit -= SiguientePasoTalar; 
        Hoguera.OnBonfireLit -= SiguientePaso;
    }

    void Start() => UpdateUI();

    void Update()
    {
        // Paso 0: Movimiento
        if (_currentStep == 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
            SiguientePaso();

        // Paso 1: Zoom
        if (_currentStep == 1 && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)))
            SiguientePaso();
    }

    void CheckItemPickup() 
    {
        if (_currentStep == 2 || _currentStep == 4) SiguientePaso();
    }

    void SiguientePasoTalar()
    {
        if (_currentStep == 3) SiguientePaso();
    }

    void CheckBonfireUpdate(string material) 
    {
        if (_currentStep == 5) SiguientePaso();
    }

    void SiguientePaso()
    {
        _currentStep++;
        if (_currentStep < _instructions.Length) UpdateUI();
        else FinalizarTutorial();
    }

    void UpdateUI() => tutorialText.text = _instructions[_currentStep];

    void FinalizarTutorial()
    {
        tutorialText.text = "Tutorial completado";
        Invoke("OcultarTexto", 4f);
        this.enabled = false;
    }

    void OcultarTexto() => tutorialText.gameObject.SetActive(false);
}