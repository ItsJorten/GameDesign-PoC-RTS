using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitController : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Precision Move")]
    [Tooltip("Hoe dichtbij de agent het punt moet benaderen")]
    public float preciseStoppingDistance = 0.05f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Beweeg zo precies mogelijk naar positie (zonder formatie-offset).
    /// </summary>
    public void MoveToExact(Vector3 worldPosition)
    {
        if (agent == null) return;

        // Zorg dat de agent echt doorloopt tot nagenoeg het punt
        agent.stoppingDistance = Mathf.Max(0.0f, preciseStoppingDistance);
        agent.autoBraking = true; // helpt om niet te "overshooten"
        agent.SetDestination(worldPosition);
    }

    /// <summary>
    /// Oudere, generieke move (als je die nog ergens gebruikt).
    /// </summary>
    public void MoveTo(Vector3 worldPosition)
    {
        if (agent == null) return;
        agent.SetDestination(worldPosition);
    }
}
