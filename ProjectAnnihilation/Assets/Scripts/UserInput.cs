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


    private void Start()
    {
        unit = GetComponent<Unit>();

        if(unit == null)
        {
            Debug.LogWarning("Warning: UserInput requires a unit to work.");
        }

        unit.SetStatusObject(selectedVisual);
    }

    private void Update()
    {
        ManageInput();

        //selectedVisual.SetActive(unit.IsSelected);
    }

    protected virtual void ManageInput()
    {
        if (!unit.IsSelected)
            return;

        // Si le joueur veut reset l'unité
        if (Input.GetAxis("Idle") == 1)
        {
            // Si le joueur veut que l'unité ne fasse rien du tout jusqu'à nouvel ordre
            if (Input.GetAxis("Focus") == 1)
                unit.SetCurrentOrderState(UnitState.NOTHING);
            else
                unit.SetCurrentOrderState(UnitState.IDLE);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Si le joueur clique quelque part...
            Vector3? location = GetMousePositionOnTerrain(out Unit otherUnit);

            // ...est-ce sur une unité à pourchaser ?
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
                    unit.SetCurrentOrderState(UnitState.MOVING_FOCUS);
                else // ... ou attaquer au passage ?
                    unit.SetCurrentOrderState(UnitState.MOVING);
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
