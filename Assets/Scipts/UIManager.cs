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

    private Building currentBuilding; // onthoud referentie voor refresh

    void OnEnable()
    {
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSelectionChanged += UpdateUI;
            
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged += RefreshBuildingPanel;
    }

    void OnDisable()
    {
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSelectionChanged -= UpdateUI;
                    
        if (ResourceManager.Instance != null)
        ResourceManager.Instance.OnResourcesChanged -= RefreshBuildingPanel;
    }

    void Start()
    {
        if (buildingPanel != null) buildingPanel.SetActive(false);
    }

    private void UpdateUI(List<Selectable> selection)
    {
        currentBuilding = null;
            if (selection != null && selection.Count == 1 && selection[0].Type == SelectableType.Building)
            {
            buildingPanel.SetActive(true);
            buildingTitle.text = selection[0].name;
            currentBuilding = selection[0].GetComponent<Building>();

            produceButton.onClick.RemoveAllListeners();
            if (currentBuilding != null)
            {
                produceButton.onClick.AddListener(() => currentBuilding.StartProduction());
            }

            RefreshBuildingPanel(); // enable/disable knop o.b.v. resources + state
        }
        else
        {
            buildingPanel.SetActive(false);
            produceButton.onClick.RemoveAllListeners();
        }
    }

    private void RefreshBuildingPanel()
    {
        if (!buildingPanel.activeSelf || currentBuilding == null) return;

        bool canAfford = true;
        if (ResourceManager.Instance != null)
            canAfford = ResourceManager.Instance.CanAfford(currentBuilding.costMetal, currentBuilding.costEnergy);

        // knop alleen actief als we kunnen betalen en niet aan het produceren zijn
        produceButton.interactable = canAfford && !currentBuilding.IsProducing;
    }
}