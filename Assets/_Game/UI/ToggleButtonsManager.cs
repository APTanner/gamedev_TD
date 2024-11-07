using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonsManager : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color clickedColor = Color.green;

    private Button currentHighlightedButton = null;
    public BuildingManager buildingManager; 

    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();

        foreach (Button button in buttons)
        {
            button.image.color = normalColor;

            BuildingButton buildingButton = button.GetComponent<BuildingButton>();

            if (buildingButton != null)
            {
                button.onClick.AddListener(() => ToggleButtonColor(button, buildingButton.buildingPrefab));
            }
            else 
            {
                button.onClick.AddListener(() => ToggleNonBuildingButton(button));
            }
        }
    }

    void ToggleButtonColor(Button clickedButton, IBuildable buildablePrefab)
    {
        if (currentHighlightedButton == clickedButton)
        {
            clickedButton.image.color = normalColor;
            currentHighlightedButton = null;
            buildingManager.StopPlacingObject();
        }
        else
        {
            if (currentHighlightedButton != null && currentHighlightedButton.GetComponent<BuildingButton>() == null)
            {
                currentHighlightedButton.onClick.Invoke();
            }

            if (currentHighlightedButton != null)
            {
                currentHighlightedButton.image.color = normalColor;
                buildingManager.StopPlacingObject();
            }

            clickedButton.image.color = clickedColor;
            currentHighlightedButton = clickedButton;

            buildingManager.SetBuildableObject(buildablePrefab);
        }
    }

    void ToggleNonBuildingButton(Button clickedButton)
    {
        buildingManager.StopPlacingObject();

        if (currentHighlightedButton != null && currentHighlightedButton != clickedButton && currentHighlightedButton.GetComponent<BuildingButton>() == null)
        {
            currentHighlightedButton.onClick.Invoke();
        }

        if (currentHighlightedButton == clickedButton)
        {
            clickedButton.image.color = normalColor;
            currentHighlightedButton = null;
        }
        else
        {
            if (currentHighlightedButton != null)
            {
                currentHighlightedButton.image.color = normalColor;
            }

            clickedButton.image.color = clickedColor;
            currentHighlightedButton = clickedButton;

            Debug.Log($"{clickedButton.name} non-building button selected.");
        }
    }
}
