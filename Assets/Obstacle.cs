using UnityEngine;

public class Obstacle : BuildableObject
{
    protected override void Awake()
    {
        base.Awake();
        Health = 1000;
        Price = 20;
    }

    public override Vector2Int Size => new Vector2Int(1, 1);

    public override bool IsSellable => false;
}
