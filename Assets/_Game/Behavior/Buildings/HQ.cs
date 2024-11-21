using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HQ : BuildableObject
{
    public override Vector2Int Size => new Vector2Int(2, 2);

    private Slider healthSlider; // Reference to the slider
    private TMP_Text healthText; // Reference to the health text
    private int maxHealth;

    protected override void Awake()
    {
        base.Awake();
        Health = 10000;
        maxHealth = Health;
        IsSellable = false;

        // Find and assign the UI elements
        FindUIElements();

        // Initialize UI elements
        UpdateHealthUI();
    }

    private void FindUIElements()
    {
        GameObject canvas = GameObject.Find("MainCanvas");
        if (canvas == null)
        {
            Debug.LogError("MainCanvas not found. Make sure it exists in the scene.");
            return;
        }

        Transform healthPanel = canvas.transform.Find("MainHUD/TopLeftPanel/HealthPanel");
        if (healthPanel == null)
        {
            Debug.LogError("HealthPanel not found. Make sure the hierarchy is correct.");
            return;
        }

        // Assign the slider and text
        healthSlider = healthPanel.Find("HQHealthSlider")?.GetComponent<Slider>();
        healthText = healthPanel.Find("HQHealthNum")?.GetComponent<TMP_Text>();

        if (healthSlider == null)
        {
            Debug.LogError("HQHealthSlider not found. Make sure it exists in the HealthPanel.");
        }

        if (healthText == null)
        {
            Debug.LogError("HQHealthNum not found. Make sure it exists in the HealthPanel.");
        }
    }

    public void TakeCollisionDamage(int damage)
    {
        base.TakeDamage(damage); // Calls the parent class logic
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)Health / maxHealth; // Update slider value
        }

        if (healthText != null)
        {
            healthText.text = $"{Health}/{maxHealth}"; // Update health counter text
        }
    }
}
