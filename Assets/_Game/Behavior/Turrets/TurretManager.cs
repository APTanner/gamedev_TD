using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    private List<TurretController> m_reactionTurrets = new();
    private List<TurretController> m_otherTurrets = new();
    private List<ArtyController> m_artillery = new();

    protected void Awake()
    {
        Instance = this;
    }

    public void Register(TurretController turret)
    {
        if (IsReactionTurret(turret))
        {
            m_reactionTurrets.Add(turret);
        }
        else if (turret is ArtyController arty)
        {
            m_artillery.Add(arty);
        }
        else
        {
            m_otherTurrets.Add(turret);
        }
    }

    public void Deregister(TurretController turret)
    {
        if (IsReactionTurret(turret))
        {
            m_reactionTurrets.Remove(turret);
        }
        else if (turret is ArtyController arty)
        {
            m_artillery.Remove(arty);
        }
        else
        {
            m_otherTurrets.Remove(turret);
        }
    }

    private bool IsReactionTurret(TurretController turret)
    {
        GameObject go = turret.gameObject;
        return
            go.TryGetComponent(typeof(Gatling), out _) ||
            go.TryGetComponent(typeof(Railgun), out _);
    }

    protected void FixedUpdate()
    {
        HandleThreateningEnemies();

        foreach (var turret in m_otherTurrets)
        {
            turret.UpdateTurret();
        }
        foreach (var turret in m_reactionTurrets)
        {
            turret.UpdateTurret();
        }

        HandleArtillery();
        foreach (var turret in m_artillery)
        {
            turret.UpdateTurret();
        }
    }

    private void HandleThreateningEnemies()
    {
        Collider[] colliders = Physics.OverlapSphere(
            GameManager.Instance.GetHQPosition(),
            Defines.HQThreatDistance,
            1 << Defines.SwarmerLayer);

        List<SwarmerController> swarmers = colliders
            .Select(collider => collider.GetComponent<SwarmerController>())
            .Where(sc => sc != null)
            .ToList();

        foreach (var swarmer in swarmers)
        {
            Vector3 swarmerPos = swarmer.transform.position;

            float sqrDist = float.MaxValue;
            TurretController best = null;

            foreach (var turret in m_reactionTurrets)
            {
                Vector3 turretPos = turret.transform.position;

                if (!turret.HasTarget && (swarmerPos - turretPos).sqrMagnitude < sqrDist)
                {
                    Vector3 toTarget = swarmerPos - turretPos;
                    float dist = toTarget.magnitude;
                    toTarget /= dist;

                    if (!Physics.Raycast(turretPos, toTarget, dist, 1 << Defines.EnvironmentLayer))
                    {
                        sqrDist = dist * dist;
                        best = turret;
                    }
                }
            }

            if (best != null)
            {
                best.AssignTarget(swarmer);
            }
        }
    }

    private void HandleArtillery()
    {
        if (m_artillery.Count == 0)
        {
            return;
        }

        SwarmerManager sm = SwarmerManager.Instance;
        List<List<SwarmerController>> enemiesInMostPopulousCells = sm.GetMostPopulousCells();
        if (enemiesInMostPopulousCells.Count == 0)
        {
            return;
        }

        int idx = 0;
        int itrCnt = 0;
        foreach (var arty in m_artillery)
        {
            if (!arty.HasTarget)
            {
                var cell = enemiesInMostPopulousCells[idx];
                arty.AssignTarget(cell.Count > itrCnt ? cell[itrCnt] : cell[0]);
                ++idx;
                if (idx == enemiesInMostPopulousCells.Count)
                {
                    idx = 0;
                    ++itrCnt;
                }
            }
        }
    }
}
