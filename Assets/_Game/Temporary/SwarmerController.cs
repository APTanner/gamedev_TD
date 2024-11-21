using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwarmerController : MonoBehaviour
{
    [SerializeField] private float maxHealth = 5f;

    [SerializeField]
    private GameObject vfxPrefab;

    private float currentHealth;

    public bool ShowDesiredHeading = false;
    public float separationWeight;
    public float obstacleWeight;

    private LayerMask m_environmentLayer;
    private LayerMask m_swarmerLayer;
    private LayerMask m_targetsLayer;
    private Rigidbody m_rb;

    private SwarmerManager m_manager;
    private float m_radius;
    private IBuilding m_target;
    private int m_attention;

    private bool m_bAttacking = false;
    private bool m_bFacingTarget = false;

    public bool IsAttacking => m_bAttacking;
    public IBuilding Target => m_target;

    // 'EGrid' is enemy grid
    // this is to differentiate between the building grid and the enemy grid
    public Vector2Int EGridCoord { get; set; }

    protected void Awake()
    {
        m_environmentLayer = 1 << Defines.EnvironmentLayer;
        m_swarmerLayer     = 1 << Defines.SwarmerLayer;
        m_targetsLayer     = 1 << Defines.PlayerLayer;

        m_rb = GetComponent<Rigidbody>();
        m_radius = GetComponent<SphereCollider>().radius;

        SwarmerManager.Instance.Register(this);
        m_manager = SwarmerManager.Instance;

        currentHealth = maxHealth;
    }

    protected void Update()
    {
        if (!m_manager.DebugMovement)
        {
            return;
        }

        Debug.DrawRay(forwardRay.origin, forwardRay.direction * m_manager.ObstacleAvoidDistance, forwardHit ? Color.red : Color.white);
        Debug.DrawRay(leftRay.origin, leftRay.direction * m_manager.ObstacleAvoidDistance, leftHit ? Color.red : Color.white);
        Debug.DrawRay(rightRay.origin, rightRay.direction * m_manager.ObstacleAvoidDistance, rightHit ? Color.red : Color.white);

        if (!forwardHit && !leftHit && !rightHit)
        {
            return;
        }

        for (int i = 0; i < 2; ++i)
        {
            Debug.DrawRay(leftRays[i].origin, leftRays[i].direction * m_manager.WhiskerDistance, leftHits[i] ? Color.red : Color.white);
            Debug.DrawRay(rightRays[i].origin, rightRays[i].direction * m_manager.WhiskerDistance, rightHits[i] ? Color.red : Color.white);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Perform any destruction logic, like playing an animation or sound
        //Debug.Log($"{gameObject.name} has been destroyed.");
        Destroy(gameObject); // Destroy the swarmer
    }

    // DEBUG 
    private Ray forwardRay;
    private bool forwardHit;
    private Ray leftRay;
    private bool leftHit;
    private Ray rightRay;
    private bool rightHit;

    private Ray[] leftRays = new Ray[2];
    private Ray[] rightRays = new Ray[2];

    bool[] leftHits = new bool[2];
    bool[] rightHits = new bool[2];


    public void UpdateSwarmer()
    {
        // Make sure to clear the reference if our target gets destroyed
        if (m_target != null && (m_target as Component) == null)
        {
            m_target = null;
        }

        Vector3 forward = transform.forward;

        Quaternion avoidRot = Quaternion.AngleAxis(m_manager.AvoidanceAngle, Vector3.up);

        forwardRay = new Ray(transform.position, forward);
        rightRay   = new Ray(transform.position + transform.right * m_radius, avoidRot * forward);
        leftRay    = new Ray(transform.position - transform.right * m_radius, Quaternion.Inverse(avoidRot) * forward);

        AvoidanceDistances d = new();

        forwardHit = Physics.Raycast(forwardRay,
                                     out RaycastHit forwardHitInfo,
                                     m_manager.ObstacleAvoidDistance,
                                     m_environmentLayer | m_targetsLayer);

        rightHit   = Physics.Raycast(rightRay,
                                     out RaycastHit rightHitInfo,
                                     m_manager.ObstacleAvoidDistance,
                                     m_environmentLayer | m_targetsLayer);

        leftHit    = Physics.Raycast(leftRay,
                                     out RaycastHit leftHitInfo,
                                     m_manager.ObstacleAvoidDistance,
                                     m_environmentLayer | m_targetsLayer);

        HandleTargetSelection(ref d, in forwardHitInfo, in rightHitInfo, in leftHitInfo);

        for (int i = 0; i < 2; ++i)
        {
            float dAngle = i * 45;
            const float initialAngle = 45;
            Quaternion rotation = Quaternion.AngleAxis(initialAngle + dAngle, Vector3.up);

            rightRays[i] = new Ray(transform.position, rotation * transform.forward);
            leftRays[i] = new Ray(transform.position, Quaternion.Inverse(rotation) * transform.forward);


            leftHits[i]  = Physics.Raycast(leftRays[i],
                                           out RaycastHit lInfo,
                                           m_manager.WhiskerDistance);
            rightHits[i] = Physics.Raycast(rightRays[i],
                                           out RaycastHit rInfo,
                                           m_manager.WhiskerDistance);


            d.LeftDistances += lInfo.distance;
            d.RightDistances += rInfo.distance;
        }

        if (m_target != null)
        {
            if (m_attention == 0)
            {
                m_target = null;
            }
            else
            {
                --m_attention;
            }
        }
        Vector2 targetPos = m_target == null ? SwarmerTarget.Instance.transform.position.xz() : m_target.GetPosition().xz();
        Vector2 toTarget = (targetPos - transform.position.xz()).normalized;
        float cross = MathFunctions.Cross(forward.xz(), toTarget);
        int favoredRotation = -(int)Mathf.Sign(cross);
        int avoidanceRotation = 0;

        // If we need to avoid something
        if (forwardHit || rightHit || leftHit)
        {
            avoidanceRotation = GetAvoidance(favoredRotation, ref d);
        }

        // GETTING NEIGHBOR HEADINGS USING SPHERE CAST
        //Collider[] colliders = Physics.OverlapSphere(transform.position, m_manager.SwarmerViewRange, m_swarmerLayer);
        //Vector3 neighborHeading = Vector3.zero;
        //foreach (Collider collider in colliders)
        //{
        //    neighborHeading += collider.transform.forward;
        //}

        // Perform a sphere check to find all colliders within the radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_manager.NeighborDetectionDistance, m_swarmerLayer);

        Vector3 seperationAccumulation = Vector3.zero;
        foreach (var collider in colliders)
        {
            // Don't try and avoid it if it's attacking
            if (collider.transform.position == transform.position || collider.GetComponent<SwarmerController>().IsAttacking)
            {
                continue;
            }
            Vector3 avoidHeading = -(collider.transform.position - transform.position);
            //Debug.Log($"{avoidHeading}");

            float dist = avoidHeading.magnitude;
            float sqrtDenominator = Mathf.Max(Mathf.Abs(dist - 2*m_radius), .03f) * 4;
            float scale = 1 / (sqrtDenominator * sqrtDenominator);
            if (ShowDesiredHeading)
            {
                Debug.Log($"Scale: {scale} -- sqrMag: {(Vector3.SqrMagnitude(avoidHeading))} radius constant: {(m_radius * 1.99f * m_radius * 1.99f)}");
            }
            avoidHeading *= scale;
            if (ShowDesiredHeading)
            {
                Debug.DrawLine(transform.position, transform.position + avoidHeading, Color.white);
            }
            //Debug.Log($"{avoidHeading}");
            seperationAccumulation += avoidHeading;
        }
        Vector2 separation = seperationAccumulation.xz();
        //Debug.Log(separation);

        Vector2 alignment = m_manager.GetCellHeading(transform.position);

        //Vector2 desiredHeading = avoidanceRotation == 0 ? (toTarget + alignment).normalized : transform.right * avoidanceRotation;

        Vector2 desiredHeading = (toTarget * m_manager.TargetWeight +
                                  alignment * m_manager.AlignmentWeight +
                                  (transform.right * avoidanceRotation).xz() * d.AvoidanceStrength * m_manager.ObstacleWeight +
                                  separation * m_manager.SeparationWeight
                                  ).normalized;

        separationWeight = (separation * m_manager.SeparationWeight).magnitude;
        obstacleWeight = ((transform.right * avoidanceRotation).xz() * d.AvoidanceStrength * m_manager.ObstacleWeight).magnitude;


        //transform.Rotate(new Vector3(0, m_manager.TurnSpeed * (avoidanceRotation == 0 ? favoredRotation * Mathf.Min(Mathf.Abs(cross),1) : avoidanceRotation) * Time.fixedDeltaTime, 0));
        if (ShowDesiredHeading)
        {
            Debug.DrawLine(transform.position, transform.position + MathFunctions.UnflattenVector2(desiredHeading) * 5, Color.magenta);
            Debug.DrawLine(transform.position, transform.position + MathFunctions.UnflattenVector2(separation), Color.green);
        }
        var targetRotation = Quaternion.LookRotation(MathFunctions.UnflattenVector2(desiredHeading), Vector3.up);
        //transform.rotation = targetRotation;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_manager.TurnSpeed * Time.fixedDeltaTime);
        Move(d);

    }

    private void HandleTargetSelection(ref AvoidanceDistances d, in RaycastHit center, in RaycastHit right, in RaycastHit left)
    {
        m_bFacingTarget = false;
        if (center.collider?.gameObject.layer == Defines.PlayerLayer)
        {
            SetTarget(center);
            m_bFacingTarget = true;
            m_attention = Defines.SwarmerAttentionSpan;
        }
        // Because the right is checked first it means it will always favor a right target
        // maybe we want to randomize this?
        else if (m_target == null && right.collider?.gameObject.layer == Defines.PlayerLayer)
        {
            SetTarget(right);
            m_attention = Defines.SwarmerAttentionSpan;
        }
        else if (m_target == null && left.collider?.gameObject.layer == Defines.PlayerLayer)
        {
            SetTarget(left);
            m_attention = Defines.SwarmerAttentionSpan;
        }

        if (m_target is Wall)
        {
            // perform sanity check
            Vector2 t = (SwarmerTarget.Instance.transform.position.xz() - transform.position.xz()).normalized;
            Vector3 toTarget = new(t.x, 0, t.y);
            Vector3 pos = transform.position + toTarget * m_manager.ObstacleAvoidDistance/2;
            Quaternion atTarget = Quaternion.LookRotation(toTarget, Vector3.up);
            bool bBlocker = Physics.CheckBox(
                pos,
                new(m_radius, m_radius, m_manager.ObstacleAvoidDistance/2),
                atTarget,
                m_environmentLayer | m_targetsLayer);

            Vector3 f = atTarget * Vector3.forward;

            if (!bBlocker)
            {
                m_target = null;
            }
        }

        if (m_target == null)
        {
            d.CenterDistance = center.distance;
            d.RightDistance = right.distance;
            d.LeftDistance = left.distance;
        }
        else
        {
            d.CenterDistance = center.collider != null &&
                center.collider.gameObject.layer == Defines.EnvironmentLayer ?
                center.distance : 0;
            d.RightDistance = right.collider != null &&
                right.collider.gameObject.layer == Defines.EnvironmentLayer ?
                right.distance : 0;
            d.LeftDistance = left.collider != null &&
                left.collider.gameObject.layer == Defines.EnvironmentLayer ?
                left.distance : 0;
        }

        if (m_target != null)
        {
            Debug.DrawLine(transform.position, m_target.GetPosition(), Color.black);
        }
    }

    private void SetTarget(in RaycastHit hit)
    {
        Component component = hit.transform.parent.GetComponent(typeof(IBuilding));
        if (component is IBuilding building)
        {
            m_target = building;
        }
        else
        {
            Debug.LogError($"A building did not have a component implementing 'IBuilding' on it. There is no way to track its data");
        }
    }

    private int GetAvoidance(int favoredRotation, ref AvoidanceDistances dist)
    {
        dist.AvoidanceStrength =
            GetDistanceValue(dist.RightDistance) +
            GetDistanceValue(dist.CenterDistance) +
            GetDistanceValue(dist.LeftDistance);


        // If we just need to adjust a little bit
        if (!rightHit || !leftHit)
        {
            return rightHit ? -1 : 1;
        }

        // see if we've found some empty space
        for (int i = 0; i < 2; ++i)
        {
            if (!rightHits[i] || !leftHits[i])
            {
                if (!rightHits[i] && !leftHits[i])
                {
                    return favoredRotation;
                }
                else
                {
                    return rightHits[i] ? -1 : 1;
                }
            }
        }

        float rightBlockers = dist.RightDistance + dist.RightDistances;
        float leftBlockers = dist.LeftDistance + dist.LeftDistances;
        return rightBlockers < leftBlockers ? -1 : 1;
    }

    private float GetDistanceValue(float value)
    {
        float percentDistance = Mathf.Max(value - m_radius, 0) / (m_manager.ObstacleAvoidDistance - m_radius);
        return value == 0 ? value : Mathf.Lerp(1000, 1, percentDistance);
    }

    private void Move(in AvoidanceDistances d)
    {
        m_bAttacking = false; // reset
        Vector2 currentV = m_rb.linearVelocity.xz();

        Vector2 desiredV = transform.forward.xz() * m_manager.MaxSpeed;

        // Check if we have a valid target
        if (m_target != null)
        {
            Vector2 toTarget = m_target.GetPosition().xz() - transform.position.xz();
            float dist = toTarget.magnitude;
            // check if we are going in for an attack
            if (m_target is HQ)
            {
                // If the target is HQ, move directly toward it at max speed
                desiredV = (toTarget / dist) * m_manager.MaxSpeed;
            }
            else if (dist <= m_manager.AttackDistance * 1.5f)
            {
                m_bAttacking = m_bFacingTarget;
                // TRIAL 1
                // 100 * (x^2 - d)^3 -> get desired velocity around the distance one wants to attack at
                // graph in desmos to see
                //float value = dist - m_manager.AttackDistance;
                //value = Mathf.Clamp(.5f * value * value * value, -1f, 1f);
                float value = Mathf.Clamp(0.5f * (dist - m_manager.AttackDistance), -1, 1);
                //Debug.Log($"Current dist to target: {dist}. The current magnitude of acceleration: {value}");
                desiredV = value * m_manager.MaxSpeed * (toTarget / dist);
            }
        }

        Vector2 dv = Vector2.ClampMagnitude(desiredV-currentV, m_manager.Acceleration);
        m_rb.AddForce(MathFunctions.UnflattenVector2(dv), ForceMode.Impulse);
        //Debug.Log($"Current Velocity: {m_rb.velocity.magnitude}");

        if (DeservesSlowdown(d.CenterDistance) ||
            DeservesSlowdown(d.RightDistance) ||
            DeservesSlowdown(d.LeftDistance))
        {
            m_rb.linearVelocity *= .1f;
        };
    }

    private bool DeservesSlowdown(float distance)
    {
        return distance != 0 && distance < m_manager.ObstacleAvoidSlowdownRange;
    }

    private struct AvoidanceDistances
    {
        public float CenterDistance;
        public float RightDistance;
        public float RightDistances;
        public float LeftDistance;
        public float LeftDistances;

        public float AvoidanceStrength;
    }

    protected void OnDestroy()
    {
        m_manager.Deregister(this); 

        if (vfxPrefab != null)
        {
            GameObject vfxInstance = Instantiate(vfxPrefab, transform.position, Quaternion.identity);

            ParticleSystem particleSystem = vfxInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                Destroy(vfxInstance, particleSystem.main.duration);
            }
            else
            {
                Destroy(vfxInstance, 8.0f);
            }
        }
    }
}
