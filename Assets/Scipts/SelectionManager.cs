using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;

/*
 * SelectionManager
 * ----------------
 * Centrale afhandeling voor selectie en orders in je RTS PoC.
 * - LMB click: single select (Shift = add/remove)
 * - LMB drag: selection box (multi-select). Buildings worden niet box-geselecteerd.
 * - RMB click: move order voor alle geselecteerde Units (met precieze bestemming).
 *
 * Belangrijke instellingen in de Inspector:
 *  - selectableLayerMask : alleen objects op deze layer(s) worden selecteerbaar (Units/Buildings)
 *  - groundLayerMask     : waar je klikdoelen voor movement mag kiezen (bv. Plane = Ground)
 *  - selectionBoxUI      : UI die het sleepkader tekent
 *
 * Tip: Zorg dat je Camera de tag "MainCamera" heeft; we gebruiken Camera.main voor raycasts.
 */

[DisallowMultipleComponent]
public class SelectionManager : MonoBehaviour
{
    // -------- Singleton (gemak: makkelijke toegang vanuit UI e.d.) --------
    public static SelectionManager Instance { get; private set; }

    // -------- Inspector: Layers & UI refs --------
    [Header("Layers")]
    [Tooltip("La(a)g(en) waar Units/Buildings op staan die klikbaar/selekteerbaar zijn.")]
    public LayerMask selectableLayerMask;

    [Tooltip("La(a)g(en) die de grond representeren voor move-clicks.")]
    public LayerMask groundLayerMask;

    [Header("Drag Select UI")]
    [Tooltip("Verwijzing naar de UI die de selectie-box tekent.")]
    public SelectionBoxUI selectionBoxUI;

    // -------- Instelbare tuning waarden --------
    [Header("Tuning")]
    [Tooltip("Minimale pixels slepen voordat we een box-select starten.")]
    public float dragThresholdPixels = 5f;

    [Tooltip("Radius waarbinnen NavMesh.SamplePosition een geldig punt mag zoeken (kleiner = preciezer).")]
    public float navMeshClampRadius = 0.5f; // was 2f; strakker = dichter bij klik

    [Tooltip("Kleine formatie-offset bij multi-select (wordt alleen gebruikt als er >1 unit is).")]
    public float multiSelectOffsetRadius = 1.0f;

    // -------- Huidige selectie (multi) + event --------
    public readonly List<Selectable> Selected = new List<Selectable>();

    // UI/andere systemen kunnen hierop abonneren (bijv. UIManager toont Building-panel bij single-building selection)
    public event Action<List<Selectable>> OnSelectionChanged;

    // -------- Intern state --------
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector2 dragStartScreen; // startpunt voor drag (in schermpixels)

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogWarning("[SelectionManager] MainCamera niet gevonden (tag ontbreekt?). Raycasts gebruiken Camera.main.");
    }

    private void Update()
    {
        // Als muis boven UI zweeft, negeer wereldinteractie
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        HandleLeftMouse();
        HandleRightMouse();
    }

    // ======================
    //  LEFT MOUSE (Selectie)
    // ======================
    private void HandleLeftMouse()
    {
        // Muisknop omlaag: mogelijke start van klik of drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartScreen = Input.mousePosition;
            isDragging = false; // bepalen we later op basis van afstand
        }

        // Vasthouden: check of we genoeg ver slepen om box te starten
        if (Input.GetMouseButton(0))
        {
            Vector2 current = Input.mousePosition;
            if (!isDragging && Vector2.Distance(current, dragStartScreen) > dragThresholdPixels)
            {
                isDragging = true;
                if (selectionBoxUI != null) selectionBoxUI.Begin(dragStartScreen);
            }

            if (isDragging && selectionBoxUI != null)
                selectionBoxUI.Drag(current);
        }

        // Loslaten: afronden (klik of box)
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                // Box select afronden
                Vector2 end = Input.mousePosition;
                if (selectionBoxUI != null)
                {
                    Rect screenRect = selectionBoxUI.GetScreenRect(end); // rect in schermpixels
                    selectionBoxUI.End();
                    PerformBoxSelection(screenRect, addToSelection: IsShiftHeld());
                }
                else
                {
                    // Geen UI-component? Valt terug op "geen selectie".
                    if (!IsShiftHeld()) ClearSelection();
                    NotifySelectionChanged();
                }
            }
            else
            {
                // Enkel-klik selectie
                HandleClickSelection();
            }
        }
    }

    // Eén klik selectie (met Shift voor add/remove)
    private void HandleClickSelection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, selectableLayerMask))
        {
            var sel = hit.collider.GetComponentInParent<Selectable>();
            bool additive = IsShiftHeld();

            if (!additive) ClearSelection();

            if (sel != null)
            {
                // Bij additive: toggle, anders force select
                if (additive) ToggleSelect(sel);
                else ForceSelect(sel);
            }

            NotifySelectionChanged();
            return;
        }

        // Klikte op niks → deselecteer alles (tenzij Shift = add)
        if (!IsShiftHeld())
        {
            ClearSelection();
            NotifySelectionChanged();
        }
    }

    // Box-select: selecteert alle Units waarvan hun schermpositie in de screenRect valt.
    private void PerformBoxSelection(Rect screenRect, bool addToSelection)
    {
        if (!addToSelection) ClearSelection();

        // Pak alle Selectables in de scene (PoC: eenvoudig en voldoende)
#if UNITY_2023_1_OR_NEWER
        Selectable[] all = GameObject.FindObjectsByType<Selectable>(FindObjectsSortMode.None);
#else
        Selectable[] all = GameObject.FindObjectsOfType<Selectable>();
#endif

        for (int i = 0; i < all.Length; i++)
        {
            Selectable s = all[i];
            if (s == null) continue;

            // Meestal wil je buildings niet multi-box-selecteren in een RTS
            if (s.Type != SelectableType.Unit) continue;

            // Wereld → schermruimte; achter camera (z<0) negeren
            Vector3 screenPos = mainCamera.WorldToScreenPoint(s.transform.position);
            if (screenPos.z < 0f) continue;

            if (screenRect.Contains((Vector2)screenPos))
            {
                ForceSelect(s);
            }
        }

        NotifySelectionChanged();
    }

    // ================
    //  RIGHT MOUSE (Move)
    // ================
    private void HandleRightMouse()
{
    if (!Input.GetMouseButtonDown(1)) return;
    if (Selected.Count == 0) return;

    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

    // 1) Eerst kijken: klikten we op een Selectable?
    if (Physics.Raycast(ray, out RaycastHit selHit, 1000f, selectableLayerMask))
    {
        var sel = selHit.collider.GetComponentInParent<Selectable>();
        if (sel != null && sel.Type == SelectableType.Unit)
        {
            var targetHealth = sel.GetComponent<Health>();
            if (targetHealth != null)
            {
                // Bepaal het team van de speler (eerste geselecteerde unit)
                int playerTeam = 0;
                for (int i = 0; i < Selected.Count; i++)
                {
                    var h = Selected[i].GetComponent<Health>();
                    if (h != null) { playerTeam = h.teamId; break; }
                }

                // Alleen aanvallen als doel niet van ons team is
                if (targetHealth.teamId != playerTeam)
                {
                    // Geef alle geselecteerde Units het target
                    for (int i = 0; i < Selected.Count; i++)
                    {
                        if (Selected[i].Type != SelectableType.Unit) continue;
                        var combat = Selected[i].GetComponent<UnitCombat>();
                        if (combat != null) combat.SetForcedTarget(targetHealth);
                    }
                    return; // klaar, geen move
                }
            }
        }
    }

    // 2) Geen (vijandige) selectable geraakt? Dan is het een move-click op de grond.
    if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundLayerMask))
    {
        if (NavMesh.SamplePosition(groundHit.point, out NavMeshHit navHit, navMeshClampRadius, NavMesh.AllAreas))
        {
            Vector3 target = navHit.position;

            // Tel Units
            int unitCount = 0;
            for (int i = 0; i < Selected.Count; i++)
                if (Selected[i].Type == SelectableType.Unit) unitCount++;

            for (int i = 0; i < Selected.Count; i++)
            {
                if (Selected[i].Type != SelectableType.Unit) continue;

                var unit = Selected[i].GetComponent<UnitController>();
                if (unit == null) continue;

                Vector3 dest = target;
                if (unitCount > 1 && multiSelectOffsetRadius > 0f)
                {
                    Vector2 circle = UnityEngine.Random.insideUnitCircle * multiSelectOffsetRadius;
                    dest += new Vector3(circle.x, 0f, circle.y);
                }

                unit.MoveToExact(dest);

                // Belangrijk: als we net een forced target hadden, nul 'm bij move
                var combat = Selected[i].GetComponent<UnitCombat>();
                if (combat != null) combat.SetForcedTarget(null);
            }
        }
    }
}

    // ==================
    //  Select helpers
    // ==================
    private void ClearSelection()
    {
        for (int i = 0; i < Selected.Count; i++)
            if (Selected[i] != null) Selected[i].Deselect();
        Selected.Clear();
    }

    // Forceer dat iets geselecteerd is (voegt toe als die niet in de lijst staat)
    private void ForceSelect(Selectable s)
    {
        if (s == null) return;
        if (!Selected.Contains(s))
        {
            Selected.Add(s);
            s.Select();
        }
        else
        {
            s.Select(); // zorg dat visuals kloppen
        }
    }

    // Toggle gedrag voor Shift-klik op een enkel object
    private void ToggleSelect(Selectable s)
    {
        if (s == null) return;
        if (Selected.Contains(s))
        {
            s.Deselect();
            Selected.Remove(s);
        }
        else
        {
            s.Select();
            Selected.Add(s);
        }
    }

    private void NotifySelectionChanged()
    {
        OnSelectionChanged?.Invoke(Selected);
    }

    private static bool IsShiftHeld()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
}
