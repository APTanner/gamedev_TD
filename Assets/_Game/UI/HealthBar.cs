using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider m_slider;
    private TextMeshProUGUI m_text;

    private void Awake()
    {
        m_slider = GetComponentInChildren<Slider>();
        m_text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Switchboard.OnHQHealthChanged += EventManager_OnHQHealthChanged;
    }

    private void OnDisable()
    {
        Switchboard.OnHQHealthChanged -= EventManager_OnHQHealthChanged;
    }

    private void EventManager_OnHQHealthChanged(int health)
    {
        m_slider.value = (float)health / Defines.HQMaxHealth;
        m_text.text = $"{health}/{Defines.HQMaxHealth}";
    }
}
