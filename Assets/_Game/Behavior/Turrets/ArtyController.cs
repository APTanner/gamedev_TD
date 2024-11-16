using UnityEngine;

public class ArtyController : TurretController
{
    protected override void RotateToFaceTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 targetPosition = currentTarget.GetComponent<Rigidbody>().position;
        Vector3 targetVelocity = currentTarget.GetComponent<Rigidbody>().linearVelocity;

        float g = Mathf.Abs(Physics.gravity.y);
        float timeToImpact = MathFunctions.Sqrt2 * bulletSpeed / g;

        Vector3 predictedPosition = targetPosition + targetVelocity * timeToImpact;

        // Horizontal rotation (Y-axis)
        Vector3 horizontalDirection = new Vector3(predictedPosition.x - transform.position.x, 0f, predictedPosition.z - transform.position.z).normalized;
        Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
        turretBase.rotation = Quaternion.RotateTowards(turretBase.rotation, horizontalLookRotation, Time.deltaTime * rotationSpeed);

        Vector2 toAimPoint = predictedPosition.xz() - transform.position.xz();
        float distanceToAimPoint = toAimPoint.magnitude;
        toAimPoint /= distanceToAimPoint;

        float horizontalVelocityProportion = distanceToAimPoint / (bulletSpeed * timeToImpact);
        Debug.Log($"Time to impact: {timeToImpact}, Distance: {distanceToAimPoint}, Needed proportion: {horizontalVelocityProportion}");
        if (horizontalVelocityProportion > 1)
        {
            // This happens when the bullet lacks the rangle to get there in time
            // It is impossible to find an elevation where it will hit
            // Probably need to pick a different target at this point
            return;
        }

        float elevationAngle = Mathf.Acos(horizontalVelocityProportion);
        elevationAngle *= Mathf.Rad2Deg;
        Quaternion elevationRotation = Quaternion.Euler(-elevationAngle, 0f, 0f);
        gunPivot.localRotation = Quaternion.RotateTowards(gunPivot.localRotation, elevationRotation, Time.deltaTime * rotationSpeed);
    }
}
