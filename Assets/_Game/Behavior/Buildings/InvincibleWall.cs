using UnityEngine;

public class InvincibleWalll : BuildableObject
{
    protected override void Awake()
    {
        base.Awake();
        Health = 999999999;
        Price = 0;
    }

    public override Vector2Int Size => new Vector2Int(1, 1);

}
