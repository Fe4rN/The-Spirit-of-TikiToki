using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI tutorialText;
    public Camera playerCamera;

    [Header("Ajustes de Estética")]
    public float typingSpeed = 0.03f;
    public Color highlightColor = new Color(1f, 0.8f, 0f);
    public Color successColor = new Color(0.2f, 1f, 0.2f);

    private int _currentStep = 0;
    private Coroutine _typeRoutine;

    private bool _hasWood = false;
    private bool _hasLeaves = false;
    private bool _woodInBonfire = false;
    private bool _leavesInBonfire = false;

    private string[] _instructions = {
        "Usa <b>WASD</b> para moverte",
        "Usa <b>Q</b> y <b>E</b> para el Zoom",
        "Busca el <b>Hacha</b> y recógela con ESPACIO",
        "Equipa el hacha y <b>TALA</b> un árbol",
        "Pulsa <b>R</b> para soltar el hacha",
        "Recoge la <b>Madera</b> y las <b>Hojas</b>",
        "Lleva los materiales a la <b>Hoguera</b>",
        "Mantén ESPACIO para <b>ENCENDER</b> el fuego"
    };

    // --- MÉTODOS DE ACTUALIZACIÓN VISUAL CORREGIDOS ---

    void UpdateUI()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(TypeText(_instructions[_currentStep]));
    }

    void SetInstantText(string txt)
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        tutorialText.text = txt;
        // Importante: Al poner texto instantáneo, mostramos todos los caracteres
        tutorialText.maxVisibleCharacters = txt.Length;
    }

    IEnumerator TypeText(string text)
    {
        // 1. Ponemos el texto COMPLETO primero para que TMP parsee los <b> inmediatamente
        tutorialText.text = text;

        // 2. Escondemos todos los caracteres
        tutorialText.maxVisibleCharacters = 0;

        // 3. Forzamos la actualización de la malla para que TMP sepa cuántos caracteres hay
        tutorialText.ForceMeshUpdate();

        int totalVisibleCharacters = tutorialText.textInfo.characterCount;
        int counter = 0;

        // 4. Vamos aumentando el límite de caracteres visibles
        while (counter <= totalVisibleCharacters)
        {
            tutorialText.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // --- EL RESTO DE TU LÓGICA SE MANTIENE IGUAL ---

    void OnEnable()
    {
        PlayerInventory.OnItemCollected += CheckItemPickup;
        PlayerInventory.OnBonfireMaterialAdded += CheckBonfireUpdate;
        PlayerInventory.OnItemDropped += CheckItemDropped;
        Tree.OnTreeDestroyed += SiguientePasoArbolRoto;
        Hoguera.OnBonfireLit += SiguientePaso;
    }

    void OnDisable()
    {
        PlayerInventory.OnItemCollected -= CheckItemPickup;
        PlayerInventory.OnBonfireMaterialAdded -= CheckBonfireUpdate;
        PlayerInventory.OnItemDropped -= CheckItemDropped;
        Tree.OnTreeDestroyed -= SiguientePasoArbolRoto;
        Hoguera.OnBonfireLit -= SiguientePaso;
    }

    void Start() => UpdateUI();

    void Update()
    {
        if (_currentStep == 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)) SiguientePaso();
        if (_currentStep == 1 && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))) SiguientePaso();

        if (Input.GetKey(KeyCode.Q)) playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 30, Time.deltaTime * 5);
        if (Input.GetKey(KeyCode.E)) playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 60, Time.deltaTime * 5);
    }

    void CheckItemPickup(string itemName)
    {
        if (_currentStep == 2 && itemName == "Axe") SiguientePaso();
        else if (_currentStep == 5)
        {
            if (itemName == "woodPile") _hasWood = true;
            if (itemName == "leavesPile") _hasLeaves = true;
            if (_hasWood && _hasLeaves) SiguientePaso();
            else ActualizarSubtareaRecogida();
        }
    }

    void CheckBonfireUpdate(string material)
    {
        if (_currentStep == 6)
        {
            if (material == "woodPile") _woodInBonfire = true;
            if (material == "leavesPile") _leavesInBonfire = true;
            if (_woodInBonfire && _leavesInBonfire) SiguientePaso();
            else ActualizarSubtareaHoguera();
        }
    }

    void SiguientePasoArbolRoto() { if (_currentStep == 3) SiguientePaso(); }
    void CheckItemDropped() { if (_currentStep == 4) SiguientePaso(); }

    void ActualizarSubtareaRecogida()
    {
        string txt = "";
        if (_hasWood && !_hasLeaves) txt = "Madera recogida. ¡Busca las <b>Hojas</b>!";
        if (!_hasWood && _hasLeaves) txt = "Hojas recogidas. ¡Busca la <b>Madera</b>!";
        StartCoroutine(FlashColor(highlightColor));
        SetInstantText(txt);
    }

    void ActualizarSubtareaHoguera()
    {
        string txt = "";
        if (_woodInBonfire) txt = "Madera en la hoguera. ¡Faltan las <b>Hojas</b>!";
        if (_leavesInBonfire) txt = "Hojas en la hoguera. ¡Falta la <b>Madera</b>!";
        StartCoroutine(FlashColor(highlightColor));
        SetInstantText(txt);
    }

    void SiguientePaso()
    {
        _currentStep++;
        StartCoroutine(FlashColor(successColor));
        if (_currentStep < _instructions.Length) UpdateUI();
        else FinalizarTutorial();
    }

    IEnumerator FlashColor(Color color)
    {
        tutorialText.color = color;
        float elapsed = 0;
        while (elapsed < 0.5f)
        {
            tutorialText.color = Color.Lerp(color, Color.white, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        tutorialText.color = Color.white;
    }

    void FinalizarTutorial()
    {
        SetInstantText("Tutorial completado");
        StartCoroutine(FlashColor(successColor));
        Invoke("OcultarTexto", 4f);
        this.enabled = false;
    }

    void OcultarTexto() => tutorialText.gameObject.SetActive(false);
}