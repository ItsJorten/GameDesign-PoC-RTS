using UnityEngine;

/*
 * Health
 * ------
 * Houdt HP en teamId bij. Bij 0 HP -> Destroy (PoC-simpel).
 * teamId: 0 = speler, 1 = vijand (je kunt meer teams gebruiken).
 */
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximale levenspunten.")]
    public float maxHealth = 100f;

    [Tooltip("Start-/huidige levenspunten.")]
    public float currentHealth = 100f;

    [Header("Team")]
    [Tooltip("0 = speler, 1 = vijand, ...")]
    public int teamId = 0;

    public System.Action OnDeath; // handig voor FX/UI

    void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    /// <summary> Brengt schade toe en handelt sterven af. </summary>
    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            OnDeath?.Invoke();
            Die();
        }
    }

    void Die()
    {
        // PoC: direct verwijderen. Later kun je death anim/fx doen.
        Destroy(gameObject);
    }

    /// <summary> Health in [0..1], handig voor UI. </summary>
    public float Normalized => maxHealth > 0f ? currentHealth / maxHealth : 0f;
}
