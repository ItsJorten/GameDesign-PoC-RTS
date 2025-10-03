using UnityEngine;
using UnityEngine.UI;

/*
 * HealthBarUI
 * -----------
 * - Leest Health van de unit (parent).
 * - Zet Slider.value = Health.Normalized (0..1).
 * - Billboard: draait altijd richting MainCamera.
 * - Optioneel: verberg balk als health vol is.
 * - Optioneel: kleurverloop groen→geel→rood.
 */
[DisallowMultipleComponent]
public class HealthBarUI : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Health component van de unit (laat leeg om parent te pakken).")]
    public Health health;

    [Tooltip("De Slider die de HP toont (0..1).")]
    public Slider slider;

    [Header("Placement")]
    [Tooltip("Offset t.o.v. de unit-positie (y is hoogte).")]
    public Vector3 worldOffset = new Vector3(0f, 2.0f, 0f);

    [Header("Behaviour")]
    [Tooltip("Verberg healthbar als de unit full HP heeft.")]
    public bool hideWhenFull = true;

    [Tooltip("Kleurverloop gebaseerd op HP (0=rood, 0.5=geel, 1=groen).")]
    public bool useColorGradient = true;

    [Tooltip("Image die de 'Fill' kleurt (meestal de Fill child van de Slider).")]
    public Image fillImage;

    private Camera cam;
    private Transform target;

    void Awake()
    {
        cam = Camera.main;

        if (health == null)
            health = GetComponentInParent<Health>();

        target = (health != null) ? health.transform : transform.parent;

        if (slider == null)
            slider = GetComponentInChildren<Slider>(true);

        if (slider != null)
        {
            // Zorg dat de slider in [0..1] werkt
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }
    }

    void LateUpdate()
    {
        if (health == null || slider == null || target == null) return;

        // Update waarde
        float t = health.Normalized;
        slider.value = t;

        // Verberg indien vol (optioneel)
        if (hideWhenFull)
            slider.gameObject.SetActive(t < 0.999f);

        // Billboard & position
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        // Kleurverloop (groen -> geel -> rood)
        if (useColorGradient && fillImage != null)
        {
            Color c = Color.Lerp(Color.red, Color.green, t); // rood→groen
            // kleine tweak: tussen rood en groen via geel:
            if (t < 0.5f)
                c = Color.Lerp(Color.red, Color.yellow, t / 0.5f);
            else
                c = Color.Lerp(Color.yellow, Color.green, (t - 0.5f) / 0.5f);

            fillImage.color = c;
        }
    }
}
