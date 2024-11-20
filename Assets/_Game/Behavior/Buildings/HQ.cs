using UnityEngine;

public class HQ : BuildableObject
{
    public override int Health { get; protected set; } = 10000;
    public override Vector2Int Size => new Vector2Int(2, 2);
}
