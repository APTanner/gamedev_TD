using UnityEngine;
using UnityEngine.UI;

public class AudioSliders : MonoBehaviour
{
    [SerializeField] private Slider m_master;
    [SerializeField] private Slider m_music;
    [SerializeField] private Slider m_effects;

    protected void OnEnable()
    {
        Debug.Log("Enabled");
        m_master.onValueChanged.AddListener(MasterValueChanged);
        m_music.onValueChanged.AddListener(MusicVolumeChanged);
        m_effects.onValueChanged.AddListener(EffectsVolumeChanged);
    }

    private void MusicVolumeChanged(float value)
    {
        Switchboard.MusicVolumeChanged(value);
    }

    private void EffectsVolumeChanged(float value)
    {
        Switchboard.EffectVolumeChanged(value);
    }

    private void MasterValueChanged(float value)
    {
        Debug.Log($"Master slider changed to value {value}");
        Switchboard.MasterVolumeChanged(value);
    }

    protected void OnDisable()
    {
        m_master.onValueChanged.RemoveListener(MasterValueChanged);
        m_music.onValueChanged.RemoveListener(MusicVolumeChanged);
        m_effects.onValueChanged.RemoveListener(EffectsVolumeChanged);
    }
}
