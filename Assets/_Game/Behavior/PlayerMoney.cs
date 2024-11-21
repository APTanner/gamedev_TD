using TMPro;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance { get; private set; }
    public int Money { get; private set; } = 500; 

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

    private void Start()
    {
        UpdateMoneyUI(); 
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
