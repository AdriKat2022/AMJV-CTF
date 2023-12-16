using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;



public enum PossibleTargets
{
    All,
    EnemiesOnly,
    AlliesOnly,
    None
}


public class Unit : MonoBehaviour, ISelectable
{

    [SerializeField]
    private UnitData unitData;

    private NavMeshAgent navigation;

    private GameManager gameManager;

    public bool IsAttacker => isAttacker;
    private bool isAttacker; // Defines the team of the unit


    private float currentHp;
    private float speedBonus;
    private float attackBonus;

    private bool isFocused;

    private GameObject targetableUnit;

    public bool IsInvisible => isInvisible;
    private bool isInvisible = false; // If other units can see them
    private bool isInvincible = false;
    private bool isInvulnerable = false;

    public bool IsSelected => isSelected;
    [SerializeField]
    private bool isSelected;



    [Header("Debug")]
    [SerializeField]
    private UnitState unitState;
    [SerializeField]
    private UnitState currentOrder;
    [SerializeField]
    private LayerMask terrainLayer;


    [Header("Memory usage")]
    [SerializeField]
    private int enemyDetectionBuffer = 30;



    private bool inEndLag;
    private float endLagTimer;

    private Transform followedTarget; // Following

    private Vector3 pointA; // Patrolling
    private Vector3 pointB;

    #region UNIT ACTIONS (TO OVERRIDE BY UNIT)
    protected virtual void Action(GameObject target = null) {
        endLagTimer = unitData.attackEndLag;
    }
    protected virtual void SpecialAction(GameObject target = null) {
        endLagTimer = unitData.specialAttackEndLag;
    }

    #endregion

    private enum UnitState
    {
        NOTHING, // Forces nothing (pointless to be honest)
        IDLE,
        MOVING,
        MOVENATTACK,
        FOLLOWING,
        PATROLLING
    }

    private void Start()
    {
        //Time.timeScale = 10f;
        gameManager = GameManager.Instance;
        navigation = GetComponent<NavMeshAgent>();

        isSelected = false;
        isFocused = false;

        navigation.speed = unitData.speed;

        Initialize(unitData);
    }

    protected virtual void Initialize(UnitData unitData) // Can be overriden if a unit needs a specific initialization
    {
        currentHp = unitData.maxHp;
        inEndLag = false;

        unitState = UnitState.IDLE;

        speedBonus = 0;
        attackBonus = 0;
    }


    private void Update()
    {
        Debug.Log(unitState);

        ManageInput();

        if (inEndLag)
        {
            OnActionEndLag();
            return;
        }

        UpdateStateMachine();
    }

    /// <summary>
    /// Runs once per frame while in end lag.<br />
    /// UpdateStateMachine is NOT called during end lag. This function is where you can act during those frames.<br />
    /// 
    /// By default DECREASES the endLagTimer (in seconds) normally.
    /// </summary>
    protected virtual void OnActionEndLag()
    {
        if (!inEndLag)
            return;

        endLagTimer -= Time.deltaTime;
        if (endLagTimer <= 0f)
        {
            inEndLag = false;
        }
    }

    protected virtual void ManageInput()
    {
        // TODO : Selection is done from the game manager script (with Select())

        if (!isSelected)
            return;

        // TODO : create two scripts 1) for player input ; 2) for player AI

        /*Debug.Log(Input.GetAxis("Idle"));
        Debug.Log(Input.GetAxis("Focus"));
        Debug.Log(Input.GetMouseButtonDown(0));*/

        if (Input.GetAxis("Idle") == 1)
        {
            Debug.Log("Set to idle or nothing");
            if (Input.GetAxis("Focus") == 1)
                currentOrder = UnitState.NOTHING;
            else
                currentOrder = UnitState.IDLE;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Vector3? location = GetMousePositionOnTerrain(out Unit unit);

            if(unit != null)
            {
                followedTarget = unit.transform;
                currentOrder = UnitState.MOVENATTACK;
            }
            else
            {
                Debug.Log("Set a destination");
                SetDestination(location);
                Debug.Log(location);
                currentOrder = UnitState.MOVING;
            }
        }
        else if (Input.GetAxis("Follow") == 1)
        {
            GetMousePositionOnTerrain(out Unit unit);

            if (unit != null)
            {
                followedTarget = unit.transform;
                currentOrder = UnitState.FOLLOWING;
            }
        }
    }

    #region State Machine

    private void UpdateStateMachine()
    {

        switch (unitState)
        {
            case UnitState.NOTHING:

                NothingState();

                break;

            case UnitState.IDLE:

                IdlingState();

                break;

            case UnitState.MOVING:

                MovingState();

                break;

            case UnitState.MOVENATTACK:

                MoveNAttackState();

                break;

            case UnitState.FOLLOWING:

                FollowingState();

                break;

            case UnitState.PATROLLING:

                PatrollingState();

                break;
        }
    }

    #region States

    /// <summary>
    /// Does nothing.<br />
    /// Absolutely nothing.
    /// </summary>
    protected virtual void NothingState()
    {
        PauseNavigation();
        unitState = currentOrder;
    }
    /// <summary>
    /// Does nothing, but launches attacks any unit in range
    /// </summary>
    protected virtual void IdlingState()
    {
        PauseNavigation();

        if (CanAttack() && currentOrder == UnitState.IDLE)
        {
            Action();
        }
        else
        {
            unitState = currentOrder;
        }
    }
    /// <summary>
    /// Moves towards a position (not a unit)<br />
    /// If focus mode is enabled (isFocused), the unit doesn't care about attacking other units, until it has reached the end of his path.
    /// </summary>
    protected virtual void MovingState()
    {
        ResumeNavigation();

        if(currentOrder != UnitState.MOVING)
        {
            unitState = currentOrder;
        }
        else if(CanAttack() && !isFocused)
        {
            Action();
        }

        if (HasArrived())
            currentOrder = UnitState.IDLE;
            
        /*if (navigation.pathStatus == NavMeshPathStatus.PathComplete)
        {
            navigation.isStopped = true;
            currentOrder = UnitState.IDLE;
        }*/
    }
    /// <summary>
    /// Targets any other unit (followedUnit).<br />
    /// Move towards them, and attack them once possible.<br />
    /// Keeps chasing the target if gets out of its attack range.
    /// </summary>
    protected virtual void MoveNAttackState()
    {
        if (followedTarget != null && currentOrder == UnitState.MOVENATTACK)
        {
            SetDestination(followedTarget.position);

            if (CanAttack(followedTarget.gameObject))
            {
                Action();
            }
        }
        else if(currentOrder == UnitState.MOVENATTACK)
        {
            SetDestination(null);
            currentOrder = UnitState.IDLE;
        }
        else
        {
            unitState = currentOrder;  // Exit condition
        }
    }
    /// <summary>
    /// Follows an unit.<br />
    /// Like MoveNAttackState but without targeting the followed unit
    /// </summary>
    protected virtual void FollowingState()
    {
        if(followedTarget != null)
            SetDestination(followedTarget.position);
        else
        {
            SetDestination(null);
            currentOrder = UnitState.IDLE;
        }

        ResumeNavigation();

        if (CanAttack() && currentOrder == UnitState.FOLLOWING)
        {
            Action();
        }
        else
            unitState = currentOrder;  // Exit condition
    }
    /// <summary>
    ///  Goes on and on between two positions.<br />
    ///  Attacks anyone in sight.
    /// </summary>
    protected virtual void PatrollingState()
    {
        if (CanAttack() && currentOrder != UnitState.PATROLLING)
        {
            Action();
            PauseNavigation();
        }
        else if(!inEndLag)
        {
            ResumeNavigation();
            unitState = currentOrder;
        }
    }

    #endregion

    #region Helper functions

    private bool HasArrived()
    {
        return (transform.position - navigation.destination).magnitude < navigation.stoppingDistance*1.01f;
    }
    /// <summary>
    /// Check if the unit can attack a unit.<br/>
    /// Will update the variable 'targetableUnit' with the closest gameObject each time this is called with the parameter target = null.
    /// </summary>
    /// <param name="target">Null to test for any target. Specify one to test if this one can be attacked.</param>
    private bool CanAttack(GameObject target = null)
    {
        // TODO: Implement basic requirement for a unit to attack

        if (target == null)
        {
            GameObject closestGameObject = null;
            float distance = unitData.attackRange;

            Collider[] colliders = new Collider[30];

            Physics.OverlapSphereNonAlloc(transform.position, unitData.attackRange, colliders);

            foreach (Collider collider in colliders)
            {
                if (gameObject.layer != collider.gameObject.layer)
                    continue;

                float currentDistance = (transform.position - collider.gameObject.transform.position).magnitude;

                if (IsTargetTeam(collider.gameObject) || currentDistance < distance)
                {
                    closestGameObject = collider.gameObject;
                    distance = currentDistance;
                }
            }

            targetableUnit = closestGameObject;

            if(closestGameObject != null)
                return true;
        }
        else
        {
            Collider[] colliders = new Collider[30];

            Physics.OverlapSphereNonAlloc(transform.position, unitData.attackRange, colliders);

            foreach (Collider collider in colliders)
            {
                if (gameObject.layer != collider.gameObject.layer)
                    continue;

                if(target == collider.gameObject)
                {
                    targetableUnit = target;
                    return true;
                }
            }
        }

        return false;
    }
    /// <summary>
    /// Check if the given units can be attacked according to the teamTarget tag (not specialTeamTarget) specified in unitData.
    /// </summary>
    /// <param name="target">A null target will return false.</param>
    /// <returns></returns>
    private bool IsTargetTeam(GameObject target)
    {
        if(target.TryGetComponent(out Unit unit))
        {
            return PossibleTargets.All == unitData.teamTarget ||
                (unit.IsAttacker == IsAttacker && unitData.teamTarget == PossibleTargets.AlliesOnly) ||
                (unit.IsAttacker != IsAttacker && unitData.teamTarget == PossibleTargets.EnemiesOnly);
        }

        return false;
    }
    private void PauseNavigation()
    {
        navigation.isStopped = true;
    }
    private void ResumeNavigation()
    {
        navigation.isStopped = false;
    }
    private Vector3? GetMousePositionOnTerrain(out Unit other) // Return mouse position on terrain, returns null if nothing was hit.
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // If it's an unit - set is as other and pass the collision point
            if(hit.collider.gameObject.TryGetComponent(out other))
                return hit.point;

            other = default;

            if ((terrainLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
                return hit.point;

        }
        other = default;
        return null;
    }
    private void SetDestination(Vector3? dest) // if null, reset and stops the navigation
    {
        if(dest != null)
        {
            navigation.destination = (Vector3)dest;
            navigation.isStopped = false;
        }
        else
        {
            navigation.isStopped = true;
            navigation.destination = transform.position;
        }
    }

    #endregion

    #endregion State Machine


    #region Health Related
    public void Damage(float damage)
    {
        if(!isInvincible)
            currentHp -= damage;
        else
        {
            // Do some fancy block effect or not
        }

        if(currentHp <= 0)
            KnockedDown();
    }

    public void Heal(float heal)
    {
        currentHp += heal;
        currentHp = Mathf.Clamp(currentHp, 0, unitData.maxHp);
    }

    private void KnockedDown()
    {
        if (!isInvulnerable) {
            Destroy(gameObject);
            // Or launch a fancy coroutine to show it died idk
        }
    }

    #endregion Health Related

    public void Select()
    {
        // TODO : Check if player is in the same team than this unit
        isSelected = true;
        // Unit is selected (with pointer mouse)
    }

    public void Deselect()
    {
        isSelected = false;
    }
}
