using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonsManager : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color clickedColor = Color.green;

    private Button currentHighlightedButton = null;
    public BuildingManager buildingManager;

    protected void OnEnable()
    {
        Switchboard.OnWaveStart += Switchboard_OnWaveStart;
    }

    protected void OnDisable()
    {
        Switchboard.OnWaveStart -= Switchboard_OnWaveStart;
    }

    private void Switchboard_OnWaveStart(int obj)
    {
        if (currentHighlightedButton == null)
        {
            return;
        }
        if (currentHighlightedButton.GetComponent<BuildingButton>().isBuildingButton)
        {
            ToggleButtonColor(currentHighlightedButton, null);
        }
        else
        {
            buildingManager.ToggleSellingMode();
            ToggleNonBuildingButton(currentHighlightedButton);
        }
    }

    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        int i = 0;

        foreach (Button button in buttons)
        {
            BuildingButton buildingButton = button.GetComponent<BuildingButton>();

            if (buildingButton != null)
            {
                button.image.color = normalColor;
                if (buildingButton.isBuildingButton)
                {
                    button.onClick.AddListener(() => ToggleButtonColor(button, buildingButton.buildingPrefab));
                }
                else
                {
                    button.onClick.AddListener(() => ToggleNonBuildingButton(button));
                }
            }
            i++;
        }
    }

    void ToggleButtonColor(Button clickedButton, IBuilding buildablePrefab)
    {
        if (currentHighlightedButton == clickedButton)
        {
            clickedButton.image.color = normalColor;
            currentHighlightedButton = null;
            buildingManager.StopPlacingObject();
        }
        else
        {
            if (currentHighlightedButton != null && !currentHighlightedButton.GetComponent<BuildingButton>().isBuildingButton)
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

        if (currentHighlightedButton != null && currentHighlightedButton != clickedButton && !currentHighlightedButton.GetComponent<BuildingButton>().isBuildingButton)
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
