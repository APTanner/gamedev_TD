using TMPro;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance { get; private set; }
    public int Money { get; private set; }

    [SerializeField] private TMP_Text moneyText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        UpdateMoneyUI();
    }

    public void SubtractMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            UpdateMoneyUI();
        }
        else
        {
            Debug.Log("Not enough money.");
        }
    }

    public void ResetMoney()
    {
        Money = 0;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{Money}";
        }
        else
        {
            Debug.LogWarning("Money Text UI is not assigned in the inspector.");
        }
    }
}
