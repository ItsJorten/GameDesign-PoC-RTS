using UnityEngine;

/// <summary>
/// ResourceManager houdt Metal & Energy bij, 
/// regelt CanAfford/TrySpend, en verwerkt optionele passieve income.
/// Singleton pattern zodat andere scripts (UI, Building) dit makkelijk kunnen aanspreken.
/// </summary>
[DisallowMultipleComponent]
public class ResourceManager : MonoBehaviour
{
    // --- Singleton ---
    public static ResourceManager Instance { get; private set; }

    [Header("Start Amounts")]
    [Tooltip("Beginhoeveelheid bij start van het spel.")]
    public int startMetal = 200;
    public int startEnergy = 200;

    [Header("Passive Income (per second)")]
    [Tooltip("Automatisch inkomen per seconde. Gebruik >0 voor drip.")]
    public float passiveMetalPerSecond = 0f;
    public float passiveEnergyPerSecond = 0f;

    // Huidige waarden
    public int Metal { get; private set; }
    public int Energy { get; private set; }

    // Event dat UI kan subscriben om zichzelf te verversen
    public System.Action OnResourcesChanged;

    // --- Accumulators om fracties van passive income bij te houden ---
    private float accMetal = 0f;
    private float accEnergy = 0f;

    void Awake()
    {
        // Singleton setup (slechts één ResourceManager mag bestaan)
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
            return; 
        }
        Instance = this;

        // Zet startwaarden
        Metal = startMetal;
        Energy = startEnergy;

        Debug.Log($"[ResourceManager] Initialized with Metal:{Metal}, Energy:{Energy}");
    }

    void Update()
    {
        // Passive income voor Metal
        if (passiveMetalPerSecond > 0f)
        {
            accMetal += passiveMetalPerSecond * Time.deltaTime;
            if (accMetal >= 1f)
            {
                int add = Mathf.FloorToInt(accMetal);
                accMetal -= add;
                AddResource(ResourceType.Metal, add);
                Debug.Log($"[ResourceManager] Passive Metal +{add} -> {Metal}");
            }
        }

        // Passive income voor Energy
        if (passiveEnergyPerSecond > 0f)
        {
            accEnergy += passiveEnergyPerSecond * Time.deltaTime;
            if (accEnergy >= 1f)
            {
                int add = Mathf.FloorToInt(accEnergy);
                accEnergy -= add;
                AddResource(ResourceType.Energy, add);
                Debug.Log($"[ResourceManager] Passive Energy +{add} -> {Energy}");
            }
        }
    }

    /// <summary>
    /// Verhoogt of verlaagt een resource. Roept het OnResourcesChanged event aan.
    /// Gebruik een negatief amount om kosten/uitgaven te doen.
    /// </summary>
    public void AddResource(ResourceType type, int amount)
    {
        if (amount == 0) return;

        if (type == ResourceType.Metal) 
            Metal = Mathf.Max(0, Metal + amount);
        else 
            Energy = Mathf.Max(0, Energy + amount);

        Debug.Log($"[ResourceManager] AddResource {type} {amount:+#;-#;0} -> Metal:{Metal}, Energy:{Energy}");

        OnResourcesChanged?.Invoke();
    }

    /// <summary> True als we genoeg resources hebben om te betalen. </summary>
    public bool CanAfford(int metalCost, int energyCost)
    {
        return Metal >= metalCost && Energy >= energyCost;
    }

    /// <summary>
    /// Probeert kosten af te boeken. 
    /// True = betaling gelukt en geboekt, False = niet genoeg.
    /// </summary>
    public bool TrySpend(int metalCost, int energyCost)
    {
        if (!CanAfford(metalCost, energyCost)) 
        {
            Debug.Log($"[ResourceManager] Cannot afford M:{metalCost} E:{energyCost} -> Current M:{Metal}, E:{Energy}");
            return false;
        }

        Metal -= metalCost;
        Energy -= energyCost;

        Debug.Log($"[ResourceManager] Spent M:{metalCost}, E:{energyCost} -> Now M:{Metal}, E:{Energy}");

        OnResourcesChanged?.Invoke();
        return true;
    }
}
