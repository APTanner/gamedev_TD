using UnityEngine;

public class ArtyController : TurretController
{
    protected override bool RotateToFaceTarget()
    {
        Vector3 targetPosition = m_target.lastPosition;
        Vector3 targetVelocity = m_target.rigidbody.linearVelocity;

        float g = Mathf.Abs(Physics.gravity.y);
        float timeToImpact = MathFunctions.Sqrt2 * bulletSpeed / g;

        Vector3 predictedPosition = targetPosition + targetVelocity * timeToImpact;

        // Horizontal rotation (Y-axis)
        Vector3 horizontalDirection = new Vector3(predictedPosition.x - transform.position.x, 0f, predictedPosition.z - transform.position.z).normalized;
        Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
        turretBase.rotation = Quaternion.RotateTowards(turretBase.rotation, horizontalLookRotation, Time.fixedDeltaTime * rotationSpeed);

        float angleFromTarget = Quaternion.Angle(turretBase.rotation, horizontalLookRotation);

        Vector2 toAimPoint = predictedPosition.xz() - transform.position.xz();
        float distanceToAimPoint = toAimPoint.magnitude;
        toAimPoint /= distanceToAimPoint;

        float horizontalVelocityProportion = distanceToAimPoint / (bulletSpeed * timeToImpact);
        if (horizontalVelocityProportion > 1)
        {
            // This happens when the bullet lacks the rangle to get there in time
            // It is impossible to find an elevation where it will hit
            // Probably need to pick a different target at this point
            return false;
        }

        float elevationAngle = Mathf.Acos(horizontalVelocityProportion);
        elevationAngle *= Mathf.Rad2Deg;
        Quaternion elevationRotation = Quaternion.Euler(-elevationAngle, 0f, 0f);
        gunPivot.localRotation = Quaternion.RotateTowards(gunPivot.localRotation, elevationRotation, Time.fixedDeltaTime * rotationSpeed);

        float verticalAngleFromTarget = Quaternion.Angle(gunPivot.localRotation, elevationRotation);

        return Mathf.Max(angleFromTarget, verticalAngleFromTarget) <= Defines.TurretAimTolerance;
    }

    // targets will be found by the turret manager -> just don't do anything
    protected override void FindTarget()
    {
        return;
    }

    protected override bool IsTargetValid()
    {
        if (m_target.IsValid && m_target.IsAlive())
        {
            m_target.UpdateTarget();
            if ((m_target.lastPosition - transform.position).sqrMagnitude <= detectionRange * detectionRange)
            {
                return true;
            }
        }
        m_target.Invalidate();
        return false;
    }

    public override void UpdateTurret()
    {
        if (IsPreview)
        {
            return;
        }

        fireCooldown -= Time.fixedDeltaTime;

        if (!HasTarget && !FindCloseTarget())
        {
            FindTarget();
        }

        if (m_target.IsValid)
        {
            bool bIsAimed = RotateToFaceTarget();
            if (fireCooldown <= 0f && bIsAimed)
            {
                Fire();
                // get a new target after we fire
                m_target.Invalidate();
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            OnStopFiring();
        }
    }
}
