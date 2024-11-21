using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance { get; private set; }
    public int Money { get; private set; } = 500; // Starting gold

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
    }

    public void SubtractMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
        }
        else
        {
            Debug.Log("Not enough gold.");
        }
    }
}
