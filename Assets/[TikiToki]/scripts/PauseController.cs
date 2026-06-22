using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace TikiToki.UI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "ControlPausa")]
    public class PauseController : MonoBehaviour
    {
        [Header("Configuration")]
        [UnityEngine.Serialization.FormerlySerializedAs("esMenuPrincipal")]
        public bool isMainMenu = false;
        [UnityEngine.Serialization.FormerlySerializedAs("nombreEscenaMenuPrincipal")]
        public string mainMenuSceneName = "MainMenu";

        [Header("Menu Panels")]
        [UnityEngine.Serialization.FormerlySerializedAs("panelPausaPrincipal")]
        public GameObject mainPausePanel;
        [UnityEngine.Serialization.FormerlySerializedAs("panelOpciones")]
        public GameObject optionsPanel;
        [UnityEngine.Serialization.FormerlySerializedAs("panelNiveles")]
        public GameObject levelsPanel;

        [Header("End Game Panels")]
        [UnityEngine.Serialization.FormerlySerializedAs("panelVictoria")]
        public GameObject victoryPanel;
        [UnityEngine.Serialization.FormerlySerializedAs("panelDerrota")]
        public GameObject defeatPanel;

        private Stack<GameObject> menuHistory = new Stack<GameObject>();

        private void Awake()
        {
            Time.timeScale = 1f;
        }

        void Start()
        {
            InitializeAll();
        }

        private void InitializeAll()
        {
            if (mainPausePanel) mainPausePanel.SetActive(true);
            if (optionsPanel) optionsPanel.SetActive(true);
            if (levelsPanel) levelsPanel.SetActive(true);

            if (mainPausePanel) mainPausePanel.SetActive(isMainMenu);
            if (optionsPanel) optionsPanel.SetActive(false);
            if (levelsPanel) levelsPanel.SetActive(false);

            menuHistory.Clear();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscapeKey();
            }
        }

        public void HandleEscapeKey()
        {
            if (menuHistory.Count > 0)
            {
                CloseLastMenu();
            }
            else if (!isMainMenu)
            {
                PauseGame();
                OpenMenu(mainPausePanel);
            }
        }

        public void OpenMenu(GameObject menuToOpen)
        {
            if (menuToOpen == null) return;

            if (menuHistory.Count > 0)
            {
                menuHistory.Peek().SetActive(false);
            }
            else if (isMainMenu && mainPausePanel != null)
            {
                mainPausePanel.SetActive(false);
            }

            menuToOpen.SetActive(true);
            menuHistory.Push(menuToOpen);
        }

        public void CloseLastMenu()
        {
            if (menuHistory.Count == 0) return;

            GameObject closedMenu = menuHistory.Pop();
            closedMenu.SetActive(false);

            if (menuHistory.Count > 0)
            {
                menuHistory.Peek().SetActive(true);
            }
            else
            {
                if (isMainMenu)
                {
                    if (mainPausePanel) mainPausePanel.SetActive(true);
                }
                else
                {
                    ResumeGame();
                }
            }
        }

        public void PauseGame() 
        { 
            if (!isMainMenu) Time.timeScale = 0f; 
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
            if (mainPausePanel && !isMainMenu) mainPausePanel.SetActive(false);
            if (optionsPanel) optionsPanel.SetActive(false);
            if (levelsPanel) levelsPanel.SetActive(false);
            menuHistory.Clear();
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void ActivateEndPanel(GameObject endPanel)
        {
            if (endPanel == null) return;

            PauseGame();

            if (menuHistory.Count > 0) menuHistory.Peek().SetActive(false);

            endPanel.SetActive(true);
            menuHistory.Push(endPanel);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            menuHistory.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void NextLevel()
        {
            Time.timeScale = 1f;
            menuHistory.Clear();

            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                ReturnToMainMenu();
            }
        }
    }
}
