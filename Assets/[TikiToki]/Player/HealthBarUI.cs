using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void updateSlider(int value)
    {
        Debug.Log("Slider value" + value);
        slider.value = value;
    }
}
