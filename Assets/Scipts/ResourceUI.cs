using UnityEngine;
using TMPro;

public class ResourceUITMP : MonoBehaviour
{
    public TMP_Text metalText;
    public TMP_Text energyText;

    void OnEnable()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged += Refresh;
        Refresh();
        Debug.Log("[ResourceUITMP] OnEnable + Refresh called");
    }

    void OnDisable()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged -= Refresh;
    }

    void Start() => Refresh();

    void Refresh()
    {
        if (ResourceManager.Instance == null) { Debug.Log("[ResourceUITMP] No ResourceManager"); return; }
        if (metalText == null || energyText == null) { Debug.Log("[ResourceUITMP] TMP refs missing"); return; }

        metalText.text  = $"Metal: {ResourceManager.Instance.Metal}";
        energyText.text = $"Energy: {ResourceManager.Instance.Energy}";
        // Debug.Log($"[ResourceUITMP] UI -> M:{ResourceManager.Instance.Metal} E:{ResourceManager.Instance.Energy}");
    }
}
