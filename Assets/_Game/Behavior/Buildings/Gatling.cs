using UnityEngine;

public class Gatling : BuildableObject
{
    protected override void Awake()
    {
        base.Awake();
        Health = 1000;
        Price = 100;
        //Debug.Log($"[Gatling] {gameObject.name} Initialized: Price = {Price}");
    }
}
