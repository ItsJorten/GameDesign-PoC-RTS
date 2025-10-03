using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Produceert precies één type unit, na bouwtijd.
/// Nu met: kosten (Metal/Energy) en betaalcheck via ResourceManager.
/// </summary>
[DisallowMultipleComponent]
public class Building : MonoBehaviour
{
    [Header("Production")]
    public GameObject unitPrefab;
    public Transform spawnPoint;
    public float buildTime = 1.5f;

    [Header("Cost per unit")]
    public int costMetal = 50;
    public int costEnergy = 20;

    private bool isProducing = false;
    public bool IsProducing => isProducing;

    public void StartProduction()
    {

        // 1) Guard clauses
        if (isProducing || unitPrefab == null || spawnPoint == null) return;

        // 2) Betaal check: als te weinig → niks doen (UI kan dit ook tonen)
        if (ResourceManager.Instance != null)
        {
            if (!ResourceManager.Instance.TrySpend(costMetal, costEnergy))
            {
                Debug.Log("Not enough resources to produce unit.", this);
                return;
            }
        }

        // 3) Start productie (kosten zijn nu al afgeschreven)
        StartCoroutine(ProduceCoroutine());
    }

    private IEnumerator ProduceCoroutine()
    {
        isProducing = true;

        // Simuleer bouwtijd
        yield return new WaitForSeconds(buildTime);

        // Spawn unit
        GameObject unitGO = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        unitGO.name = unitPrefab.name;

        // Plaats netjes op NavMesh (Warp)
        var agent = unitGO.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (NavMesh.SamplePosition(unitGO.transform.position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                agent.Warp(navHit.position);
        }

        // (Optioneel) teamId erven van het gebouw:
        var buildingHealth = GetComponent<Health>();
        var unitHealth = unitGO.GetComponent<Health>();
        if (buildingHealth != null && unitHealth != null)
            unitHealth.teamId = buildingHealth.teamId;

        isProducing = false;
    }
}
