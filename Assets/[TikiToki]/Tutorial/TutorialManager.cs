using UnityEngine;
using TMPro;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using TikiToki.Inventory;
using TikiToki.Gameplay.Environment;
using Tree = TikiToki.Gameplay.Environment.Tree;

namespace TikiToki.Gameplay
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("References")]
        public TextMeshProUGUI tutorialText;
        public Camera playerCamera;

        [Header("Aesthetics")]
        public float typingSpeed = 0.03f;
        public Color highlightColor = new Color(1f, 0.8f, 0f);
        public Color successColor = new Color(0.2f, 1f, 0.2f);

        private int _currentStep = 0;
        private Coroutine _typeRoutine;

        private bool _hasWood = false;
        private bool _hasLeaves = false;
        private bool _woodInBonfire = false;
        private bool _leavesInBonfire = false;

        private string[] _instructions = {
            "Use <b>WASD</b> to move",
            "Use <b>Q</b> and <b>E</b> to Zoom",
            "Find the <b>Axe</b> and pick it up with SPACE",
            "Equip the axe and <b>CHOP</b> a tree",
            "Press <b>R</b> to drop the axe",
            "Collect the <b>Wood</b> and <b>Leaves</b>",
            "Bring the materials to the <b>Bonfire</b>",
            "Hold SPACE to <b>LIGHT</b> the fire"
        };

        void Start()
        {
            UpdateUI();
        }

        void Update()
        {
            if (_currentStep == 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)) NextStep();
            if (_currentStep == 1 && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))) NextStep();

            if (Input.GetKey(KeyCode.Q)) playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 30, Time.deltaTime * 5);
            if (Input.GetKey(KeyCode.E)) playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 60, Time.deltaTime * 5);
        }

        void UpdateUI()
        {
            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            _typeRoutine = StartCoroutine(TypeText(_instructions[_currentStep]));
        }

        void SetInstantText(string txt)
        {
            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            tutorialText.text = txt;
            tutorialText.maxVisibleCharacters = txt.Length;
        }

        IEnumerator TypeText(string text)
        {
            tutorialText.text = text;
            tutorialText.maxVisibleCharacters = 0;
            tutorialText.ForceMeshUpdate();

            int totalVisibleCharacters = tutorialText.textInfo.characterCount;
            int counter = 0;

            while (counter <= totalVisibleCharacters)
            {
                tutorialText.maxVisibleCharacters = counter;
                counter++;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        void OnEnable()
        {
            PlayerInventory.OnItemCollected += CheckItemPickup;
            PlayerInventory.OnBonfireMaterialAdded += CheckBonfireUpdate;
            PlayerInventory.OnItemDropped += CheckItemDropped;
            Tree.OnTreeDestroyed += NextStepTreeDestroyed;
            Bonfire.OnBonfireLit += NextStep;
        }

        void OnDisable()
        {
            PlayerInventory.OnItemCollected -= CheckItemPickup;
            PlayerInventory.OnBonfireMaterialAdded -= CheckBonfireUpdate;
            PlayerInventory.OnItemDropped -= CheckItemDropped;
            Tree.OnTreeDestroyed -= NextStepTreeDestroyed;
            Bonfire.OnBonfireLit -= NextStep;
        }

        void CheckItemPickup(string itemName)
        {
            if (_currentStep == 2 && itemName == "axe") NextStep();
            else if (_currentStep == 5)
            {
                if (itemName == "woodPile") _hasWood = true;
                if (itemName == "leavesPile") _hasLeaves = true;
                if (_hasWood && _hasLeaves) NextStep();
                else UpdateItemPickupSubtitle();
            }
        }

        void CheckBonfireUpdate(string material)
        {
            if (_currentStep == 6)
            {
                if (material == "woodPile") _woodInBonfire = true;
                if (material == "leavesPile") _leavesInBonfire = true;
                if (_woodInBonfire && _leavesInBonfire) NextStep();
                else UpdateBonfireSubtitle();
            }
        }

        void NextStepTreeDestroyed() 
        { 
            if (_currentStep == 3) NextStep(); 
        }

        void CheckItemDropped() 
        { 
            if (_currentStep == 4) NextStep(); 
        }

        void UpdateItemPickupSubtitle()
        {
            string txt = "";
            if (_hasWood && !_hasLeaves) txt = "Wood collected. Look for the <b>Leaves</b>!";
            if (!_hasWood && _hasLeaves) txt = "Leaves collected. Look for the <b>Wood</b>!";
            StartCoroutine(FlashColor(highlightColor));
            SetInstantText(txt);
        }

        void UpdateBonfireSubtitle()
        {
            string txt = "";
            if (_woodInBonfire) txt = "Wood added to bonfire. Missing <b>Leaves</b>!";
            if (_leavesInBonfire) txt = "Leaves added to bonfire. Missing <b>Wood</b>!";
            StartCoroutine(FlashColor(highlightColor));
            SetInstantText(txt);
        }

        void NextStep()
        {
            _currentStep++;
            StartCoroutine(FlashColor(successColor));
            if (_currentStep < _instructions.Length) UpdateUI();
            else FinishTutorial();
        }

        IEnumerator FlashColor(Color color)
        {
            tutorialText.color = color;
            float elapsed = 0;
            while (elapsed < 0.5f)
            {
                tutorialText.color = Color.Lerp(color, Color.white, elapsed / 0.5f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            tutorialText.color = Color.white;
        }

        void FinishTutorial()
        {
            SetInstantText("Tutorial Completed");
            StartCoroutine(FlashColor(successColor));
            Invoke("HideText", 4f);
            Invoke("LoadNextLevel", 4f);
            this.enabled = false;
        }

        void LoadNextLevel()
        {
            SceneManager.LoadScene("Level1");
        }

        void HideText() => tutorialText.gameObject.SetActive(false);
    }
}
