using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToggleButtonsManager : MonoBehaviour
{
    // Define the colors to toggle between
    public Color normalColor = Color.white;
    public Color clickedColor = Color.green;

    // Dictionary to store the button's clicked state
    private Dictionary<Button, bool> buttonStates = new Dictionary<Button, bool>();

    // Store the currently highlighted button
    private Button currentHighlightedButton = null;

    void Start()
    {
        // Get all Button components in the children of this object
        Button[] buttons = GetComponentsInChildren<Button>();

        foreach (Button button in buttons)
        {
            // Initialize each button's state and color
            buttonStates[button] = false;
            button.image.color = normalColor;

            // Add a listener to each button to toggle its color on click
            button.onClick.AddListener(() => ToggleButtonColor(button));
        }
    }

    // Toggle the button's color based on its current state, ensuring only one button is highlighted at once
    void ToggleButtonColor(Button clickedButton)
    {
        // If the clicked button is already highlighted, deselect it
        if (currentHighlightedButton == clickedButton)
        {
            clickedButton.image.color = normalColor;
            currentHighlightedButton = null;
            buttonStates[clickedButton] = false;
        }
        else
        {
            // If there is another button highlighted, reset its color
            if (currentHighlightedButton != null)
            {
                currentHighlightedButton.image.color = normalColor;
                buttonStates[currentHighlightedButton] = false;
            }

            // Highlight the newly clicked button
            clickedButton.image.color = clickedColor;
            currentHighlightedButton = clickedButton;
            buttonStates[clickedButton] = true;
        }
    }
}
