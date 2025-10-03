using UnityEngine;

/*
 * Projectile
 * ----------
 * Vliegt naar een doel (Health) en doet damage bij impact of nabijheid.
 * PoC: straight-line, geen ballistiek.
 */
[DisallowMultipleComponent]
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Snelheid (m/s).")]
    public float speed = 20f;

    [Tooltip("Fail-safe levensduur (s).")]
    public float maxLifetime = 5f;

    [Tooltip("Impact radius (hoe dichtbij we damage toebrengen).")]
    public float hitRadius = 0.25f;

    private Health target;
    private float damage;
    private int shooterTeamId;

    public void Init(Health newTarget, float dmg, int teamId)
    {
        target = newTarget;
        damage = dmg;
        shooterTeamId = teamId;
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        if (target == null || target.currentHealth <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Richting → bewegen
        Vector3 dir = (target.transform.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.forward = dir; // visuele oriëntatie

        // Impact check
        float dist = Vector3.Distance(transform.position, target.transform.position);
        if (dist <= hitRadius)
        {
            if (target.teamId != shooterTeamId)
                target.TakeDamage(damage);

            Destroy(gameObject);
        }
    }

    // (Optioneel) OnTriggerEnter gebruiken i.p.v. afstand-check:
    void OnTriggerEnter(Collider other)
    {
        // Alleen als je collider/trigger route wilt gebruiken i.p.v. afstand
        // var h = other.GetComponentInParent<Health>();
        // if (h != null && h.teamId != shooterTeamId) { h.TakeDamage(damage); Destroy(gameObject); }
    }
}
