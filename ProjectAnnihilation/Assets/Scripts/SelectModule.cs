using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectModule : MonoBehaviour
{
    [SerializeField]
    private KeyCode keepSelectionKey;
    [SerializeField]
    private KeyCode deselectionKey;

    private List<Unit> selectedUnits;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        selectedUnits = new List<Unit>();
    }


    private void Update()
    {
        CheckSelection();
        CheckDeselection();
    }

    private void CheckDeselection()
    {
        if (selectedUnits.Count > 0 && Input.GetKeyDown(deselectionKey))
            DeselectAllUnits();
    }

    private void CheckSelection()
    {
        if (!Input.GetMouseButtonDown(0) || (selectedUnits.Count > 0 && !Input.GetKey(keepSelectionKey)))
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (!Input.GetKey(keepSelectionKey))
                DeselectAllUnits();

            if (hit.collider.gameObject.TryGetComponent(out UserInput other))
                SelectUnit(other.Unit);
        }
    }

    #region Select functions

    private void SelectUnit(Unit unit)
    {
        if(unit == null) return;

        if (!selectedUnits.Contains(unit))
        {
            unit.Select();
            selectedUnits.Add(unit);
        }
    }
    private void DeselectUnit(Unit unit)
    {
        if(unit == null) return;

        if (selectedUnits.Contains(unit))
        {
            unit.Deselect();
            selectedUnits.Remove(unit);
        }
    }
    private void DeselectAllUnits()
    {
        foreach(Unit unit in selectedUnits) {
            unit.Deselect();
        }

        selectedUnits.Clear();
    }

    #endregion
}
