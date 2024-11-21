using UnityEngine;

public class Cannon : BuildableObject
{
    protected override void Awake()
    {
        base.Awake();
        Health = 1000;
        Price = 200;
    }
}
