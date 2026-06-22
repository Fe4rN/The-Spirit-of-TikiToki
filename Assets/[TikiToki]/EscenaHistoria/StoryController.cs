using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TikiToki.UI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "ControladorHistoria")]
    public class StoryController : MonoBehaviour
    {
        [Header("Data")]
        [UnityEngine.Serialization.FormerlySerializedAs("historia")]
        public StoryData storyData;
        private int _currentStepIndex = 0;

        [Header("UI References")]
        [UnityEngine.Serialization.FormerlySerializedAs("displayImagen")]
        public Image displayImage;
        [UnityEngine.Serialization.FormerlySerializedAs("displayTexto")]
        public TextMeshProUGUI displayText;

        [Header("Text Settings")]
        public float typingSpeed = 0.03f;
        private Coroutine _typeRoutine;
        private bool _isTyping = false;

        void Start()
        {
            if (storyData != null && storyData.steps.Length > 0)
            {
                ShowStep();
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                HandleClick();
            }
        }

        void HandleClick()
        {
            if (_isTyping)
            {
                CompleteTextImmediately();
            }
            else
            {
                NextStep();
            }
        }

        void NextStep()
        {
            _currentStepIndex++;

            if (_currentStepIndex < storyData.steps.Length)
            {
                ShowStep();
            }
            else
            {
                FinishStory();
            }
        }

        void ShowStep()
        {
            StoryStep step = storyData.steps[_currentStepIndex];

            if (displayImage != null)
            {
                if (step.comicImage != null)
                {
                    displayImage.gameObject.SetActive(true);
                    displayImage.sprite = step.comicImage;
                }
                else
                {
                    displayImage.gameObject.SetActive(false);
                }
            }

            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            _typeRoutine = StartCoroutine(TypeText(step.narrativeText));
        }

        IEnumerator TypeText(string text)
        {
            _isTyping = true;
            displayText.text = text;
            displayText.maxVisibleCharacters = 0;
            displayText.ForceMeshUpdate();

            int totalVisibleCharacters = displayText.textInfo.characterCount;
            int counter = 0;

            while (counter <= totalVisibleCharacters)
            {
                displayText.maxVisibleCharacters = counter;
                counter++;
                yield return new WaitForSeconds(typingSpeed);
            }

            _isTyping = false;
        }

        void CompleteTextImmediately()
        {
            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            displayText.maxVisibleCharacters = displayText.textInfo.characterCount;
            _isTyping = false;
        }

        void FinishStory()
        {
            Debug.Log("End of the story. Loading scene...");
            SceneManager.LoadScene(storyData.nextSceneName);
        }
    }
}
