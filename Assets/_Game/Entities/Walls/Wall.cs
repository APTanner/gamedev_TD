using UnityEngine;

public class Wall : BuildableObject
{
    public override int Health { get; protected set; } = 1000;
    public override Vector2Int Size => new Vector2Int(1, 1); 
}
