using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace TikiToki.UI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "Barra")]
    public class ProgressBar : MonoBehaviour
    {
        public static ProgressBar Instance { get; private set; }

        [Header("UI References")]
        [UnityEngine.Serialization.FormerlySerializedAs("barraProgreso")]
        [SerializeField] private Slider progressSlider;

        [Header("Time Settings")]
        [UnityEngine.Serialization.FormerlySerializedAs("intervaloDeTiempo")]
        [Range(0.1f, 5f)] public float timeInterval = 1.0f;
        private float _timer = 0f;

        [Header("Rate of Change Rates (Loss)")]
        [UnityEngine.Serialization.FormerlySerializedAs("puntosLentos")]
        public float slowPoints = 2f;
        [UnityEngine.Serialization.FormerlySerializedAs("puntosMedios")]
        public float mediumPoints = 5f;
        [UnityEngine.Serialization.FormerlySerializedAs("puntosRapidos")]
        public float fastPoints = 10f;

        [Header("Rate of Change Rates (Rise)")]
        [UnityEngine.Serialization.FormerlySerializedAs("puntosLentosSubida")]
        public float slowRisePoints = 4f;
        [UnityEngine.Serialization.FormerlySerializedAs("puntosMediaosSubida")]
        public float mediumRisePoints = 10f;
        [UnityEngine.Serialization.FormerlySerializedAs("puntosRapidosSubida")]
        public float fastRisePoints = 20f;

        [Header("Progress State")]
        [UnityEngine.Serialization.FormerlySerializedAs("progresoActual")]
        public float currentProgress = 50f;
        private float _pointsToApply = 0f;

        private List<TikiToki.Gameplay.Environment.Bonfire> _allBonfires = new List<TikiToki.Gameplay.Environment.Bonfire>();

        // Event for decoupled UI updates
        public static event Action<float> OnProgressChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (progressSlider != null)
            {
                progressSlider.minValue = 0;
                progressSlider.maxValue = 100;
                progressSlider.value = currentProgress;
            }
            RecalculateRateOfChange();
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= timeInterval)
            {
                ApplyPointsChange();
                _timer = 0f;
            }
        }

        public void RegisterBonfire(TikiToki.Gameplay.Environment.Bonfire bonfire)
        {
            if (!_allBonfires.Contains(bonfire))
            {
                _allBonfires.Add(bonfire);
            }
            RecalculateRateOfChange();
        }

        public void RecalculateRateOfChange()
        {
            int litCount = 0;
            int extinguishedCount = 0;

            foreach (var bonfire in _allBonfires)
            {
                if (bonfire == null) continue;
                if (bonfire.isLit) litCount++; 
                else extinguishedCount++;
            }

            int difference = litCount - extinguishedCount;
            int absoluteDifference = Mathf.Abs(difference);

            if (difference > 0)
            {
                if (absoluteDifference == 1) _pointsToApply = slowRisePoints;
                else if (absoluteDifference == 3) _pointsToApply = mediumRisePoints;
                else if (absoluteDifference == 5) _pointsToApply = fastRisePoints;
                else _pointsToApply = 0;
            }
            else if (difference < 0)
            {
                if (absoluteDifference == 1) _pointsToApply = -slowPoints;
                else if (absoluteDifference == 3) _pointsToApply = -mediumPoints;
                else if (absoluteDifference == 5) _pointsToApply = -fastPoints;
                else _pointsToApply = 0;
            }
            else
            {
                _pointsToApply = 0;
            }
        }

        private void ApplyPointsChange()
        {
            if (_pointsToApply == 0) return;

            currentProgress += _pointsToApply;
            currentProgress = Mathf.Clamp(currentProgress, 0, 100);

            if (progressSlider != null)
                progressSlider.value = currentProgress;

            OnProgressChanged?.Invoke(currentProgress);
        }
    }
}
