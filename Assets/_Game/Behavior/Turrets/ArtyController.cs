using UnityEngine;
using UnityEngine.UIElements;

public class ArtyController : TurretController
{
    protected override void RotateToFaceTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 targetPosition = currentTarget.GetComponent<Rigidbody>().position;
        Vector3 targetVelocity = currentTarget.GetComponent<Rigidbody>().linearVelocity;

        Vector3 predictedPosition = CalculatePredictedPosition(targetPosition, targetVelocity);

        // Horizontal rotation (Y-axis)
        Vector3 horizontalDirection = new Vector3(predictedPosition.x - transform.position.x, 0f, predictedPosition.z - transform.position.z).normalized;
        Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
        turretBase.rotation = Quaternion.Slerp(turretBase.rotation, horizontalLookRotation, Time.deltaTime * rotationSpeed);

        // Vertical rotation (X-axis)
        Vector3 directionToTarget = predictedPosition - firePoint.position;
        float horizontalDistance = new Vector3(directionToTarget.x, 0, directionToTarget.z).magnitude;
        float verticalDistance = directionToTarget.y;

        // Calculate the angle required to hit the target
        float gravity = Mathf.Abs(Physics.gravity.y);
        float speedSquared = bulletSpeed * bulletSpeed;
        float underSqrt = speedSquared * speedSquared - gravity * (gravity * horizontalDistance * horizontalDistance + 2 * verticalDistance * speedSquared);

        if (underSqrt >= 0)
        {
            float sqrtPart = Mathf.Sqrt(underSqrt);
            float angle1 = Mathf.Atan((speedSquared + sqrtPart) / (gravity * horizontalDistance)) * Mathf.Rad2Deg;
            float angle2 = Mathf.Atan((speedSquared - sqrtPart) / (gravity * horizontalDistance)) * Mathf.Rad2Deg;

            // Choose the smaller angle for a lower trajectory
            float elevationAngle = Mathf.Min(angle1, angle2);

            // Apply the elevation angle to the gun pivot
            gunPivot.localRotation = Quaternion.Euler(-elevationAngle, 0f, 0f);
        }
        else
        {
            // Target is out of range
            Debug.LogWarning("Target is out of range.");
        }
    }



    protected override Vector3 CalculatePredictedPosition(Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 directionToTarget = targetPosition - firePoint.position;
        float distanceToTarget = directionToTarget.magnitude;
        float bulletGravity = Mathf.Abs(Physics.gravity.y);

        // Calculate the time to reach the target's horizontal distance
        float travelTime = distanceToTarget / bulletSpeed;

        // Predict the future position of the target
        Vector3 predictedPosition = targetPosition + targetVelocity * travelTime;

        // Adjust the predicted position for gravity effect
        predictedPosition.y += 0.5f * bulletGravity * travelTime * travelTime;

        return predictedPosition;
    }


}
