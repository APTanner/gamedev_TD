using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    const string MASTER_MIXERID = "MasterVolume";
    const string MUSIC_MIXERID = "MusicVolume";
    const string EFFECTS_MIXERID = "EffectVolume";

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

        Switchboard.OnWaveStart += Switchboard_OnWaveStart;
        Switchboard.OnWaveEnd += Switchboard_OnWaveEnd;

        Switchboard.OnMasterVolumeChanged += Switchboard_OnMasterVolumeChanged;
        Switchboard.OnMusicVolumeChanged += Switchboard_OnMusicVolumeChanged;
        Switchboard.OnEffectVolumeChanged += Switchboard_OnEffectVolumeChanged;

        Switchboard.OnHQHealthChanged += Switchboard_OnHQHealthChanged;

        SceneManager_sceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }



    protected void Start()
    {
        Switchboard_OnMasterVolumeChanged(1);
        Switchboard_OnMusicVolumeChanged(1);
        Switchboard_OnEffectVolumeChanged(1);
    }

    protected void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

        Switchboard.OnMasterVolumeChanged -= Switchboard_OnMasterVolumeChanged;
        Switchboard.OnMusicVolumeChanged -= Switchboard_OnMusicVolumeChanged;
        Switchboard.OnEffectVolumeChanged -= Switchboard_OnEffectVolumeChanged;

        Switchboard.OnHQHealthChanged -= Switchboard_OnHQHealthChanged;
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

    private void Switchboard_OnEffectVolumeChanged(float volume)
    {
        Mixer.SetFloat(EFFECTS_MIXERID, volume == 0 ? -100 : Mathf.Log10(volume * Defines.EffectBaseVolume) * 20);
    }

    private void Switchboard_OnMusicVolumeChanged(float volume)
    {
        Debug.Log(volume == 0 ? -100 : Mathf.Log10(volume * Defines.MusicBaseVolume) * 20);
        Mixer.SetFloat(MUSIC_MIXERID, volume == 0 ? -100 : Mathf.Log10(volume * Defines.MusicBaseVolume) * 20);
    }

    private void Switchboard_OnMasterVolumeChanged(float volume)
    {
        Mixer.SetFloat(MASTER_MIXERID, volume == 0 ? -100 : Mathf.Log10(volume) * 20);
    }

    [Header("Mixer")]
    [SerializeField] private AudioMixer Mixer;

    [Header("Music")]
    [SerializeField] private AudioSource MenuTheme;
    [SerializeField] private string MenuSceneName;
    [SerializeField] private AudioSource BattleTheme;
    [SerializeField] private AudioSource BuildingTheme;
    [SerializeField] private float FadeDuration;

    [Header("Effects")]
    [SerializeField] private AudioMixerGroup EffectAudioGroup;
    [SerializeField] private AudioSource ButtonClick;
    [SerializeField] private AudioSource HQDamageWarning;

    private bool m_bInMenu;
    private float m_battleMusicTime;

    public void PlayEffectAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        GameObject gameObject = new GameObject("One shot audio");
        gameObject.transform.position = position;
        AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = EffectAudioGroup;
        audioSource.spatialBlend = 1f;
        audioSource.volume = volume;
        audioSource.Play();
        Object.Destroy(gameObject, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
    }

    public void PlayButtonClick()
    {
        ButtonClick.PlayOneShot(ButtonClick.clip);
    }

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

    private void Switchboard_OnHQHealthChanged(int health)
    {
        if (health == Defines.HQMaxHealth || HQDamageWarning.isPlaying)
        {
            return;
        }

        HQDamageWarning.Play();
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
