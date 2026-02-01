using UnityEngine;
using System.Collections; // Necesario para las Corrutinas

public class MascotaMenu : MonoBehaviour
{
    [Header("Ajuste de Sensibilidad")]
    public float divisorDistancia = 2f;

    [Header("L�mites de Rotaci�n")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = 140f;
    public float maxY = 230f;

    [Header("Eyes Material Settings")]
    [SerializeField] private Renderer bodyRenderer; // The Renderer of your mesh
    [SerializeField] private int eyesMaterialIndex = 5; // The index of the eyes material in the materials array
    private Material eyesMatInstance;

    public float velocidadTransicion = 100f;

    [Header("Cierre de Juego")]
    [SerializeField] private AnimationClip animacionFinal;
    [SerializeField] private GameObject canvasDesactivar;
    private Animator _animator;
    private bool _bloqueadoPorAnimacion = false;


    private float distance;
    public bool estaEnfadado = false;

    void Start()
    {
        _animator = GetComponent<Animator>();

        if (Camera.main != null)
            distance = Vector3.Distance(Camera.main.transform.position, transform.position) / divisorDistancia;

        if (bodyRenderer != null)
        {
            // Clone the material so we don’t change the shared mesh for all instances
            Material[] mats = bodyRenderer.materials;
            eyesMatInstance = mats[eyesMaterialIndex];
            mats[eyesMaterialIndex] = eyesMatInstance; // assign back to array
            bodyRenderer.materials = mats;

            // Start neutral (black)
            eyesMatInstance.SetColor("_BaseColor", Color.black);
        }
    }


    void Update()
    {
        if (_bloqueadoPorAnimacion) return;

        Quaternion rotacionObjetivo;

        if (estaEnfadado)
        {
            rotacionObjetivo = Quaternion.Euler(0, 190, 0);
        }
        else
        {
            rotacionObjetivo = CalcularRotacionRaton();
        }

        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotacionObjetivo, velocidadTransicion * Time.deltaTime);
    }

    Quaternion CalcularRotacionRaton()
    {
        if (Camera.main == null) return transform.localRotation;

        Vector3 mouse = Input.mousePosition;
        mouse.z = distance;
        Vector3 lookPoint = Camera.main.ScreenToWorldPoint(mouse);

        Quaternion lookRotation = Quaternion.LookRotation(lookPoint - transform.position);
        Vector3 currentRot = lookRotation.eulerAngles;

        float rotX = (currentRot.x > 180) ? currentRot.x - 360 : currentRot.x;
        float clampedX = Mathf.Clamp(rotX, minX, maxX);
        float clampedY = Mathf.Clamp(currentRot.y, minY, maxY);

        return Quaternion.Euler(clampedX, clampedY, 0);
    }

    public void CambiarEstadoEnfado(bool enfadado)
    {
        estaEnfadado = enfadado;
        if (eyesMatInstance == null) return;

        eyesMatInstance.SetColor("_BaseColor", enfadado ? Color.red : Color.black);
    }


    // --- LOGICA DE CIERRE ---

    // En MascotaMenu.cs
    public void EjecutarAnimacionFinal()
    {
        if (canvasDesactivar != null)
        {
            canvasDesactivar.SetActive(false);
        }

        transform.localRotation = Quaternion.Euler(0, 190, 0);

        if (_animator != null && animacionFinal != null)
        {
            _bloqueadoPorAnimacion = true;

            Debug.Log("Iniciando secuencia de cierre...");
            _animator.SetTrigger("disparoOjos");

            StartCoroutine(EsperarYSalir());
        }
    }

    private IEnumerator EsperarYSalir()
    {
        // Esperamos la duraci�n exacta del clip de animaci�n
        yield return new WaitForSeconds(animacionFinal.length);

        Debug.Log("Cerrando aplicaci�n...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}