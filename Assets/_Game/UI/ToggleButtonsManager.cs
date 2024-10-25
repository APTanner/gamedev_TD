using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToggleButtonsManager : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color clickedColor = Color.green;

    private Button currentHighlightedButton = null;

    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();

        foreach (Button button in buttons)
        {
            button.image.color = normalColor;

            button.onClick.AddListener(() => ToggleButtonColor(button));
        }
    }

    void ToggleButtonColor(Button clickedButton)
    {
        if (currentHighlightedButton == clickedButton)
        {
            clickedButton.image.color = normalColor;
            currentHighlightedButton = null;
        }
        else
        {
            if (currentHighlightedButton != null)
            {
                currentHighlightedButton.onClick.Invoke();
            }

            clickedButton.image.color = clickedColor;
            currentHighlightedButton = clickedButton;
        }
    }
}
