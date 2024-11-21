using UnityEngine;

public class Artillery : BuildableObject
{
    protected override void Awake()
    {
        base.Awake();
        Health = 1000;
        Price = 300;
    }
}
