using System.Collections.Generic;
using UnityEngine;

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
    public ParticleSystem muzzleFlash; 

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
    }

    protected virtual void Update()
    {
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

    private void FindGridTarget()
    {
        Vector2Int turretCoord = SwarmerManager.Instance.GetCoord(transform.position);
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(turretCoord);

        while (queue.Count > 0)
        {
            Vector2Int currentCoord = queue.Dequeue();
            if (visited.Contains(currentCoord)) continue;
            visited.Add(currentCoord);

            Vector3 cellCenter = new Vector3(currentCoord.x * SwarmerManager.Instance.CellSize, 0, currentCoord.y * SwarmerManager.Instance.CellSize);
            float distance = Vector3.Distance(transform.position, cellCenter);

            if (distance > detectionRange) continue; 

            foreach (SwarmerController swarmer in SwarmerManager.Instance.GetEnemiesInCell(currentCoord))
            {
                if (swarmer != null)
                {
                    currentTarget = swarmer;
                    return;
                }
            }

            Vector2Int[] neighbors = {
                new Vector2Int(currentCoord.x + 1, currentCoord.y),
                new Vector2Int(currentCoord.x - 1, currentCoord.y),
                new Vector2Int(currentCoord.x, currentCoord.y + 1),
                new Vector2Int(currentCoord.x, currentCoord.y - 1)
            };

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        currentTarget = null; 
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

    private Vector3 CalculatePredictedPosition(Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 directionToTarget = targetPosition - firePoint.position;
        float distanceToTarget = directionToTarget.magnitude;

        // Calculate the time it takes for the bullet to reach the target's current position
        float travelTime = distanceToTarget / bulletSpeed;

        // Predict the future position of the target based on its velocity and the calculated travel time
        Vector3 predictedPosition = targetPosition + targetVelocity * travelTime;

        return predictedPosition;
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
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
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
