using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;                             // <— TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("Refs (assign in Inspector)")]
    public GameObject buildingPanel;
    public Button produceButton;
    public TMP_Text buildingTitle;       // <— TMP i.p.v. Text

    void OnEnable()
    {
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSelectionChanged += UpdateUI;
    }

    void OnDisable()
    {
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSelectionChanged -= UpdateUI;
    }

    void Start()
    {
        if (buildingPanel != null) buildingPanel.SetActive(false);
    }

    private void UpdateUI(List<Selectable> selection)
    {
        if (buildingPanel == null || produceButton == null || buildingTitle == null)
            return;

        Selectable single = (selection != null && selection.Count == 1) ? selection[0] : null;
        bool showBuildingPanel = (single != null && single.Type == SelectableType.Building);

        if (!showBuildingPanel)
        {
            buildingPanel.SetActive(false);
            produceButton.onClick.RemoveAllListeners();
            return;
        }

        buildingPanel.SetActive(true);
        buildingTitle.text = single.name;

        var building = single.GetComponent<Building>();
        produceButton.onClick.RemoveAllListeners();

        if (building != null)
        {
            produceButton.interactable = !building.IsProducing;
            produceButton.onClick.AddListener(() => building.StartProduction());
        }
        else
        {
            buildingPanel.SetActive(false);
        }
    }
}
