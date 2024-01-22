using UnityEngine;

public class UserInput : MonoBehaviour
{
    public Unit Unit => unit;

    [SerializeField]
    private LayerMask terrainLayer;
    [SerializeField]
    private GameObject stateVisual;


    private Unit unit;
    private VisualTargetUnit visualTargetManager;


    private bool wasSelected;

    private void Start()
    {
        TryGetComponent(out unit);
        TryGetComponent(out visualTargetManager);

        if (unit == null)
            Debug.LogWarning("Warning: UserInput requires a unit to work.");
        if (visualTargetManager == null)
            Debug.LogWarning("Visual target manager not detected", gameObject);

        wasSelected = false;
        unit.SetStatusObject(stateVisual);

        visualTargetManager.UnlockTarget();
        visualTargetManager.ShowTarget(false);
    }

    private void Update()
    {
        visualTargetManager.ShowTarget(unit.IsSelected && unit.CurrentOrder != UnitState.NOTHING && unit.CurrentOrder != UnitState.IDLE);
        ManageInput();
    }

    private void ManageInput()
    {
        wasSelected &= unit.IsSelected;

        if (!unit.IsSelected || Input.GetKey(SelectModule.Instance.keepSelectionKey))
            return;

        if (!wasSelected) // Prevents selection from killing the current action
        {
            wasSelected = true;
            return;
        }

        // Si le joueur veut reset l'unité
        if (Input.GetAxis("Idle") == 1)
        {
            // Si le joueur veut que l'unité ne fasse rien du tout jusqu'à nouvel ordre
            if (Input.GetAxis("Focus") == 1)
                unit.SetCurrentOrderState(UnitState.IDLE);
            else
                unit.SetCurrentOrderState(UnitState.NOTHING);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Si le joueur clique quelque part...
            Vector3? location = GetMousePositionOnTerrain(out Unit otherUnit);

            if (location == null)
                return;


            // ...est-ce sur une unité à pourchaser ?
            if (otherUnit != null && unit.CanTarget(otherUnit))
            {
                OrderUnitToAttack(otherUnit);
            }
            // ...est-ce sur une position ?
            else
                OrderUnitMoveTo((Vector3)location);
        }
        else if (Input.GetAxis("Follow") == 1)
        {
            GetMousePositionOnTerrain(out Unit unit);

            if (unit != null)
            {
                unit.SetFollowedTarget(unit.transform);
                unit.SetCurrentOrderState(UnitState.FOLLOWING);
            }
            // FOLLOW IS DEPRECATED FOR THE MOMENT
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

    public void OrderUnitMoveTo(Vector3 location, bool showTargetVisual = true)
    {
        unit.SetDestination(location);

        // ... doit-on se concentrer sur la position ?
        if (Input.GetAxis("Focus") == 1)
            unit.SetCurrentOrderState(UnitState.MOVING);
        else // ... ou attaquer au passage ?
            unit.SetCurrentOrderState(UnitState.MOVING_ALERT);

        if (!showTargetVisual){
            visualTargetManager.SetColor(Color.clear);
            return;
        }
        
        visualTargetManager.UnlockTarget();
        visualTargetManager.PlaceTargetAt(location);
        visualTargetManager.SetColor(visualTargetManager.simpleMoveColor);
    }

    public void OrderUnitToAttack(Unit unitToAttack, bool showTargetVisual = true)
    {
        unit.SetFollowedTarget(unitToAttack.transform);
        unit.SetCurrentOrderState(UnitState.MOVENATTACK);

        if (!showTargetVisual)
        {
            visualTargetManager.SetColor(Color.clear);
            return;
        }
        visualTargetManager.LockTarget(unitToAttack);
        visualTargetManager.SetColor(visualTargetManager.attackUnitColor);
    }
}
