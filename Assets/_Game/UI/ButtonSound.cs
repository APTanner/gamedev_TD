using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    private Button m_button;

    protected void Awake()
    {
        m_button = GetComponent<Button>();
    }

    protected void OnEnable()
    {
        m_button.onClick.AddListener(OnClick);
    }

    protected void OnDisable()
    {
        m_button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        AudioManager.Instance.PlayButtonClick();
    }
}
