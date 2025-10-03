using UnityEngine;

// We maken een enum om onderscheid te kunnen maken tussen types die je kunt selecteren.
// Handig voor later: zo weet je of iets een Unit of een Building is.
public enum SelectableType { Unit, Building }

[DisallowMultipleComponent] // voorkomt dat je per ongeluk 2x dit script toevoegt.
public class Selectable : MonoBehaviour
{
    // Stel in via de Inspector wat dit object is.
    public SelectableType Type = SelectableType.Unit;

    // Hier koppel je in de Inspector een child-object (bijv. een cirkel of highlight mesh).
    // Dit object wordt aan/uit gezet om te laten zien of het geselecteerd is.
    public GameObject selectionIndicator;

    // Houdt intern bij of dit object geselecteerd is.
    public bool IsSelected { get; private set; }

    // Wordt aangeroepen door de SelectionManager wanneer dit object geselecteerd wordt.
    public void Select()
    {
        // Als het al geselecteerd is, doe niks.
        if (IsSelected) return;

        // Zet de state naar true en toon het indicator-object.
        IsSelected = true;
        if (selectionIndicator != null) selectionIndicator.SetActive(true);
    }

    // Wordt aangeroepen als dit object gedeselecteerd moet worden.
    public void Deselect()
    {
        // Als het al niet geselecteerd was, doe niks.
        if (!IsSelected) return;

        // Zet de state terug naar false en verberg de indicator.
        IsSelected = false;
        if (selectionIndicator != null) selectionIndicator.SetActive(false);
    }
}
