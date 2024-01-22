using System.Collections;
using UnityEngine;
using UnityEngine.AI;



public enum PossibleTargets
{
    All,
    EnemiesOnly,
    AlliesOnly,
    SelfOnly,
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
    #region Variables
    private static readonly int enemyDetectionBuffer = 10;


    [Header("Unit options")]
    [SerializeField]
    protected UnitData unitData;
    [SerializeField]
    protected bool isAttacker; // Defines the team of the unit
    // UIMap variable moved below in Private Script Variables
    

    // References
    protected NavMeshAgent navigation;
    protected HealthModule healthModule;
    protected GameManager gameManager;
    protected Rigidbody rb;


    // State Variables
    private bool isInvisible = false; // If other units can see them
    private bool isInvincible = false; // Cannot take damage
    private bool isInvulnerable = false; // Cannot die (hp cannot fall below 1)
    private bool isSelected;
    private bool isKing;
    private bool isInWater = false; // Serializing this is useless (except for debugging) so i removed it


    // Shared variables
    public UnitState CurrentOrder => currentOrder;
    public bool IsAttacker => isAttacker;
    public bool IsInvisible => isInvisible;
    public bool IsInvincible => isInvincible;
    public bool IsInvulnerable => isInvulnerable;
    public bool IsSelected => isSelected;
    public bool IsKing => isKing;
    public bool IsInWater => isInWater; // Is this useful ?
    public UnitData UnitData => unitData;


    // Private script variables (add serializeField to see it in the inspector (for debug)
    [Header("Debug")]
    private GameObject targetableUnit;
    private GameObject currentTile;
    private UiMap uiMap; // Removed it from this editor for now since it isn't used yet
    private UnitState lastCurrentOrder;
    private UnitState unitState;
    private UnitState currentOrder;
    private bool canAttack;
    private bool showAttackRange;
    private bool inEndLag;
    private bool usingTiles = true;
    private float endLagTimer;
    private float timeBeforeTargetting;
    private Transform followedTarget; // Following
    private Vector3 pointA; // Patrolling
    private Vector3 pointB;

    protected float actionCooldown;
    protected float specialActionCooldown;
    #endregion



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
                rend.material.color = unitData.NothingColor;
                break;

            case UnitState.IDLE:
                rend.material.color = unitData.IdleColor;
                break;

            case UnitState.MOVING:
                rend.material.color = unitData.MovingColor;
                break;

            case UnitState.MOVING_FOCUS:
                rend.material.color = unitData.MovingFocusedColor;
                break;

            case UnitState.MOVENATTACK:
                rend.material.color = unitData.ChaseColor;
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

        float blinkTimer = unitData.BlinkSpeed;
        bool state = false;

        while (isSelected)
        {

            if(state && blinkTimer >= unitData.BlinkSpeed)
            {
                blinkTimer = 0;
                state = !state;
                rend.material.color = unitData.SelectedColor;

                if(unitData.UseFullBlink)
                    statusObject.SetActive(false);
            }
            else if(!state && blinkTimer >= unitData.BlinkSpeed)
            {
                blinkTimer = 0;
                state = !state;

                if (unitData.UseFullBlink)
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
            Gizmos.DrawWireSphere(transform.position, unitData.AttackRange);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down);
    }

    #endregion

    #region UNIT ACTIONS (TO OVERRIDE BY UNIT)
    protected virtual bool Action(GameObject target = null) {
        if (actionCooldown > 0)
            return false;

        PauseNavigation();
        inEndLag = true;
        endLagTimer = unitData.AttackEndLag;
        actionCooldown = unitData.AttackRechargeTime;

        return true;
    }
    protected virtual bool SpecialAction(GameObject target = null) {
        if (specialActionCooldown > 0)
            return false;

        PauseNavigation();
        inEndLag = true;
        endLagTimer = unitData.SpecialAttackEndLag;
        specialActionCooldown = unitData.SpecialAttackRechargeTime;

        return true;
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
            DamageData dd = new(damage, hitstun ,knockback);
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
            DamageData dd = new(0, hitstun, knockback);
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

        navigation.speed = unitData.Speed;

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
        DecreaseCooldowns();

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

    private void DecreaseCooldowns()
    {
        actionCooldown = Mathf.Max(actionCooldown - Time.deltaTime, 0);
        specialActionCooldown = Mathf.Max(specialActionCooldown - Time.deltaTime, 0);
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
        Ray ray = new(gameObject.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == currentTile)
                return;


            //Debug.Log(hit.collider.gameObject);

            if (!hit.collider.gameObject.TryGetComponent(out Tile tile))
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
                navigation.speed = unitData.Speed;
            }

            // type 1 is the wall type, you should not be up there.

            // Type 2 is the slow type, it slows down the unit by half her speed (might change).
            if (tileType == 2)
            {
                navigation.speed = unitData.Speed/2;
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

                HoldPosition();

                break;

            case UnitState.MOVING:

                MovingAlertState();

                break;

            case UnitState.MOVING_FOCUS:

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
    /// Does nothing, but launches attacks to unit in range
    /// </summary>
    protected virtual void HoldPosition()
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
    protected virtual void MovingAlertState()
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
    protected virtual void MovingState()
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
            float distance = unitData.AttackRange;

            Collider[] colliders = new Collider[enemyDetectionBuffer];

            int nColliders = Physics.OverlapSphereNonAlloc(transform.position, unitData.AttackRange, colliders, unitData.UnitLayer);

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
            return unitData.CanSelfAttack;
        }
        else
        {
            Collider[] colliders = new Collider[30];

            int nColliders = Physics.OverlapSphereNonAlloc(transform.position, unitData.AttackRange, colliders, unitData.UnitLayer);

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
            return PossibleTargets.All == unitData.TeamTarget ||
                (unit.IsAttacker == IsAttacker && unitData.TeamTarget == PossibleTargets.AlliesOnly) ||
                (unit.IsAttacker != IsAttacker && unitData.TeamTarget == PossibleTargets.EnemiesOnly);
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

    public void BecomeKing() => isKing = true;

    #region Unit selection (interface)

    public void Select()
    {
        isSelected = true;

        if(unitData.BlinkOnSelected)
            StartCoroutine(BlinkIfSelected());
    }

    public void Deselect()
    {
        isSelected = false;
    }

    #endregion

    #region PowerUps

    protected float speedBonus;
    protected float attackBonus;
    protected float defenseBonus;

    private int attackBoostPowerUpsActive = 0;
    private int speedBoostPowerUpsActive = 0;
    private int defenseBoostPowerUpsActive = 0;
    private int invincibilityPowerUpsActive = 0;
    private int invulnerablePowerUpsActive = 0;

    public void ApplyBonuses(PowerUp[] powerUps)
    {
        foreach (PowerUp powerUp in powerUps)
        {
            StartCoroutine(ApplyPowerUp(powerUp));
        }
    }
    public void ApplyBonus(PowerUp powerUp)
    {
        StartCoroutine(ApplyPowerUp(powerUp));
    }

    private IEnumerator ApplyPowerUp(PowerUp powerUp)
    {
        switch (powerUp.powerUpType)
        {
            case PowerUpType.SpeedBoost:

                speedBoostPowerUpsActive++;
                speedBonus += powerUp.value;

                //speedBoostVisualModule.enabled = true;

                if (powerUp.hasExitCondition)
                    yield return new WaitUntil(powerUp.endCondition);
                else
                    yield return new WaitForSeconds(powerUp.duration);


                //if (speedPowerUpsActive == 0)
                //    speedBoostVisualModule.enabled = false;

                speedBoostPowerUpsActive--;
                speedBonus -= powerUp.value;

                break;

            case PowerUpType.AttackBoost:

                attackBoostPowerUpsActive++;
                attackBonus += powerUp.value;

                //jumpBoostVisualModule.enabled = true;


                if (powerUp.hasExitCondition)
                    yield return new WaitUntil(powerUp.endCondition);
                else
                    yield return new WaitForSeconds(powerUp.duration);


                attackBoostPowerUpsActive--;
                attackBonus -= powerUp.value;

                //if (attackBoostPowerUpsActive == 0)
                //    jumpBoostVisualModule.enabled = false;


                break;

            case PowerUpType.DefenseBoost:

                defenseBoostPowerUpsActive++;
                defenseBonus += powerUp.value;

                //jumpBoostVisualModule.enabled = true;


                if (powerUp.hasExitCondition)
                    yield return new WaitUntil(powerUp.endCondition);
                else
                    yield return new WaitForSeconds(powerUp.duration);


                defenseBoostPowerUpsActive--;
                defenseBonus -= powerUp.value;

                //if (attackBoostPowerUpsActive == 0)
                //    jumpBoostVisualModule.enabled = false;


                break;


            case PowerUpType.Invincibility:

                invincibilityPowerUpsActive++;
                isInvincible = true;


                if (powerUp.hasExitCondition)
                    yield return new WaitUntil(powerUp.endCondition);
                else
                    yield return new WaitForSeconds(powerUp.duration);


                invincibilityPowerUpsActive--;

                if(invincibilityPowerUpsActive <= 0)
                    isInvincible = false;

                break;

            case PowerUpType.Invulnerability:

                invincibilityPowerUpsActive++;
                isInvulnerable = true;


                if (powerUp.hasExitCondition)
                    yield return new WaitUntil(powerUp.endCondition);
                else
                    yield return new WaitForSeconds(powerUp.duration);


                invincibilityPowerUpsActive--;

                if (invincibilityPowerUpsActive <= 0)
                    isInvulnerable = false;

                break;

                /*invincibleTimer = 0;

                if (isInvincible)
                    yield break;


                OnPlayerInvincibility?.Invoke();

                heartParticlesEmissionModule.enabled = true;

                AudioClip lastMusic = SoundManager.Instance.GetCurrentMusic();

                SoundManager.Instance.PlayMusic(SoundManager.Instance.invincibility);

                rainbow.Activate();

                float previousMass = rb.mass;
                rb.mass = 9999;

                while (invincibleTimer < powerUp.duration)
                {
                    isInvincible = true;

                    invincibleTimer += Time.deltaTime;
                    yield return null;
                }

                OnPlayerInvincibilityEnd?.Invoke();

                isInvincible = false;

                rainbow.Deactivate();

                heartParticlesEmissionModule.enabled = false;


                rb.mass = previousMass;

                SoundManager.Instance.PlayMusic(lastMusic);

                break;*/

        }
    }
    #endregion
}
