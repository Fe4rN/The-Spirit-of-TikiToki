using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class ControladorHistoria : MonoBehaviour
{
    [Header("Datos")]
    public HistoriaData historia;
    private int _indiceActual = 0;

    [Header("Referencias UI")]
    public Image displayImagen;
    public TextMeshProUGUI displayTexto;

    [Header("Ajustes de Texto")]
    public float typingSpeed = 0.03f;
    private Coroutine _typeRoutine;
    private bool _estaEscribiendo = false;

    void Start()
    {
        if (historia != null && historia.pasos.Length > 0)
        {
            MostrarPaso();
        }
    }

    void Update()
    {
        // Detecta clic izquierdo o barra espaciadora
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            GestionarClick();
        }
    }

    void GestionarClick()
    {
        // Si está escribiendo, saltamos la animación y mostramos todo el texto de golpe
        if (_estaEscribiendo)
        {
            CompletarTextoInmediatamente();
        }
        else
        {
            SiguientePaso();
        }
    }

    void SiguientePaso()
    {
        _indiceActual++;

        if (_indiceActual < historia.pasos.Length)
        {
            MostrarPaso();
        }
        else
        {
            FinalizarHistoria();
        }
    }

    void MostrarPaso()
    {
        PasoHistoria paso = historia.pasos[_indiceActual];

        // Cambiamos la imagen inmediatamente
        if (displayImagen != null) displayImagen.sprite = paso.imagenComic;

        // Iniciamos el efecto de máquina de escribir
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(TypeText(paso.textoRelato));
    }

    IEnumerator TypeText(string text)
    {
        _estaEscribiendo = true;
        displayTexto.text = text;
        displayTexto.maxVisibleCharacters = 0;
        displayTexto.ForceMeshUpdate();

        int totalVisibleCharacters = displayTexto.textInfo.characterCount;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            displayTexto.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(typingSpeed);
        }

        _estaEscribiendo = false;
    }

    void CompletarTextoInmediatamente()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        displayTexto.maxVisibleCharacters = displayTexto.textInfo.characterCount;
        _estaEscribiendo = false;
    }

    void FinalizarHistoria()
    {
        Debug.Log("Fin de la historia. Cargando escena...");
        SceneManager.LoadScene(historia.nombreEscenaSiguiente);
    }
}