using UnityEngine;

public class HQ : BuildableObject
{
    public override Vector2Int Size => new Vector2Int(2, 2);
    protected override void Awake()
    {
        base.Awake();
        Health = 10000;
        IsSellable = false;
    }
}
