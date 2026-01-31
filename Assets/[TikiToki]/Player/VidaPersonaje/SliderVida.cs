using UnityEngine;
using UnityEngine.UI;

public class SliderVida : MonoBehaviour
{
    public Slider slider;

    public void SetVidaMax(int vida)
    {
        slider.maxValue = vida;
        slider.value = vida;
    }
    public void SetSlider(int vida)
    {
        slider.value = vida;
    }
}
