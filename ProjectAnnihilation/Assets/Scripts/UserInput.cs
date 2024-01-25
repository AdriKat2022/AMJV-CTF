using UnityEngine;

public class UserInput : MonoBehaviour
{
    private const float Y_OFFSET = 1f;

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

    [Header("Debug")]
    [SerializeField]
    private bool debug;


    private Unit unit;
    private VisualTargetUnit visualTargetManager;
    private GameManager gameManager;

    private bool wasSelected;

    private void Start()
    {
        gameManager = GameManager.Instance;

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
        if (!gameManager.GameStarted)
            return;

        visualTargetManager.ShowTarget(unit.IsSelected && unit.CurrentOrder != UnitState.NOTHING && unit.CurrentOrder != UnitState.IDLE);

        if(inputType == InputType.PlayerInput)
            ManagePlayerInput();
        else
            ManageAutoInput();
    }

    public bool CanBeSelected => inputType == InputType.PlayerInput;
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

    private void ManageAutoInput()
    {

        if (Random.value > 0.8)
            return;

        Unit king = null;
        Unit closestUnitInteractable = null;
        float minDistance = 9999f;
        
        switch (unit.UnitData.AttackTargets)
        {
            case TargetType.AlliesOnly: // Most likely a healer

                float smallestRatio = 1;

                SelectModule.Instance.GetAllUnits().ForEach(otherUnit =>
                {
                    if (otherUnit == unit) return;

                    if (otherUnit.IsKing) king = otherUnit;

                    if (otherUnit.IsAttacker != unit.IsAttacker) return;

                    float ratio = otherUnit.CurrentHealth/otherUnit.UnitData.MaxHP;

                    if (ratio >= smallestRatio) return;
                    
                    smallestRatio = ratio;
                    closestUnitInteractable = otherUnit;

                });

                if(closestUnitInteractable != null)
                    OrderUnitToAttack(closestUnitInteractable, false);

                break;

            default: // Every other case
                
                SelectModule.Instance.GetAllUnits().ForEach(otherUnit =>
                {
                    if(otherUnit == unit) return;
                    
                    if (otherUnit.IsKing) king = otherUnit;

                    if (otherUnit.IsAttacker == unit.IsAttacker) return;

                    if(!CanSeePoint(otherUnit.transform)) return;

                    if ((otherUnit.transform.position - transform.position).sqrMagnitude > minDistance) return;
                    
                    minDistance = (otherUnit.transform.position - transform.position).sqrMagnitude;
                    closestUnitInteractable = otherUnit;
                });

                if (king != null && CanSeePoint(king.transform))
                    OrderUnitToAttack(king, false);

                else if(closestUnitInteractable != null)
                    OrderUnitToAttack(closestUnitInteractable, false);

                else if (king != null)
                    OrderUnitToAttack(king, false);

                if (Random.value > .95f)
                    OrderUnitToSpecialAttack();

                //if (debug)
                //{
                //    Debug.Log(gameObject);
                //    Debug.Log(closestUnitInteractable, gameObject);
                //}

                break;
        }

    }

    private bool CanSeePoint(Transform point)
    {
        if (point == null) return false;

        if (debug)
        {
            Debug.DrawLine(transform.position + Vector3.up * Y_OFFSET, transform.position + Vector3.up * Y_OFFSET, Color.magenta, 1f);
        }

        RaycastHit hit;

        bool blocked = Physics.Raycast(
            transform.position + Vector3.up * Y_OFFSET,
            point.position - transform.position + Vector3.up * Y_OFFSET,
            out hit,
            (point.position - transform.position).magnitude,
            LayerMask.GetMask("Ground"));

        //Debug.Log(hit);

        return !blocked;
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

    public void OrderUnitToSpecialAttack() => unit.ActivateSpecialAbility();
    

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
