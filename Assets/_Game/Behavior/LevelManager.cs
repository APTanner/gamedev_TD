using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public LevelData[] Levels;

    private int m_levelIndex = 0;

    private void Start()
    {
        GameManager gm = GameManager.Instance;
        gm.LevelData = Levels[0];
        gm.SetupLevel();

        Switchboard.LevelStart(m_levelIndex);
    }

    public void RestartLevel()
    {
        GameManager.Instance.SetupLevel();
        Switchboard.LevelStart(m_levelIndex);
    }

    public void NextLevel()
    {
        if (++m_levelIndex >= Levels.Length)
        {
            GoToMainMenu();
            return;
        }

        GameManager gm = GameManager.Instance;
        gm.LevelData = Levels[m_levelIndex];
        gm.SetupLevel();

        Switchboard.LevelStart(m_levelIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
