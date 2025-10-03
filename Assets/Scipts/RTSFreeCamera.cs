using UnityEngine;

// Eenvoudige RTS-camera: pan (WASD), rotate (Q/E of RMB drag), zoom (scroll).
// We veranderen de YAW (y-as rotatie) en verplaatsen de camera in de lokale X/Z.
// De tilt (x-as) laat je initieel vast staan (bv. 45 graden), zodat de hoek constant blijft.
[DisallowMultipleComponent]
public class RTSFreeCamera : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;           // Snelheid voor WASD-pan
    public float fastMultiplier = 2f;       // Houd Shift ingedrukt voor sneller pannen

    [Header("Rotation")]
    public float rotateSpeedKeys = 90f;     // Graden/s bij Q/E
    public float rotateSpeedMouse = 0.2f;   // Gevoeligheid bij RMB slepen

    [Header("Zoom")]
    public float zoomSpeed = 100f;          // Scroll-snelheid
    public float minHeight = 10f;           // Niet te laag
    public float maxHeight = 120f;          // Niet te hoog

    private Camera cam;
    private float yaw;   // rotatie rond y-as
    private bool rotatingWithMouse;

    void Awake()
    {
        cam = GetComponent<Camera>();
        yaw = transform.eulerAngles.y; // start-yaw bewaren
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    void HandleMovement()
    {
        // Input axes (WASD/pijltjes). Old Input Manager: "Horizontal"/"Vertical"
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Shift voor sneller pannen
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? fastMultiplier : 1f);

        // Richting t.o.v. huidige yaw (dus "vooruit" = waar de camera naartoe kijkt in de XZ-vlak)
        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 right   = new Vector3(transform.right.x,   0f, transform.right.z).normalized;

        Vector3 move = (forward * v + right * h) * speed * Time.deltaTime;
        transform.position += move;
    }

    void HandleRotation()
    {
        // Toetsen Q/E draaien yaw
        if (Input.GetKey(KeyCode.Q)) yaw -= rotateSpeedKeys * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) yaw += rotateSpeedKeys * Time.deltaTime;

        // RMB ingedrukt? Dan slepen voor rotatie
        if (Input.GetMouseButtonDown(1)) rotatingWithMouse = true;
        if (Input.GetMouseButtonUp(1))   rotatingWithMouse = false;

        if (rotatingWithMouse)
        {
            yaw += Input.GetAxis("Mouse X") * (rotateSpeedMouse * 100f) * Time.deltaTime;
            // (Je kunt ook tilt met Mouse Y aanpassen, maar jij wilde hoek constant houden.)
        }

        // Pas yaw toe, behoud tilt (x) en roll (z) van huidige transform
        Vector3 eul = transform.eulerAngles;
        eul.y = yaw;
        transform.eulerAngles = eul;
    }

    void HandleZoom()
    {
        // Verplaats langs de wereld-verticale as (y) om te “zoomen”.
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newY = Mathf.Clamp(transform.position.y - scroll * zoomSpeed * Time.deltaTime, minHeight, maxHeight);
            float deltaY = newY - transform.position.y;
            transform.position += new Vector3(0f, deltaY, 0f);
        }
    }
}
