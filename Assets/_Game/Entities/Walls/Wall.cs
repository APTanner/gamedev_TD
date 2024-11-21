using UnityEngine;

public class Wall : BuildableObject
{
    protected override void Awake()
    {
        base.Awake();
        Health = 1000;
        Price = 50;
    }

    public override Vector2Int Size => new Vector2Int(1, 1);

}
