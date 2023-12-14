using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour, ISelectable
{

    [SerializeField]
    private UnitData unitData;

    private NavMeshAgent navigation;

    private GameManager gameManager;

    public bool IsAttacker => isAttacker;
    private bool isAttacker; // Defines the team of the unit


    private float hp;
    private float speedBonus;

    private bool isFocused;

    public bool IsInvisible => isInvisible;
    private bool isInvisible = false; // If other units can see them
    private bool isInvincible = false;
    private bool isInvulnerable = false;

    public bool IsSelected => isSelected;
    private bool isSelected;


    [Header("Debug")]
    [SerializeField]
    private UnitState unitState;
    [SerializeField]
    private UnitState currentOrder;

    private bool inEndLag;
    private float endLagTimer;

    private Transform followedTarget; // Following

    private Vector3 pointA; // Patrolling
    private Vector3 pointB;

    #region UNIT ACTIONS (TO OVERRIDE BY UNIT)
    protected virtual void Action() { }
    protected virtual void SpecialAction() { }

    #endregion

    private enum UnitState
    {
        NOTHING, // Forces nothing
        IDLE,
        MOVING,
        MOVENATTACK,
        FOLLOWING,
        PATROLLING
    }

    private void OnValidate()
    {

    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        navigation = GetComponent<NavMeshAgent>();

        isSelected = false;
        isFocused = false;

        Initialize(unitData);
    }

    protected virtual void Initialize(UnitData unitData) // Can be overriden if a unit needs a specific initialization
    {
        hp = unitData.maxHp;
        inEndLag = false;
    }


    private void Update()
    {
        Debug.Log(unitState);

        if (inEndLag)
        {
            OnEndLag();
            return;
        }

        UpdateStateMachine();
    }

    /// <summary>
    /// Runs once per frame while in end lag.
    /// UpdateStateMachine is NOT called during end lag. This function is where you can act during those frames.
    /// 
    /// By default DECREASES the endLagTimer (in seconds) normally.
    /// </summary>
    protected virtual void OnEndLag()
    {
        endLagTimer -= Time.deltaTime;
        if (endLagTimer <= 0f)
        {
            inEndLag = false;
        }
    }

    protected virtual void ManageInput()
    {
        // TODO : manages inputs and switch currentOrder
    }

    #region State Machine

    private void UpdateStateMachine()
    {

        switch (unitState)
        {
            case UnitState.NOTHING:

                IdlingState();

                break;

            case UnitState.IDLE:

                IdlingState();

                break;

            case UnitState.MOVING:

                MovingState();

                break;

            case UnitState.MOVENATTACK:

                MoveNAttack();

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
    /// Does nothing, but launches attacks any unit in range
    /// </summary>
    protected virtual void IdlingState()
    {
        if (CanAttack() && currentOrder != UnitState.NOTHING)
        {
            Action();
        }
        else
        {
            if(currentOrder != UnitState.NOTHING)
                unitState = UnitState.IDLE;
        }
    }
    /// <summary>
    /// Moves towards a position (not a unit)
    /// If focus mode is enabled (isFocused), the unit doesn't care about attacking other units.
    /// </summary>
    protected virtual void MovingState()
    {
        navigation.isStopped = false;

        if(currentOrder != UnitState.MOVING)
        {
            unitState = currentOrder;
        }
        else if(CanAttack() && !isFocused)
        {

        }
    }
    /// <summary>
    /// Targets any other unit (followedUnit).
    /// Move towards them, and attack them once possible.
    /// </summary>
    protected virtual void MoveNAttack()
    {

        if (CanAttack() && unitState == UnitState.MOVENATTACK)
        {
            Action();
            PauseNavigation();
        }
        else
        {
            if (unitState != currentOrder)
            {
                unitState = currentOrder;  // Exit condition
                return;
            }
            SetDestination(followedTarget.position);
            ResumeNavigation();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    protected virtual void FollowingState()
    {
        navigation.destination = followedTarget.position;
        navigation.isStopped = false;

        if (CanAttack())
        {
            Action();
        }
        else
        {
            unitState = currentOrder;  // Exit condition
        }
    }
    /// <summary>
    /// 
    /// </summary>
    protected virtual void PatrollingState()
    {
        if (CanAttack())
        {
            Action();
            PauseNavigation();
        }
        else if(!inEndLag)
        {
            ResumeNavigation();
        }

        unitState = currentOrder;
    }

    #endregion

    #region Helper functions

    private bool CanAttack()
    {
        // TODO : Implement basic requirement for a unit to attack

        // TODO : Verify it's toward the unit we are attacking (if MOVENATTACK state)

        return true;
    }
    private void PauseNavigation()
    {
        navigation.isStopped = true;
    }
    private void ResumeNavigation()
    {
        navigation.isStopped = false;
    }
    private Vector3? GetMousePositionOnTerrain() // Return mouse position on terrain, returns null if nothing was hit.
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                return hit.point;
            }
        }
        return null;
    }
    private void SetDestination(Vector3? dest) // if null, 
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
            hp -= damage;
        else
        {
            // Do some fancy block effect or not
        }

        if(hp <= 0)
            KnockedDown();
    }

    public void Heal(float heal)
    {
        hp += heal;
        hp = Mathf.Clamp(hp, 0, unitData.maxHp);
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
