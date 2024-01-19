using System.Collections;
using UnityEngine;
using UnityEngine.AI;



public enum PossibleTargets
{
    All,
    EnemiesOnly,
    AlliesOnly,
    None
}

public enum UnitState
{
    NOTHING, // Forces nothing (pointless to be honest)
    IDLE,
    MOVING,
    MOVING_FOCUS,
    MOVENATTACK,
    FOLLOWING,
    PATROLLING
}

public class Unit : MonoBehaviour, ISelectable
{
    [SerializeField]
    protected UnitData unitData;
    public UnitData UnitData => unitData;

    [SerializeField] private UiMap uiMap;
    protected NavMeshAgent navigation;
    protected HealthModule healthModule;
    protected GameManager gameManager;
    private Rigidbody rb;



    protected float speedBonus;
    protected float attackBonus;


    private GameObject targetableUnit;


    [SerializeField]
    private bool isAttacker; // Defines the team of the unit
    public bool IsAttacker => isAttacker;

    public bool IsInvisible => isInvisible;
    private bool isInvisible = false; // If other units can see them
    private bool isInvincible = false; // Cannot take damage
    private bool isInvulnerable = false; // Cannot die (hp cannot fall below 1)

    public bool IsSelected => isSelected;
    private bool isSelected;

    public bool IsKing => isKing;
    [SerializeField]
    private bool isKing;

    public bool IsInWater => isInWater;
    [SerializeField]
    private bool isInWater = false;
    private GameObject currentTile;
    public UnitState CurrentOrder => currentOrder;

    [Header("Debug")]
    [SerializeField]
    private UnitState unitState;
    [SerializeField]
    private UnitState currentOrder;
    [SerializeField]
    private bool canAttack;
    [SerializeField]
    private bool showAttackRange;


    private UnitState lastCurrentOrder;


    [Header("Memory usage")]
    private static readonly int enemyDetectionBuffer = 10;

    private bool inEndLag;
    private float endLagTimer;

    private float timeBeforeTargetting;

    private Transform followedTarget; // Following

    private Vector3 pointA; // Patrolling
    private Vector3 pointB;

    private bool usingTiles = true;

    #region Status visuals

    private GameObject statusObject; // Defined by UserInput

    public void SetStatusObject(GameObject obj) => statusObject = obj;
    private void CheckCurrentOrderChange()
    {
        if(currentOrder != lastCurrentOrder)
        {
            lastCurrentOrder = currentOrder;
            UpdateStateVisual();
        }
    }
    private void UpdateStateVisual()
    {
        if (statusObject == null || !statusObject.TryGetComponent(out Renderer rend))
            return;

        switch (currentOrder)
        {
            case UnitState.NOTHING:
                rend.material.color = unitData.nothingColor;
                break;

            case UnitState.IDLE:
                rend.material.color = unitData.idleColor;
                break;

            case UnitState.MOVING:
                rend.material.color = unitData.movingColor;
                break;

            case UnitState.MOVING_FOCUS:
                rend.material.color = unitData.movingFocusedColor;
                break;

            case UnitState.MOVENATTACK:
                rend.material.color = unitData.chaseColor;
                break;

            case UnitState.FOLLOWING:
                //rend.material.color = unitData.color;
                break;

            case UnitState.PATROLLING:
                //rend.material.color = unitData.nothingColor;
                break;
        }
    }

    private IEnumerator BlinkIfSelected()
    {
        statusObject.TryGetComponent(out Renderer rend);
        statusObject.transform.localScale = Vector3.one * 2;

        float blinkTimer = unitData.blinkSpeed;
        bool state = false;

        while (isSelected)
        {

            if(state && blinkTimer >= unitData.blinkSpeed)
            {
                blinkTimer = 0;
                state = !state;
                rend.material.color = unitData.selectedColor;

                if(unitData.useFullBlink)
                    statusObject.SetActive(false);
            }
            else if(!state && blinkTimer >= unitData.blinkSpeed)
            {
                blinkTimer = 0;
                state = !state;

                if (unitData.useFullBlink)
                    statusObject.SetActive(true);

                UpdateStateVisual();
            }

            blinkTimer += Time.deltaTime;

            yield return null;
        }
        statusObject.transform.localScale = Vector3.one;

        statusObject.SetActive(true);

        UpdateStateVisual();
    }

    #endregion

    #region DEBUG (Gizmos)
    void OnDrawGizmosSelected()
    {
        if (showAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, unitData.attackRange);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down);
    }

    #endregion

    #region UNIT ACTIONS (TO OVERRIDE BY UNIT)
    protected virtual void Action(GameObject target = null) {
        PauseNavigation();
        inEndLag = true;
        endLagTimer = unitData.attackEndLag;
    }
    protected virtual void SpecialAction(GameObject target = null) {
        PauseNavigation();
        inEndLag = true;
        endLagTimer = unitData.specialAttackEndLag;
    }

    #endregion


    #region Classic actions

    protected void DealDamage(GameObject target, float damage, float hitstun = 0, Vector3? knockback = null) // Just deal simple damage to target
    {
        if(target == null)
        {
            Debug.LogWarning("Attempting to deal damage to null target");
            return;
        }

        if (target.TryGetComponent(out IDamageable damageableTarget))
        {
            DamageData dd = new DamageData(damage, hitstun ,knockback);
            damageableTarget.Damage(dd, healthModule);
        }
    }

    protected void DealKnockback(GameObject target, Vector3 knockback, float hitstun = 0)
    {
        if (target == null)
        {
            Debug.LogWarning("Attempting to deal damage to null target");
            return;
        }

        if (target.TryGetComponent(out IDamageable damageableTarget))
        {
            DamageData dd = new DamageData(0, hitstun, knockback);
            damageableTarget.Damage(dd, healthModule);
        }
    }

    protected void CreateRepulsiveSphere(float radius, float knockbackForce, Vector3? offset = null)
    {
        Collider[] units = new Collider[15];

        if(offset != null)
            Physics.OverlapSphereNonAlloc(transform.position + (Vector3)offset, radius, units);
        else
            Physics.OverlapSphereNonAlloc(transform.position, radius, units);

        // Do a animation or something

        foreach (Collider unit in units)
        {
            if (unit == null)
                continue;

            float forceFactor = (radius - (unit.transform.position - transform.position).magnitude) * knockbackForce;

            Vector3 knockback = (unit.transform.position - transform.position).normalized * forceFactor;

            DealKnockback(unit.gameObject, knockback);
        }
    }

    #endregion

    // TODO : create attack visual

    private void Awake()
    {
        gameObject.tag = isAttacker ? "Ally" : "Enemy";
        gameManager = GameManager.Instance;
    }
    private void Start()
    {
        navigation = GetComponent<NavMeshAgent>();
        healthModule = GetComponent<HealthModule>();
        rb = GetComponent<Rigidbody>();

        isSelected = false;
        isKing = false;
        canAttack = true;

        navigation.speed = unitData.speed;

        PassKnockbackFunctionToHealthModule();

        UpdateStateVisual();

        Initialize();
    }

    protected virtual void Initialize() // Can be overriden if a unit needs a specific initialization
    {
        inEndLag = false;

        unitState = UnitState.IDLE;

        speedBonus = 0;
        attackBonus = 0;

        timeBeforeTargetting = 0;
    }

    private void Update()
    {
        CheckCurrentOrderChange(); // For the visual of the unit

        if (inEndLag)
        {
            OnActionEndLag();
            return;
        }

        UpdateStateMachine();
    }

    private void FixedUpdate()
    {
        GroundUpdate();
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

    // MANAGED INPUT HAS BEEN MOVED TO USERINPUT.CS

    #region State management functions

    public void SetCurrentOrderState(UnitState order)
    {
        currentOrder = order;
    }
    public void SetFollowedTarget(Transform target)
    {
        followedTarget = target;
    }
    #endregion

    private void GroundUpdate()
    {
        RaycastHit hit;
        Ray ray = new Ray(gameObject.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == currentTile)
                return;

            Tile tile;

            //Debug.Log(hit.collider.gameObject);

            if(!hit.collider.gameObject.TryGetComponent(out tile))
            {
                if (usingTiles)
                {
                    Debug.LogWarning("No Tile detected beneath /!\\");
                    usingTiles = false;
                }
                return;
            }

            int tileType = tile.tileType;
            
            // type 0 is the default tile, there is nothing special.
            if (tileType == 0)
            {
                navigation.speed = unitData.speed;
            }

            // type 1 is the wall type, you should not be up there.

            // Type 2 is the slow type, it slows down the unit by half her speed (might change).
            if (tileType == 2)
            {
                navigation.speed = unitData.speed/2;
            }
            
            // Type 3 is the void type, every unit on this tile must die.
            if (tileType == 3)
            {
                healthModule.KnockedDown();
            }
            currentTile = hit.collider.gameObject;

            // Type 4 is the starting point, where you are supposed to bring the flag back
            if (tileType == 4 && IsKing==true)
            {
                GameManager.Instance.TriggerFinalMoove();
            }

            // Type 5 is the Flag.

            if ( tileType == 5 && IsAttacker == true)
            {
                if(hit.collider.gameObject.TryGetComponent(out Flag flag))
                {
                    Debug.Log("Entered");
                    if(flag.isFlagAvalaible == true)
                    {
                        BecomeKing();
                        flag.isFlagAvalaible= false;
                    }
                }
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

            case UnitState.MOVING_FOCUS:

                MovingFocusState();

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
        timeBeforeTargetting -= Time.deltaTime;
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
            Action(targetableUnit);
        }
        else
        {
            unitState = currentOrder;
        }
    }
    /// <summary>
    /// Moves towards a position (not a unit)<br />
    /// The unit will attack anyone in sight.
    /// </summary>
    protected virtual void MovingState()
    {
        ResumeNavigation();

        if(currentOrder != UnitState.MOVING)
        {
            unitState = currentOrder;
        }
        else if(CanAttack())
        {
            Action(targetableUnit);
        }

        if (HasArrived())
            currentOrder = UnitState.IDLE;
    }
    /// <summary>
    /// Moves towards a position (not a unit)<br />
    /// The unit doesn't care about attacking other units, until it has reached the end of his path.
    /// </summary>
    protected virtual void MovingFocusState()
    {
        ResumeNavigation();

        if (currentOrder != UnitState.MOVING_FOCUS)
        {
            unitState = currentOrder;
        }

        if (HasArrived())
            currentOrder = UnitState.IDLE;
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
            ResumeNavigation();

            if (CanAttack(followedTarget.gameObject))
            {
                Action(followedTarget.gameObject);
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
            Action(targetableUnit);
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
        // TODO : make it oscillate between pointA and pointB

        if (CanAttack() && currentOrder != UnitState.PATROLLING)
        {
            Action(targetableUnit);
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
    /// <summary>
    /// Check if the unit has arrived to its destination point of the NavMesh
    /// </summary>
    /// <returns></returns>
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
        if (target == null)
        {
            GameObject closestGameObject = null;
            float distance = unitData.attackRange;

            Collider[] colliders = new Collider[enemyDetectionBuffer];

            int nColliders = Physics.OverlapSphereNonAlloc(transform.position, unitData.attackRange, colliders, unitData.unitLayer);

            for(int i = 0; i<nColliders; i++)
            {
                Collider collider = colliders[i];

                if (collider == null || gameObject.layer != collider.gameObject.layer || gameObject == collider.gameObject)
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
            {
                canAttack = true;
                return true;
            }
        }
        else if (target == gameObject)
        {
            return unitData.canSelfAttack;
        }
        else
        {
            Collider[] colliders = new Collider[30];

            int nColliders = Physics.OverlapSphereNonAlloc(transform.position, unitData.attackRange, colliders, unitData.unitLayer);

            for (int i = 0; i < nColliders; i++)
            {
                Collider collider = colliders[i];

                // Skip if the collider is null or not a unit
                if (collider == null || gameObject.layer != collider.gameObject.layer)
                    continue;

                if(target == collider.gameObject)
                {
                    targetableUnit = target;
                    canAttack = true;
                    return true;
                }
            }
        }

        canAttack = false;
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
    protected void PauseNavigation()
    {
        navigation.isStopped = true;
    }
    protected void ResumeNavigation()
    {
        navigation.isStopped = false;
    }
    public void SetDestination(Vector3? dest) // if null, reset and stops the navigation
    {
        if(dest != null)
        {
            navigation.destination = (Vector3)dest;
        }
        else
        {
            navigation.isStopped = true;
            navigation.destination = transform.position;
        }
    }

    #endregion

    #endregion State Machine

    #region Knockback - HealthModule
    private IEnumerator GetKnockback(Vector3 knockback, Rigidbody rb)
    {
        rb.AddForce(knockback, ForceMode.Impulse);

        //while(knockback.magnitude > .1f)
        //{
        //    rb.AddForce(knockback);
        //    knockback = Vector3.Lerp(knockback, Vector3.zero, Time.deltaTime);

        //    yield return null;
        //}

        yield return null;
    }

    // 
    private void PassKnockbackFunctionToHealthModule()
    {
        healthModule.SetKnockbackCoroutine(GetKnockback, rb);
    }
    #endregion

    public void BecomeKing() { isKing = true; }

    #region Unit selection (interface)

    public void Select()
    {
        isSelected = true;

        if(unitData.blinkOnSelected)
            StartCoroutine(BlinkIfSelected());
    }

    public void Deselect()
    {
        isSelected = false;
    }

    #endregion
}
