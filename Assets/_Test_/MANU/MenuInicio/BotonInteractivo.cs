using UnityEngine;
using UnityEngine.EventSystems;

public class BotonInteractivo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MascotaMenu mascota;

    // Se ejecuta cuando el ratón entra en el área del botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mascota != null) mascota.CambiarEstadoEnfado(true);
    }

    // Se ejecuta cuando el ratón sale del área del botón
    public void OnPointerExit(PointerEventData eventData)
    {
        if (mascota != null) mascota.CambiarEstadoEnfado(false);
    }

    // Método para conectar al evento OnClick del botón en el Inspector
    public void AlPulsarBoton()
    {
        if (mascota != null) mascota.EjecutarAnimacionFinal();
    }
}