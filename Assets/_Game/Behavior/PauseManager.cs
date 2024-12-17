using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MainMenuManager;

public class PauseManager : MonoBehaviour
{
    public enum MenuState { PauseMenu, Controls, Settings }
    public static PauseManager Instance { get; private set; }

    public GameObject[] panels; // 0: StartMenu, 1: Controls

    [SerializeField] private GameObject pausePanel;
    public Button controlsButton, closeControlsButton, exitButton, resumeButton, menuButton, settingsButton, closeSettingsButton;

    private bool isPaused = false;

    private void Start()
    {
        // Hide the pause panel at start
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        controlsButton.onClick.AddListener(() => ChangeMenu(MenuState.Controls));
        closeControlsButton.onClick.AddListener(() => ChangeMenu(MenuState.PauseMenu));
        settingsButton.onClick.AddListener(() => ChangeMenu(MenuState.Settings));
        closeSettingsButton.onClick.AddListener(() => ChangeMenu(MenuState.PauseMenu));

        resumeButton.onClick.AddListener(TogglePause);
        menuButton.onClick.AddListener(GoToMainMenu);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            exitButton.gameObject.SetActive(false); // Hide the Exit button in WebGL
        }
        else
        {
            exitButton.gameObject.SetActive(true);
            exitButton.onClick.AddListener(ExitGame);
        }

        ChangeMenu(MenuState.PauseMenu);
    }

    private void Update()
    {
        // Press 'P' or 'Esc' to toggle pause
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void ChangeMenu(MenuState state)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == (int)state);
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
            ChangeMenu(MenuState.PauseMenu);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
