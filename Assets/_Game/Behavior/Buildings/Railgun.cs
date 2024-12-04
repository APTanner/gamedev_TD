using UnityEngine;

public class Railgun : BuildableObject
{
    protected override void Awake()
    {
        base.Awake();
        Health = 1000;
        Price = 250;
    }
}
