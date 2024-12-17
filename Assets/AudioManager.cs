using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
            return;
        }

        Destroy(this.gameObject);
    }

    private void Initialize()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        Switchboard.OnMusicVolumeChanged += Switchboard_OnMusicVolumeChanged;

        Switchboard.OnWaveStart += Switchboard_OnWaveStart;
        Switchboard.OnWaveEnd += Switchboard_OnWaveEnd;

        SceneManager_sceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    protected void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        Switchboard.OnMusicVolumeChanged -= Switchboard_OnMusicVolumeChanged;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MenuTheme.Stop();
        BattleTheme.Stop();
        BuildingTheme.Stop();

        m_battleMusicTime = 0;

        if (scene.name == MenuSceneName)
        {
            m_bInMenu = true;
            MenuTheme.Play();
        }
        else
        {
            m_bInMenu = false;
            BuildingTheme.Play();
        }
    }

    [Header("Music")]
    [SerializeField] private AudioSource MenuTheme;
    [SerializeField] private string MenuSceneName;
    [SerializeField] private AudioSource BattleTheme;
    [SerializeField] private AudioSource BuildingTheme;
    [SerializeField] private float FadeDuration;

    [Header("Effects")]

    private bool m_bInMenu;
    private float m_battleMusicTime;

    private void Switchboard_OnWaveStart(int wave)
    {
        if (m_bInMenu)
        {
            return;
        }

        BattleTheme.time = m_battleMusicTime;
        StartCoroutine(FadeMusic(BattleTheme, BuildingTheme, FadeDuration));
    }

    private void Switchboard_OnWaveEnd(int wave)
    {
        if (m_bInMenu)
        {
            return;
        }

        m_battleMusicTime = BattleTheme.time;
        StartCoroutine(FadeMusic(BuildingTheme, BattleTheme, FadeDuration));
    }

    private void Switchboard_OnMusicVolumeChanged(float volume)
    {
        MenuTheme.volume = volume;
        BattleTheme.volume = volume;
        BuildingTheme.volume = volume;
    }

    private IEnumerator FadeMusic(AudioSource fadeIn, AudioSource fadeOut, float time)
    {
        float fadeOutStartVolume = fadeOut.volume;
        float fadeInStartVolume = fadeIn.volume;

        fadeIn.volume = 0f;
        fadeIn.Play();

        for (float t = 0; t <= time; t += Time.unscaledDeltaTime)
        {
            float normalizedTime = t / time;
            fadeOut.volume = Mathf.Lerp(fadeOutStartVolume, 0f, normalizedTime);
            fadeIn.volume = Mathf.Lerp(0f, fadeInStartVolume, normalizedTime);
            yield return null;
        }

        fadeOut.volume = 0f;
        fadeIn.volume = fadeInStartVolume;
        fadeOut.Stop();

        fadeOut.volume = fadeOutStartVolume;
    }
}
