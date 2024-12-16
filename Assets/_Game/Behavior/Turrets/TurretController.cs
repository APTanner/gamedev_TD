using System;
using UnityEngine;
using UnityEngine.VFX;

public class TurretController : MonoBehaviour
{
    public float detectionRange = 20f; // Range for grid-based enemy detection
    public float closeDetectionRange = 5f; // Range for sphere cast
    public float rotationSpeed = 5f;
    public float fireRate = 1f;

    public float minVerticalAimDistance = 5f;

    protected float fireCooldown = 0f;

    public Transform turretBase;
    public Transform gunPivot;
    public Transform firePoint;

    public GameObject bulletPrefab;
    protected float bulletSpeed;
    public VisualEffect muzzleFlashVFX;

    public bool IsPreview { get; set; } = false;

    protected Target m_target = new Target();

    public bool HasTarget => IsTargetValid();

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

        TurretManager.Instance.Register(this);
    }

    public virtual void UpdateTurret()
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
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            OnStopFiring();
        }
    }

    public void AssignTarget(SwarmerController controller)
    {
        m_target.UpdateTarget(controller);
    }

    protected bool FindCloseTarget()
    {
        Collider[] closeEnemies = Physics.OverlapSphere(transform.position, closeDetectionRange, 1 << Defines.SwarmerLayer);
        if (closeEnemies.Length > 0)
        {
            Transform closest = null;
            float closestDist = float.PositiveInfinity;

            foreach (Collider c in closeEnemies)
            {
                Vector3 toTarget = c.transform.position - transform.position;
                float dist = toTarget.magnitude;
                toTarget /= dist;
                if (dist < closestDist && !Physics.Raycast(transform.position, toTarget, dist, 1 << Defines.EnvironmentLayer))
                {
                    closest = c.transform;
                    closestDist = dist;
                }
            }

            // when it's in our danger area we do not care about marked targets
            if (closest != null &&
                closest.TryGetComponent<SwarmerController>(out var enemyController))
            {
                m_target.UpdateTarget(enemyController);
                return true;
            }
        }
        return false;
    }

    private static readonly int[] signs = { -1, 1 };
    protected virtual void FindTarget()
    {
        Vector2Int turretCoord = SwarmerManager.Instance.GetCoord(transform.position);
        float gridCellSize = SwarmerManager.Instance.CellSize;
        int maxCellRange = Mathf.CeilToInt(detectionRange / gridCellSize);

        // exits early for searching in a certain direction
        //                     NW     NE    SW    SE    WS   WN     ES    EW
        bool[] searchSpace = { true, true, true, true, true, true, true, true };

        // dummy reference passed in
        TryGetGridTarget(turretCoord.x, turretCoord.y, ref searchSpace[0]);

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
    }

    private bool WithinRangeBounds(Vector2Int xBounds, Vector2Int yBounds, Vector2Int coords)
    {
        return coords.x >= xBounds.x && coords.x <= xBounds.y &&
               coords.y >= yBounds.x && coords.y <= yBounds.y;
    }

    private int GetMaxDistFromTurretBound(Vector2Int xBounds, Vector2Int yBounds, Vector2Int searchOrigin)
    {
        int maxX = Mathf.Max(Mathf.Abs(searchOrigin.x - xBounds.x), Mathf.Abs(searchOrigin.x - xBounds.y));
        int maxY = Mathf.Max(Mathf.Abs(searchOrigin.y - yBounds.x), Mathf.Abs(searchOrigin.y - yBounds.y));
        return Mathf.Max(maxX, maxY);
    }

    private bool TryGetGridTarget(int x, int y, ref bool continueSearch)
    {
        float cellSize = SwarmerManager.Instance.CellSize;

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

        if (CheckCellForValidTarget(x, y))
        {
            return true;
        }

        return false;
    }

    protected bool CheckCellForValidTarget(Vector2Int coord)
    {
        return CheckCellForValidTarget(coord.x, coord.y);
    }

    private bool CheckCellForValidTarget(int x, int y)
    {
        float sqrRange = MathFunctions.Square(detectionRange);
        Vector2Int coord = new(x, y);

        foreach (var target in SwarmerManager.Instance.GetEnemiesInCell(coord))
        {
            if (!target.IsMarked && IsPositionTargetable(target.transform.position))
            {
                m_target.UpdateTarget(target);
                return true;
            }
        }
        return false;
    }

    protected virtual bool RotateToFaceTarget()
    {
        Vector3 targetPosition = m_target.lastPosition;
        Vector3 targetVelocity = m_target.rigidbody.linearVelocity;

        Vector3 predictedPosition = CalculatePredictedPosition(targetPosition, targetVelocity);

        // Horizontal rotation (Y-axis)
        Vector3 horizontalDirection = new Vector3(predictedPosition.x - transform.position.x, 0f, predictedPosition.z - transform.position.z).normalized;
        Quaternion horizontalLookRotation = Quaternion.LookRotation(horizontalDirection);
        turretBase.rotation = Quaternion.RotateTowards(turretBase.rotation, horizontalLookRotation, Time.fixedDeltaTime * rotationSpeed);

        float angleFromTarget = Quaternion.Angle(turretBase.rotation, horizontalLookRotation);

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

        // Hardcoded because 
        return angleFromTarget <= Defines.TurretAimTolerance;
    }

    // chatgpt calculus
    protected virtual Vector3 CalculatePredictedPosition(Vector3 targetPosition, Vector3 targetVelocity)
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

    private bool IsPositionTargetable(Vector3 position)
    {
        Vector3 toTarget = position - transform.position;
        float range = toTarget.magnitude;
        toTarget /= range;

        if (range <= detectionRange && !Physics.Raycast(transform.position, toTarget, range, 1 << Defines.EnvironmentLayer))
        {
            return true;
        }

        return false;
    }

    protected virtual bool IsTargetValid()
    {
        if (m_target.IsValid && m_target.IsAlive())
        {
            m_target.UpdateTarget();

            if (IsPositionTargetable(m_target.lastPosition))
            {
                return true;
            }
        }

        m_target.Invalidate();
        return false;
    }

    protected void OnDestroy()
    {
        if (m_target.IsValid)
        {
            m_target.Invalidate();
        }

        TurretManager.Instance.Deregister(this);
    }

    protected struct Target
    {
        public SwarmerController target;
        public Rigidbody rigidbody { get; private set; }
        public Vector3 lastPosition { get; private set; }
        public bool IsValid { get; private set; }

        public void UpdateTarget()
        {
            lastPosition = target.transform.position;
        }

        public void UpdateTarget(SwarmerController newTarget)
        {
            target = newTarget;
            newTarget.MarkTarget();

            IsValid = true;
            rigidbody = target.GetComponent<Rigidbody>();

            UpdateTarget();
        }

        public readonly bool IsAlive()
        {
            return target != null && (target as Component) != null;
        }

        public void Invalidate()
        {
            IsValid = false;
            if (target != null)
            {
                target.UnmarkTarget();
                target = null;
            }
        }
    }
}
