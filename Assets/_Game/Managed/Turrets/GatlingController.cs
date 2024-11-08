using UnityEngine;

public class GatlingController : TurretController
{
    public Transform barrelTransform; 
    public float barrelRotationSpeed = 1000f; 

    private bool isFiring = false;

    private void Update()
    {
        base.Update();

        if (isFiring)
        {
            RotateBarrel();
        }
    }

    protected override void Fire()
    {
        base.Fire(); 

        isFiring = true; 

        //Debug.Log("Gatling gun firing!");
    }

    private void RotateBarrel()
    {
        if (barrelTransform != null)
        {
            barrelTransform.Rotate(Vector3.up, barrelRotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    protected override void OnStopFiring()
    {
        isFiring = false; 
    }
}
