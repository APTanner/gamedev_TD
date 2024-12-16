using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Button fullscreenButton;
    public GameObject resolutionPanel;
    public GameObject fullscreenPanel;

    [System.Serializable]
    public struct ResolutionOption
    {
        public int width;
        public int height;
    }

    [SerializeField]
    private ResolutionOption[] manualResolutions = new ResolutionOption[]
    {
        new ResolutionOption {width = 1280, height = 720},
        new ResolutionOption {width = 1920, height = 1080},
        new ResolutionOption {width = 2560, height = 1440},
    };

    private int currentQualityIndex;
    private int currentResolutionIndex;
    private bool isFullscreen;

    private const string QUALITY_KEY = "QualityLevel";
    private const string RESOLUTION_KEY = "ResolutionIndex";
    private const string FULLSCREEN_KEY = "IsFullscreen";

    private void Awake()
    {
        // Load settings from PlayerPrefs
        LoadSettingsFromPlayerPrefs();
    }

    private void Start()
    {
        SetupUIOptions();
        ApplyUIVisibility();
        ApplySettings();
        UpdateFullscreenButtonText();
    }

    private void SetupUIOptions()
    {
        // Setup quality dropdown
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            var qualityNames = new List<string>(QualitySettings.names);
            qualityDropdown.AddOptions(qualityNames);
            qualityDropdown.value = currentQualityIndex;
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

#if !UNITY_WEBGL
        // Setup resolution TMP_Dropdown with hardcoded options
        if (resolutionDropdown != null && manualResolutions.Length > 0)
        {
            resolutionDropdown.ClearOptions();
            var resolutionOptions = new List<string>();
            for (int i = 0; i < manualResolutions.Length; i++)
            {
                ResolutionOption opt = manualResolutions[i];
                resolutionOptions.Add($"{opt.width}x{opt.height}");
            }

            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        if (fullscreenButton != null)
        {
            fullscreenButton.onClick.AddListener(OnFullscreenToggle);
        }
#endif
    }

    private void ApplyUIVisibility()
    {
#if UNITY_WEBGL
        if (resolutionPanel != null) resolutionPanel.SetActive(false);
        if (fullscreenPanel != null) fullscreenPanel.SetActive(false);
#endif
    }

    public void OnQualityChanged(int index)
    {
        currentQualityIndex = index;
        QualitySettings.SetQualityLevel(currentQualityIndex);
        PlayerPrefs.SetInt(QUALITY_KEY, currentQualityIndex);
        PlayerPrefs.Save();
    }

    public void OnResolutionChanged(int index)
    {
#if !UNITY_WEBGL
        currentResolutionIndex = index;

        // Apply the chosen hardcoded resolution
        var chosenRes = manualResolutions[currentResolutionIndex];
        Screen.SetResolution(chosenRes.width, chosenRes.height, isFullscreen);

        PlayerPrefs.SetInt(RESOLUTION_KEY, currentResolutionIndex);
        PlayerPrefs.Save();
#endif
    }

    public void OnFullscreenToggle()
    {
#if !UNITY_WEBGL
        isFullscreen = !isFullscreen;

        var chosenRes = manualResolutions[currentResolutionIndex];
        Screen.SetResolution(chosenRes.width, chosenRes.height, isFullscreen);

        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        UpdateFullscreenButtonText();
#endif
    }

    private void UpdateFullscreenButtonText()
    {
#if !UNITY_WEBGL
        if (fullscreenButton != null)
        {
            TMP_Text buttonText = fullscreenButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = isFullscreen ? "X" : "";
            }
        }
#endif
    }

    private void LoadSettingsFromPlayerPrefs()
    {
        currentQualityIndex = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        currentResolutionIndex = PlayerPrefs.GetInt(RESOLUTION_KEY, 0);
        isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;
    }

    private void ApplySettings()
    {
        QualitySettings.SetQualityLevel(currentQualityIndex);

#if !UNITY_WEBGL
        if (manualResolutions.Length > currentResolutionIndex)
        {
            var chosenRes = manualResolutions[currentResolutionIndex];
            Screen.SetResolution(chosenRes.width, chosenRes.height, isFullscreen);
        }
#endif
    }
}
