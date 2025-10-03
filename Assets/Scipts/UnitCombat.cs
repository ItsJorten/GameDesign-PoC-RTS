using UnityEngine;
using UnityEngine.AI;

/*
 * UnitCombat (Projectile + ForcedTarget + Pursue)
 * -----------------------------------------------
 * - Zoekt automatisch vijand binnen attackRange (dichtste).
 * - Schiet projectielen met cooldown vanaf firePoint.
 * - ForcedTarget (via SelectionManager) overschrijft auto-target:
 *   de unit loopt dan naar target toe tot in bereik en vuurt.
 */
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]
public class UnitCombat : MonoBehaviour
{
    [Header("Weapon")]
    public float attackRange = 8f;        // schietbereik
    public float fireCooldown = 0.75f;    // tijd tussen schoten
    public float damagePerShot = 10f;     // schade per projectiel
    public GameObject projectilePrefab;   // wijs je Projectile prefab toe
    public Transform firePoint;           // monding/uitgang van projectiel

    [Header("Targeting")]
    public LayerMask targetLayerMask = ~0; // Everything; we filteren op teamId
    public bool debugDrawRange = false;

    [Header("Pursue settings")]
    public bool pursueForcedTarget = true;       // loop naar target als out-of-range
    public float pursueStopBuffer = 0.6f;        // iets minder dan attackRange om oscillatie te voorkomen

    private Health myHealth;
    private NavMeshAgent agent;
    private UnitController mover;     // gebruikt MoveToExact
    private Health autoTarget;        // target via auto-acquire
    private Health forcedTarget;      // expliciet aangewezen via command
    private float fireTimer = 0f;

    void Awake()
    {
        myHealth = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();
        mover = GetComponent<UnitController>();
        if (firePoint == null) firePoint = this.transform; // fallback
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        // Kies target volgorde: Forced > Auto
        Health target = GetBestTarget();

        // Pursue: als forcedTarget bestaat en buiten bereik is, beweeg dichterbij
        if (pursueForcedTarget && forcedTarget != null && !IsInRange(forcedTarget))
            MoveTowardsRange(forcedTarget);

        // Als we een geldig target hebben en cooldown klaar is -> schieten
        if (target != null && fireTimer <= 0f)
        {
            if (IsInRange(target))
            {
                FireProjectile(target);
                fireTimer = fireCooldown;
            }
        }

        // (Optioneel) draai langzaam naar target voor visuele feedback
        if (target != null)
        {
            Vector3 to = target.transform.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(to);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
            }
        }
    }

    Health GetBestTarget()
    {
        // Forced target heeft voorrang als die geldig is
        if (IsTargetValid(forcedTarget)) return forcedTarget;

        // Anders gebruik autoTarget; her-acquire als niet (meer) geldig
        if (!IsTargetValid(autoTarget))
            autoTarget = AcquireClosestEnemy();

        return autoTarget;
    }

    bool IsTargetValid(Health h)
    {
        if (h == null) return false;
        if (h.currentHealth <= 0f) return false;
        if (h.teamId == myHealth.teamId) return false;
        return true;
    }

    bool IsInRange(Health h)
    {
        if (h == null) return false;
        float distSqr = (h.transform.position - transform.position).sqrMagnitude;
        return distSqr <= attackRange * attackRange;
    }

    Health AcquireClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, targetLayerMask);
        float best = float.MaxValue;
        Health bestH = null;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i].GetComponentInParent<Health>();
            if (!IsTargetValid(h)) continue;

            float d = (h.transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestH = h;
            }
        }
        return bestH;
    }

    void MoveTowardsRange(Health h)
    {
        if (mover == null) return;
        // Mik een punt binnen bereik voor de schutter (iets binnen attackRange)
        Vector3 to = (h.transform.position - transform.position);
        to.y = 0f;
        float dist = to.magnitude;
        if (dist < Mathf.Epsilon) return;

        float desiredDist = Mathf.Max(attackRange - pursueStopBuffer, 0.5f);
        Vector3 dest = h.transform.position - to.normalized * desiredDist;

        mover.MoveToExact(dest);
    }

    void FireProjectile(Health target)
    {
        if (projectilePrefab == null || target == null) return;

        Vector3 spawnPos = firePoint.position;
        Quaternion rot = Quaternion.LookRotation(target.transform.position - spawnPos);

        GameObject go = Object.Instantiate(projectilePrefab, spawnPos, rot);
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
            proj.Init(target, damagePerShot, myHealth.teamId);
    }

    // Wordt aangeroepen door SelectionManager bij een attack-command
    public void SetForcedTarget(Health h)
    {
        if (IsTargetValid(h))
            forcedTarget = h;
        else
            forcedTarget = null;
    }

    void OnDrawGizmosSelected()
    {
        if (!debugDrawRange) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
