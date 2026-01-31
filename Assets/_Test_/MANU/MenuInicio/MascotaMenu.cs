using UnityEngine;

public class MascotaMenu : MonoBehaviour
{
    public bool estaEnfadado = false;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        MirarAlRaton();
    }

    void MirarAlRaton()
    {
        // Convertimos la posición del ratón a coordenadas del mundo
        Vector3 posicionRaton = Input.mousePosition;
        posicionRaton.z = 10f; // Distancia desde la cámara
        Vector3 objetivo = Camera.main.ScreenToWorldPoint(posicionRaton);

        // Calculamos la dirección
        Vector2 direccion = objetivo - transform.position;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // Aplicamos la rotación (ajusta el -90 si tu objeto mira hacia otro lado)
        transform.rotation = Quaternion.Euler(0, 0, angulo - 90f);
    }

    public void CambiarEstadoEnfado(bool enfadado)
    {
        if (estaEnfadado != enfadado) // Solo avisamos si el estado realmente cambia
        {
            estaEnfadado = enfadado;
            Debug.Log("<color=orange>Estado de la Mascota:</color> " + (estaEnfadado ? "¡ENFADADO!" : "Normal"));

            if (anim != null)
            {
                anim.SetBool("Enfadado", estaEnfadado);
            }
        }
    }

    public void EjecutarAnimacionFinal()
    {
        Debug.Log("Ejecutando animación de cierre...");
        if (anim != null) anim.SetTrigger("Cerrar");

        // Cerramos el juego (esto solo funciona en el build final, no en el editor)
        Invoke("SalirDelJuego", 1.5f); // Damos tiempo a la animación
    }

    void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}