using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UserInput : MonoBehaviour
{
    private enum InputType
    {
        PlayerInput,
        Auto
    }


    public Unit Unit => unit;

    [Header("Options")]
    [SerializeField]
    private InputType inputType;
    [SerializeField]
    private LayerMask terrainLayer;


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

        visualTargetManager.UnlockTarget();
        visualTargetManager.ShowTarget(false);

        inputType = unit.IsAttacker ? InputType.PlayerInput : InputType.Auto;
    }

    private void Update()
    {
        visualTargetManager.ShowTarget(unit.IsSelected && unit.CurrentOrder != UnitState.NOTHING && unit.CurrentOrder != UnitState.IDLE);

        if(inputType == InputType.PlayerInput)
            ManagePlayerInput();
        else
            ManageAutoInput();
    }

    private void ManagePlayerInput()
    {
        wasSelected &= unit.IsSelected;

        // If selection key is held, it's to select other units, not give orders
        if (!unit.IsSelected || Input.GetKey(SelectModule.Instance.KeepSelectionKey)) 
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

            SelectModule.Instance.DeselectUnit(unit);
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
        /*else if (Input.GetAxis("Follow") == 1)
        {
            GetMousePositionOnTerrain(out Unit unit);

            if (unit != null)
            {
                unit.SetFollowedTarget(unit.transform);
                unit.SetCurrentOrderState(UnitState.FOLLOWING);
            }
            // FOLLOW IS DEPRECATED FOR THE MOMENT
        }*/

        if (Input.GetKeyDown(KeyCode.A))
        {
            unit.ActivateSpecialAbility();
        }
    }

    #region AutoInput

    public bool CanBeSelected => inputType == InputType.PlayerInput;

    private void ManageAutoInput()
    {
        // All units will act automatically
        //SelectModule.Instance.GetAllUnits().ForEach(unit =>
        //{

        //});
    }

    #endregion

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

        if (!showTargetVisual)
            HideTarget();
        else
            PlaceTarget(location);
    }

    public void OrderUnitToAttack(Unit unitToAttack, bool showTargetVisual = true)
    {
        unit.SetFollowedTarget(unitToAttack.transform);
        unit.SetCurrentOrderState(UnitState.MOVENATTACK);

        if (!showTargetVisual)
            HideTarget();
        else
            LockTarget(unitToAttack);
    }

    #region Visuals
    private void PlaceTarget(Vector3 location)
    {
        visualTargetManager.UnlockTarget();
        visualTargetManager.PlaceTargetAt(location);
        visualTargetManager.SetColor(visualTargetManager.simpleMoveColor);
    }
    private void LockTarget(Unit unitToAttack) {
        visualTargetManager.LockTarget(unitToAttack);
        visualTargetManager.SetColor(visualTargetManager.attackUnitColor);
    }
    private void HideTarget()
    {
        visualTargetManager.SetColor(Color.clear);
    }
    #endregion
}
