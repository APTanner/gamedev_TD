using System;
using UnityEngine;
using UnityEngine.VFX;

public class TurretController : MonoBehaviour
{
    public float detectionRange = 20f; // Range for grid-based enemy detection
    public float closeDetectionRange = 5f; // Range for sphere cast
    public float rotationSpeed = 5f;
    public float fireRate = 1f;
    public LayerMask enemyLayer;

    public float minVerticalAimDistance = 5f;

    private MonoBehaviour currentTarget;
    private float fireCooldown = 0f;

    public Transform turretBase;
    public Transform gunPivot;
    public Transform firePoint;

    public GameObject bulletPrefab;
    private float bulletSpeed;
    public VisualEffect muzzleFlashVFX;

    public bool IsPreview { get; set; } = false;

    protected virtual void Awake()
    {
        if (bulletPrefab != null)
        {
            Bullet bulletComponent = bulletPrefab.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletSpeed = bulletComponent.speed;
                //Debug.Log("Bullet speed: " + bulletSpeed);
            }
            else
            {
                Debug.LogWarning("Bullet prefab does not have a Bullet component with a speed property.");
            }
        }

        if (muzzleFlashVFX != null)
        {
            muzzleFlashVFX.Stop();
        }
    }

    protected virtual void Update()
    {
        if (IsPreview)
        {
            return;
        }

        fireCooldown -= Time.deltaTime;

        if (!FindCloseTarget())
        {
            FindGridTarget();
        }

        if (currentTarget != null)
        {
            RotateToFaceTarget();
            if (fireCooldown <= 0f && IsTargetInRange())
            {
                Fire();
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            OnStopFiring();
        }
    }

    private bool FindCloseTarget()
    {
        Collider[] closeEnemies = Physics.OverlapSphere(transform.position, closeDetectionRange, enemyLayer);
        if (closeEnemies.Length > 0)
        {
            Transform closestEnemy = closeEnemies[0].transform;
            float closestDistance = Vector3.Distance(transform.position, closestEnemy.position);

            foreach (var enemy in closeEnemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestEnemy = enemy.transform;
                    closestDistance = distance;
                }
            }

            if (closestEnemy.TryGetComponent<IBuilding>(out var building))
            {
                currentTarget = building as MonoBehaviour;
                return true;
            }
            else if (closestEnemy.TryGetComponent<SwarmerController>(out var enemyController))
            {
                currentTarget = enemyController;
                return true;
            }
        }

        currentTarget = null;
        return false;
    }

    private static readonly int[] signs = { -1, 1 };
    private void FindGridTarget()
    {
        Vector2Int turretCoord = SwarmerManager.Instance.GetCoord(transform.position);
        float gridCellSize = SwarmerManager.Instance.CellSize;
        int maxCellRange = Mathf.CeilToInt(detectionRange / gridCellSize);

        // special case to check current cell
        var list = SwarmerManager.Instance.GetEnemiesInCell(turretCoord);
        if (list.Count > 0)
        {
            currentTarget = list[0];
        }

        // exits early for searching in a certain direction
        //                     NW     NE    SW    SE    WS   WN     ES    EW
        bool[] searchSpace = { true, true, true, true, true, true, true, true };

        for (int d = 1; d <= maxCellRange; ++d)
        {

            for (int i = 0; i < searchSpace.Length; ++i)
            {
                searchSpace[i] = true;
            }

            for (int o = 0; o <= d && Array.Exists(searchSpace, v => v); ++o)
            {
                foreach (int sign in signs)
                {
                    // Check vertical samples
                    if (searchSpace[(sign+1)/2 + 0] && TryGetGridTarget(
                            turretCoord.x + (o * sign),
                            turretCoord.y + d,
                            ref searchSpace[(sign+1)/2])
                        ||
                        searchSpace[(sign+1)/2 + 2] && TryGetGridTarget(
                            turretCoord.x + (o * sign),
                            turretCoord.y - d,
                            ref searchSpace[(sign+1)/2 + 2]))
                    {
                        return;
                    }

                    // If we are checking the farthest offset (o value) then the corners have already
                    // been taken care of by the vertical samples. No need to check the horizontal 
                    // ones since they are duplicates
                    if (o == d) continue;

                    // Check horizontal samples
                    if (searchSpace[(sign+1)/2 + 4] && TryGetGridTarget(
                            turretCoord.x - d,
                            turretCoord.y + (o * sign),
                            ref searchSpace[(sign+1)/2 + 4])
                        ||
                        searchSpace[(sign+1)/2 + 6] && TryGetGridTarget(
                            turretCoord.x + d,
                            turretCoord.y + (o * sign),
                            ref searchSpace[(sign+1)/2 + 6]))
                    {
                        return;
                    }
                }
            }
        }

        currentTarget = null;
    }

    private bool TryGetGridTarget(int x, int y, ref bool continueSearch)
    {
        float cellSize = SwarmerManager.Instance.CellSize;
        float sqrRange = MathFunctions.Square(detectionRange);


        Vector2Int coord = new Vector2Int(x, y);
        Vector3 cellPosition = SwarmerManager.Instance.GetCellPosition(coord);

        // If the closests extents of the cell are beyond our reach just return false
        // This also means that any other cell in this direction at this distance will be further 
        // away, so we can stop searching in this direction
        if (Vector3.Distance(cellPosition, transform.position) - cellSize > detectionRange)
        {
            continueSearch = false;
            return false;
        }

        foreach (var target in SwarmerManager.Instance.GetEnemiesInCell(coord))
        {
            if ((target.transform.position - transform.position).sqrMagnitude <= sqrRange)
            {
                currentTarget = target;
                return true;
            }
        }
        return false;
    }

    private void RotateToFaceTarget()
    {
        if (currentTarget == null)
        {
            return;
        }

        Vector3 targetPosition;
        Vector3 targetVelocity = Vector3.zero;

        if (currentTarget is SwarmerController enemy)
        {
            targetPosition = enemy.transform.position;
            targetVelocity = enemy.GetComponent<Rigidbody>().linearVelocity;
        }
        else
        {
            return;
        }

        Vector3 predictedPosition = CalculatePredictedPosition(targetPosition, targetVelocity);

        // Horizontal rotation (Y-axis)
        Vector3 horizontalDirection = new Vector3(predictedPosition.x - transform.position.x, 0f, predictedPosition.z - transform.position.z).normalized;
        Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
        turretBase.rotation = Quaternion.Slerp(turretBase.rotation, horizontalLookRotation, Time.deltaTime * rotationSpeed);

        // Distance to the predicted position
        float distanceToTarget = Vector3.Distance(transform.position, predictedPosition);

        // Vertical rotation (X-axis)
        if (distanceToTarget > minVerticalAimDistance)
        {
            Vector3 directionToTarget = predictedPosition - firePoint.position;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.magnitude) * Mathf.Rad2Deg;

            // Smoothly reduce the angle adjustment as the target gets closer to the minimum distance
            float distanceFactor = Mathf.Clamp01((distanceToTarget - minVerticalAimDistance) / minVerticalAimDistance);
            float adjustedAngle = angle * distanceFactor;

            gunPivot.localRotation = Quaternion.Euler(-adjustedAngle, 0f, 0f);
        }
    }

    //private Vector3 CalculatePredictedPosition(Vector3 targetPosition, Vector3 targetVelocity)
    //{
    //    Vector3 directionToTarget = targetPosition - firePoint.position;
    //    float distanceToTarget = directionToTarget.magnitude;

    //    // Calculate the time it takes for the bullet to reach the target's current position
    //    float travelTime = distanceToTarget / bulletSpeed;

    //    // Predict the future position of the target based on its velocity and the calculated travel time
    //    Vector3 predictedPosition = targetPosition + targetVelocity * travelTime;

    //    return predictedPosition;
    //}

    // chatgpt calculus
    private Vector3 CalculatePredictedPosition(Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 directionToTarget = targetPosition - firePoint.position;
        float a = targetVelocity.sqrMagnitude - bulletSpeed * bulletSpeed;
        float b = 2 * Vector3.Dot(targetVelocity, directionToTarget);
        float c = directionToTarget.sqrMagnitude;

        // Solve the quadratic equation a*t^2 + b*t + c = 0
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            // No real solution, return current target position
            return targetPosition;
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDiscriminant) / (2 * a);
        float t2 = (-b - sqrtDiscriminant) / (2 * a);

        // Use the smallest positive time
        float t = Mathf.Min(t1, t2);
        if (t < 0) t = Mathf.Max(t1, t2);

        if (t < 0)
        {
            // Both times are negative, no interception possible
            return targetPosition;
        }

        // Calculate the predicted position
        return targetPosition + targetVelocity * t;
    }

    //private Vector3 CalculatePredictedPosition(Vector3 targetPosition, Vector3 targetVelocity)
    //{
    //    if (bulletPrefab.TryGetComponent<Rigidbody>(out Rigidbody bulletRigidbody))
    //    {
    //        float bulletMass = bulletRigidbody.mass;
    //        float bulletSpeed = this.bulletSpeed; // Ensure this value is correctly set

    //        // Gravity in Unity (usually -9.81 on the Y-axis)
    //        float gravity = Mathf.Abs(Physics.gravity.y);

    //        // Calculate the relative position and velocity
    //        Vector3 relativePosition = targetPosition - firePoint.position;
    //        Vector3 relativeVelocity = targetVelocity;

    //        // Horizontal distance and speed
    //        Vector3 horizontalRelativePosition = new Vector3(relativePosition.x, 0, relativePosition.z);
    //        float horizontalDistance = horizontalRelativePosition.magnitude;

    //        // Time to reach the target based on horizontal motion
    //        float horizontalTravelTime = horizontalDistance / bulletSpeed;

    //        // Vertical motion: Calculate the necessary initial vertical speed
    //        float verticalDisplacement = relativePosition.y;
    //        float initialVerticalSpeed = (verticalDisplacement + 0.5f * gravity * Mathf.Pow(horizontalTravelTime, 2)) / horizontalTravelTime;

    //        // Predicted position, adjusted for gravity
    //        Vector3 predictedPosition = targetPosition + targetVelocity * horizontalTravelTime;
    //        predictedPosition.y += initialVerticalSpeed * horizontalTravelTime - 0.5f * gravity * Mathf.Pow(horizontalTravelTime, 2);

    //        return predictedPosition;
    //    }

    //    // Fallback if bulletPrefab does not have a Rigidbody
    //    return targetPosition + targetVelocity * (detectionRange / bulletSpeed);
    //}


    private bool IsTargetInRange()
    {
        if (currentTarget == null)
        {
            return false;
        }

        Vector3 targetPosition = currentTarget is IBuilding building ? building.GetPosition() : (currentTarget as SwarmerController).transform.position;
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance <= detectionRange;
    }

    protected virtual void Fire()
    {
        if (muzzleFlashVFX != null)
        {
            muzzleFlashVFX.Play();
        }

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bulletInstance = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }

        //Debug.Log("Firing at target!");
    }

    protected virtual void OnStopFiring()
    {

    }
}
