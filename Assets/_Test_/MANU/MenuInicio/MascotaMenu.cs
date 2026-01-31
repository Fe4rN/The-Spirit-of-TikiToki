using UnityEngine;

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

    private float distance;
    public bool estaEnfadado = false;

    void Start()
    {
        if (Camera.main != null)
            distance = Vector3.Distance(Camera.main.transform.position, transform.position) / divisorDistancia;

        if (objetoEnfado != null) objetoEnfado.SetActive(false);
    }

    void Update()
    {
        Quaternion rotacionObjetivo;

        if (estaEnfadado)
        {
            // 1. El objetivo es mirar al frente (0, 190, 0)
            rotacionObjetivo = Quaternion.Euler(0, 190, 0);
        }
        else
        {
            // 2. El objetivo es la posición del ratón con límites aplicados
            rotacionObjetivo = CalcularRotacionRaton();
        }

        // 3. Aplicar la transición suave hacia el objetivo actual (sea el ratón o el frente)
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotacionObjetivo, velocidadTransicion * Time.deltaTime);
    }

    // Esta función calcula a dónde DEBERÍA mirar, pero no aplica la rotación todavía
    Quaternion CalcularRotacionRaton()
    {
        if (Camera.main == null) return transform.localRotation;

        Vector3 mouse = Input.mousePosition;
        mouse.z = distance;
        Vector3 lookPoint = Camera.main.ScreenToWorldPoint(mouse);

        // Usamos un objeto temporal para calcular el LookAt sin mover el objeto real
        Quaternion lookRotation = Quaternion.LookRotation(lookPoint - transform.position);
        Vector3 currentRot = lookRotation.eulerAngles;

        // Corregir y limitar X e Y
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

    public void EjecutarAnimacionFinal()
    {
        Debug.Log("Acción final ejecutada");
    }
}