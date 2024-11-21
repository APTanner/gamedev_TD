using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanel : MonoBehaviour
{
    private CanvasGroup m_group;

    private void Awake()
    {
        m_group = GetComponent<CanvasGroup>();
    }

    public virtual void EnablePanel()
    {
        m_group.alpha = 1;
        m_group.interactable = true;
        m_group.blocksRaycasts = true;
    }

    public virtual void DisablePanel()
    {
        m_group.alpha = 0;
        m_group.interactable = false;
        m_group.blocksRaycasts = false;
    }
}
