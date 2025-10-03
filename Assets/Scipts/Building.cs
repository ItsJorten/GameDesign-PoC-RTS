using System.Collections;
using UnityEngine;
using UnityEngine.AI; // nodig om de unit meteen correct op de NavMesh te zetten

[DisallowMultipleComponent]
public class Building : MonoBehaviour
{
    // Prefab van de unit die dit gebouw kan produceren
    [Tooltip("Prefab van unit die deze building produceert")]
    public GameObject unitPrefab;

    // Transform waar de unit moet spawnen (bijv. een empty object 'SpawnPoint')
    public Transform spawnPoint;

    // Tijd in seconden die het duurt om een unit te maken
    public float buildTime = 1.5f;

    // Houdt bij of het gebouw nu iets aan het produceren is
    private bool isProducing = false;
    public bool IsProducing => isProducing;

    // Wordt aangeroepen vanuit de UI wanneer je op 'Produce' klikt
    public void StartProduction()
    {
        // Voorkom dubbel starten als al bezig of setup niet klopt
        if (isProducing || unitPrefab == null || spawnPoint == null) return;

        // Start coroutine die de productie afhandelt
        StartCoroutine(ProduceCoroutine());
    }

    // Coroutine die een wachttijd simuleert en daarna de unit spawnt
    private IEnumerator ProduceCoroutine()
    {
        isProducing = true;

        // Wacht de buildtime af (simuleert productietijd)
        yield return new WaitForSeconds(buildTime);

        // Maak een nieuwe unit aan op de spawnPoint positie
        GameObject unitGO = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        unitGO.name = unitPrefab.name; // optioneel, voor nette namen in de hierarchy

        // Check of de unit een NavMeshAgent heeft
        var agent = unitGO.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // Plaats de unit netjes op de NavMesh (belangrijk voor movement)
            if (NavMesh.SamplePosition(unitGO.transform.position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(navHit.position);
            }
            else
            {
                Debug.LogWarning("Spawn position is not on NavMesh for unit: " + unitGO.name, this);
            }
        }

        // Productie afgerond, gebouw is weer beschikbaar
        isProducing = false;
    }
}
