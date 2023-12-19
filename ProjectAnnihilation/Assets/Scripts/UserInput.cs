using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    public Unit Unit => unit;

    [SerializeField]
    private Unit unit;
    [SerializeField]
    private LayerMask terrainLayer;
    [SerializeField]
    private GameObject selectedVisual;


    private bool wasSelected;

    private void Start()
    {
        unit = GetComponent<Unit>();

        if(unit == null)
        {
            Debug.LogWarning("Warning: UserInput requires a unit to work.");
        }

        wasSelected = false;
        unit.SetStatusObject(selectedVisual);
    }

    private void Update()
    {
        ManageInput();

        //selectedVisual.SetActive(unit.IsSelected);
    }

    private void ManageInput()
    {
        wasSelected &= unit.IsSelected;

        if (!unit.IsSelected)
            return;

        if (!wasSelected) // Prevents selection from killing the current action
        {
            wasSelected = true;
            return;
        }

        // Si le joueur veut reset l'unit�
        if (Input.GetAxis("Idle") == 1)
        {
            // Si le joueur veut que l'unit� ne fasse rien du tout jusqu'� nouvel ordre
            if (Input.GetAxis("Focus") == 1)
                unit.SetCurrentOrderState(UnitState.IDLE);
            else
                unit.SetCurrentOrderState(UnitState.NOTHING);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Si le joueur clique quelque part...
            Vector3? location = GetMousePositionOnTerrain(out Unit otherUnit);

            // ...est-ce sur une unit� � pourchaser ?
            if (otherUnit != null)
            {
                unit.SetFollowedTarget(otherUnit.transform);
                unit.SetCurrentOrderState(UnitState.MOVENATTACK);
            }
            // ...est-ce sur une position ?
            else
            {
                unit.SetDestination(location);
                
                // ... doit-on se concentrer sur la position ?
                if (Input.GetAxis("Focus") == 1)
                    unit.SetCurrentOrderState(UnitState.MOVING);
                else // ... ou attaquer au passage ?
                    unit.SetCurrentOrderState(UnitState.MOVING_FOCUS);
            }
            unit.ResetTimeBeforeTargetting();
        }
        else if (Input.GetAxis("Follow") == 1)
        {
            GetMousePositionOnTerrain(out Unit unit);

            if (unit != null)
            {
                unit.SetFollowedTarget(unit.transform);
                unit.SetCurrentOrderState(UnitState.FOLLOWING);
            }

            unit.ResetTimeBeforeTargetting();
        }
    }
    public Vector3? GetMousePositionOnTerrain(out Unit other) // Return mouse position on terrain, returns null if nothing was hit.
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // If it's an unit - set is as other and pass the collision point
            if (hit.collider.gameObject.TryGetComponent(out other))
                return hit.point;

            other = default;

            if ((terrainLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
                return hit.point;

        }
        other = default;
        return null;
    }

}
