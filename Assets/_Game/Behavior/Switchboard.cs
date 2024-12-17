using System;

public static class Switchboard
{
    public static event Action<int> OnWaveStart;
    public static void WaveStart(int wave)
    {
        OnWaveStart?.Invoke(wave);
    }

    // The index of the wave after the one that just ended
    public static event Action<int> OnWaveEnd;
    public static void WaveEnd(int wave)
    {
        OnWaveEnd?.Invoke(wave);
    }

    public static event Action OnWin;
    public static void Win()
    {
        OnWin?.Invoke();
    }

    public static event Action OnLose;
    public static void Lose()
    {
        OnLose?.Invoke();
    }

    // The current level index
    public static event Action<int> OnLevelStart;
    public static void LevelStart(int levelIndex)
    {
        OnLevelStart?.Invoke(levelIndex);
    }

    public static event Action<int> OnHQHealthChanged;
    public static int HQHealth { get; private set; }
    public static void HQHealthChanged(int health)
    {
        HQHealth = health;
        OnHQHealthChanged?.Invoke(health);
    }

    public static event Action<float> OnMasterVolumeChanged;
    public static float MasterVolume { get; private set; } = 1;
    public static void MasterVolumeChanged(float volume)
    {
        MasterVolume = volume;
        OnMasterVolumeChanged?.Invoke(MasterVolume);
        EffectVolumeChanged(EffectVolume);
        MusicVolumeChanged(MusicVolume);
    }

    public static event Action<float> OnEffectVolumeChanged;
    public static float EffectVolume { get; private set; } = 1;
    public static void EffectVolumeChanged(float volume)
    {
        EffectVolume = volume * MasterVolume * Defines.EffectBaseVolume;
        OnEffectVolumeChanged?.Invoke(EffectVolume);
    }

    public static event Action<float> OnMusicVolumeChanged;
    public static float MusicVolume { get; private set; } = 1;
    public static void MusicVolumeChanged(float volume)
    {
        MusicVolume = volume * MasterVolume * Defines.MusicBaseVolume;
        OnMusicVolumeChanged?.Invoke(MusicVolume);
    }
}
