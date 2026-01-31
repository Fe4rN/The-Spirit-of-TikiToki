using UnityEngine;
using UnityEngine.EventSystems;

public class BotonInteractivo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referencia")]
    public MascotaMenu mascota;

    // Se activa al pasar el ratón por encima
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mascota != null)
        {
            mascota.CambiarEstadoEnfado(true);
        }
    }

    // Se activa al quitar el ratón de encima
    public void OnPointerExit(PointerEventData eventData)
    {
        if (mascota != null)
        {
            mascota.CambiarEstadoEnfado(false);
        }
    }

    // Método para el OnClick del botón
    public void ClickEnBoton()
    {
        if (mascota != null)
        {
            mascota.EjecutarAnimacionFinal();
        }
    }


}