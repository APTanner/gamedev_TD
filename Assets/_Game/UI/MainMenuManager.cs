using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public enum MenuState { StartMenu, Settings, Controls }

    public GameObject[] panels; // 0: StartMenu, 1: Settings, 2: Controls
    public Button startButton, controlsButton, settingsButton, closeControlsButton, closeSettingsButton, exitButton;

    void Start()
    {
        // Initialize button listeners programmatically
        startButton.onClick.AddListener(StartGame);
        controlsButton.onClick.AddListener(() => ChangeMenu(MenuState.Controls));
        settingsButton.onClick.AddListener(() => ChangeMenu(MenuState.Settings));
        closeControlsButton.onClick.AddListener(() => ChangeMenu(MenuState.StartMenu));
        closeSettingsButton.onClick.AddListener(() => ChangeMenu(MenuState.StartMenu));

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            exitButton.gameObject.SetActive(false); // Hide the Exit button in WebGL
        }
        else
        {
            exitButton.gameObject.SetActive(true);
            exitButton.onClick.AddListener(ExitGame);
        }

        // Set the initial menu state
        ChangeMenu(MenuState.StartMenu);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
        //Debug.Log("Starting the game...");
    }

    private void ChangeMenu(MenuState state)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == (int)state);
        }
    }
    public void ExitGame()
    {
        Debug.Log("Exiting the game...");
        Application.Quit();
    }
}
