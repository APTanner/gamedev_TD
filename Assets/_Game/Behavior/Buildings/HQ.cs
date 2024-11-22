using UnityEngine;

public class HQ : BuildableObject
{
    public override Vector2Int Size => new Vector2Int(2, 2);

    protected override void Awake()
    {
        base.Awake();
        Health = Defines.HQMaxHealth;
        IsSellable = false;
    }

    private void Start()
    {
        UpdateHealthUI();
    }

    public void TakeCollisionDamage(int damage)
    {
        base.TakeDamage(damage); // Calls the parent class logic
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        Switchboard.HQHealthChanged(Health);
    }
}
