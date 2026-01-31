using UnityEngine;
using UnityEngine.UI;

public class ControladorBarraDual : MonoBehaviour
{
    [Header("Referencias de UI")]
    public Image barraIzquierda; // El Fill Origin debe ser Right
    public Image barraDerecha;   // El Fill Origin debe ser Left

    [Header("Valor de Prueba")]
    [Range(0, 100)] public float valorActual = 50f;

    void Update()
    {
        ActualizarBarras(valorActual);
    }

    public void ActualizarBarras(float valor)
    {
        // Limpiamos el valor entre 0 y 100
        valor = Mathf.Clamp(valor, 0, 100);

        if (valor >= 50)
        {
            // Lado Derecho (Verde)
            barraIzquierda.fillAmount = 0; // Ocultamos la izquierda

            // Mapeamos de [50, 100] a [0, 1]
            float porcentajeDerecho = (valor - 50) / 50f;
            barraDerecha.fillAmount = porcentajeDerecho;
        }
        else
        {
            // Lado Izquierdo (Rojo)
            barraDerecha.fillAmount = 0; // Ocultamos la derecha

            // Mapeamos de [0, 50] a [1, 0] 
            // (A 0 de valor, la barra debe estar llena hacia el 0)
            float porcentajeIzquierdo = 1f - (valor / 50f);
            barraIzquierda.fillAmount = porcentajeIzquierdo;
        }
    }
}