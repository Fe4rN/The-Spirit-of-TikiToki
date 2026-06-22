using UnityEngine;
using UnityEngine.UI;

namespace TikiToki.UI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "ControladorBarraDual")]
    public class DualBarController : MonoBehaviour
    {
        [Header("UI References")]
        [UnityEngine.Serialization.FormerlySerializedAs("barraIzquierda")]
        public Image leftBar;  // Fill Origin must be Right
        [UnityEngine.Serialization.FormerlySerializedAs("barraDerecha")]
        public Image rightBar; // Fill Origin must be Left

        [Header("Testing Value")]
        [UnityEngine.Serialization.FormerlySerializedAs("valorActual")]
        [Range(0, 100)] public float currentValue = 50f;

        void Update()
        {
            UpdateBars(currentValue);
        }

        public void UpdateBars(float value)
        {
            value = Mathf.Clamp(value, 0, 100);

            if (value >= 50)
            {
                leftBar.fillAmount = 0; // Hide left

                float rightPercentage = (value - 50) / 50f;
                rightBar.fillAmount = rightPercentage;
            }
            else
            {
                rightBar.fillAmount = 0; // Hide right

                float leftPercentage = 1f - (value / 50f);
                leftBar.fillAmount = leftPercentage;
            }
        }
    }
}
