using UnityEngine;
using UnityEngine.UI;

public class TreeHealthBarTree : MonoBehaviour
{
    public Slider slider;
    private Transform _camTransform;

    void Start()
    {
        if (Camera.main != null) _camTransform = Camera.main.transform;
        // No la desactivamos aquí, el Árbol lo hará en su Start
    }

    public void SetHealth(float current, float max)
    {
        // Quitamos el SetActive de aquí para que no haya conflictos
        slider.maxValue = max;
        slider.value = current;
    }

    void LateUpdate()
    {
        if (_camTransform == null) return;
        transform.rotation = _camTransform.rotation;
    }
}