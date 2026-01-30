using UnityEngine;
using UnityEngine.UI;

public class TreeHealthBarTree : MonoBehaviour
{
    public Slider slider;
    public Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        gameObject.SetActive(false); // Oculta la barra al empezar
    }

    // Actualiza el valor y muestra la barra
    public void SetHealth(int current, int max)
    {
        gameObject.SetActive(true);
        slider.maxValue = max;
        slider.value = current;
    }

    // Efecto Billboard: La barra siempre mira a la cámara
    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}