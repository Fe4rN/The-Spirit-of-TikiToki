using UnityEngine;
using System.Collections; // Necesario para las Corrutinas

public class MascotaMenu : MonoBehaviour
{
    [Header("Ajuste de Sensibilidad")]
    public float divisorDistancia = 2f;

    [Header("Límites de Rotación")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minY = 140f;
    public float maxY = 230f;

    [Header("Estado Enfado")]
    public GameObject objetoEnfado;
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

        if (objetoEnfado != null) objetoEnfado.SetActive(false);
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
        if (objetoEnfado != null) objetoEnfado.SetActive(enfadado);
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
        // Esperamos la duración exacta del clip de animación
        yield return new WaitForSeconds(animacionFinal.length);

        Debug.Log("Cerrando aplicación...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}