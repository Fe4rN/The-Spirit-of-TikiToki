using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TikiToki.UI;

namespace TikiToki.Gameplay
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "WinLose")]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Match Configuration")]
        [UnityEngine.Serialization.FormerlySerializedAs("tiempoMaximo")]
        public float maxTime = 120f;

        [Header("State Monitor (Inspector)")]
        [UnityEngine.Serialization.FormerlySerializedAs("vidasActuales")]
        public int currentLives;
        [UnityEngine.Serialization.FormerlySerializedAs("tiempoRestante")]
        public float timeRemaining;
        [UnityEngine.Serialization.FormerlySerializedAs("juegoTerminado")]
        public bool isGameOver = false;

        [Header("UI References: Time and Lives")]
        [UnityEngine.Serialization.FormerlySerializedAs("textoTiempo")]
        public TextMeshProUGUI timeText;

        [Header("UI References: Dual Bar (Progress)")]
        [UnityEngine.Serialization.FormerlySerializedAs("barraIzquierda")]
        public Image leftBar;  // Fill Origin: Right
        [UnityEngine.Serialization.FormerlySerializedAs("barraDerecha")]
        public Image rightBar; // Fill Origin: Left

        [Header("Navigation System")]
        [UnityEngine.Serialization.FormerlySerializedAs("scriptPausa")]
        public PauseController pauseController;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            timeRemaining = maxTime;
            isGameOver = false;
        }

        private void Start()
        {
            if (pauseController != null)
            {
                if (pauseController.victoryPanel != null) pauseController.victoryPanel.SetActive(false);
                if (pauseController.defeatPanel != null) pauseController.defeatPanel.SetActive(false);
            }

            UpdateDualBarUI(50f);
            UpdateUI();
        }

        private void OnEnable()
        {
            ProgressBar.OnProgressChanged += ValidateProgress;
        }

        private void OnDisable()
        {
            ProgressBar.OnProgressChanged -= ValidateProgress;
        }

        private void Update()
        {
            if (isGameOver) return;

            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateUI();
            }
            else
            {
                EndGame(false, "Time's up!");
            }
        }

        public void ValidateProgress(float progressValue)
        {
            if (isGameOver) return;

            UpdateDualBarUI(progressValue);

            if (progressValue >= 100f) EndGame(true, "Full Light!");
            else if (progressValue <= 0f) EndGame(false, "Total Darkness.");
        }

        private void UpdateDualBarUI(float value)
        {
            if (leftBar == null || rightBar == null) return;
            value = Mathf.Clamp(value, 0, 100);

            if (value >= 50)
            {
                leftBar.fillAmount = 0;
                rightBar.fillAmount = (value - 50) / 50f;
            }
            else
            {
                rightBar.fillAmount = 0;
                leftBar.fillAmount = 1f - (value / 50f);
            }
        }

        private void UpdateUI()
        {
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                timeText.text = string.Format("{0}:{1:00}", minutes, seconds);
            }
        }

        public void EndGame(bool victory, string message)
        {
            if (isGameOver) return;
            isGameOver = true;

            if (pauseController == null)
            {
                Debug.LogError("No PauseController reference in GameManager.");
                return;
            }

            if (victory)
            {
                pauseController.ActivateEndPanel(pauseController.victoryPanel);
            }
            else
            {
                pauseController.ActivateEndPanel(pauseController.defeatPanel);
            }

            Debug.Log("<color=white>Final State: </color>" + message);
        }
    }
}
