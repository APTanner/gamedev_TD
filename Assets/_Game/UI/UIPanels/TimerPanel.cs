using UnityEngine.UI;
using TMPro;

public class TimerPanel : UIPanel
{
    private void OnEnable()
    {
        Switchboard.OnLevelStart += Switchboard_OnLevelStart;
        Switchboard.OnWaveStart += EventManager_OnWaveStart;
        Switchboard.OnWaveEnd += EventManager_OnWaveEnd;
        Switchboard.OnWaveTimeChanged += EventManager_OnWaveTimeChanged;
    }

    private void OnDisable()
    {
        Switchboard.OnLevelStart -= Switchboard_OnLevelStart;
        Switchboard.OnWaveStart -= EventManager_OnWaveStart;
        Switchboard.OnWaveEnd -= EventManager_OnWaveEnd;
        Switchboard.OnWaveTimeChanged -= EventManager_OnWaveTimeChanged;
    }

    private void EventManager_OnWaveStart(int wave)
    {
        EnablePanel();
    }

    private void Switchboard_OnLevelStart(int obj)
    {
        DisablePanel();
    }

    private void EventManager_OnWaveEnd(int wave)
    {
        if (GameManager.State != GameState.LevelOver)
        {
            DisablePanel();
        }
    }

    private void EventManager_OnWaveTimeChanged(float time)
    {
        TMP_Text timerText = GetComponentInChildren<TMP_Text>();
        timerText.text = time.ToString("F2");
    }
}
