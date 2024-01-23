using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;


public enum TargetType
{
    All,
    EnemiesOnly,
    AlliesOnly,
    SelfOnly,
    None
}

public enum UnitState
{
    NOTHING, // Forces nothing
    IDLE,
    MOVING,
    MOVING_ALERT,
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
    private ParticleSystem flamethrowerParticles;


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


    // State Variables
    private bool isInvisible = false; // If other units can see them
    private bool isInvincible = false; // Cannot take damage
    private bool isInvulnerable = false; // Cannot die (hp cannot fall below 1)
    private bool isSelected = false;
    private bool isKing = false;
    private bool isInWater = false; // Serializing this is useless (except for debugging) so i removed it


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
    private float timeBeforeTargetting;
    private Transform followedTarget; // Following
    private Vector3 pointA; // Patrolling
    private Vector3 pointB;

    protected float endLagTimer;
    protected float actionCooldown;
    protected float specialActionCooldown;
    #endregion



    #region Visuals & animation

    private GameObject statusObject; // Defined by UserInput
    private TMP_Text unitText; // Defined by UserInput

    public void SetStatusObject(GameObject statusObject) => this.statusObject = statusObject;
    public void SetUnitText(TMP_Text unitText) => this.unitText = unitText;
    public void SetFlameThrowerParticles(ParticleSystem flamethrowerParticles) => this.flamethrowerParticles = flamethrowerParticles;
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

            case UnitState.MOVING_ALERT:
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
                //rend.material.color = unitData.SelectedColor;

                unitText.color = IsAttacker ? unitData.AttackerColor : unitData.DefenserColor;

                if(unitData.UseFullBlink)
                    statusObject.SetActive(false);
            }
            else if(!state && blinkTimer >= unitData.BlinkSpeed)
            {
                blinkTimer = 0;
                state = !state;
                //rend.material.color = unitData.color;

                unitText.color = unitData.SelectedColor;

                if (unitData.UseFullBlink)
                    statusObject.SetActive(true);

                UpdateStateVisual();
            }

            blinkTimer += Time.deltaTime;

            yield return null;
        }
        statusObject.transform.localScale = Vector3.one;
        statusObject.SetActive(true);
        unitText.color = IsAttacker ? unitData.AttackerColor : unitData.DefenserColor;

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

    #region Unit actions
    /// <summary>
    /// Deal simple damage to the target
    /// </summary>
    /// <param name="target">The target that will take damage</param>
    /// <param name="damage">How much damage</param>
    /// <param name="hitstun">Is there hitstun ? (acts as endlag)</param>
    /// <param name="knockback">Is there knockback ?</param>
    /// <param name="excludeAttackBonus">Do you want to ignore attack bonuses ?</param>
    /// <param name="ignoreDefense">Do you want to ignore the target's defense ?</param>
    protected void DealDamage(GameObject target, float damage, float hitstun = 0, Vector3? knockback = null, bool excludeAttackBonus = false, bool ignoreDefense = false) // Just deal simple damage to target
    {
        if(target == null)
        {
            Debug.LogWarning("Attempting to deal damage to null target");
            return;
        }

        if (target.TryGetComponent(out IDamageable damageableTarget))
        {
            DamageData dd = new(excludeAttackBonus ? damage : ((damage+attackBonusAdd) * attackBonusMultiplier), hitstun, knockback, ignoreDefense);
            damageableTarget.Damage(dd, healthModule);
        }
    }
    /// <summary>
    /// Heal a target
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="heal">Amount to heal</param>
    protected void Heal(GameObject target, float heal, bool excludeAttackMultiplier = false)
    {
        if (target.TryGetComponent(out IDamageable targetHealthModule))
        {
            targetHealthModule.Heal(excludeAttackMultiplier ? heal : ((heal + attackBonusAdd) * attackBonusMultiplier));
        }
        else
            Debug.LogWarning("Tried to heal a non healable target", gameObject);
    }
    /// <summary>
    /// Deals simple knockback to a target
    /// </summary>
    /// <param name="target">The target to push</param>
    /// <param name="knockback">The vector knockback</param>
    /// <param name="hitstun">Is there a hitstun ? (acts as endlag for the target)</param>
    protected void DealKnockback(GameObject target, Vector3 knockback, float hitstun = 0, bool useAttackBonus = false)
    {
        if (target == null)
        {
            Debug.LogWarning("Attempting to deal damage to null target");
            return;
        }

        if (target.TryGetComponent(out IDamageable damageableTarget))
        {
            DamageData dd = new(0, hitstun, knockback * (useAttackBonus ? (attackBonusAdd * attackBonusMultiplier) : 1));
            damageableTarget.Damage(dd, healthModule);
        }
    }
    /// <summary>
    /// Creates an one-frame Repulsive Sphere that pushes every unit in its radius
    /// </summary>
    /// <param name="radius">The sphere radius</param>
    /// <param name="knockbackForce">The max knockbackforce</param>
    /// <param name="offset">Let it null for the sphere center to be at the unit's position</param>
    protected void CreateRepulsiveSphere(float radius, float knockbackForce, Vector3? offset = null, bool useAttackBonus = false)
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

            DealKnockback(unit.gameObject, knockback, useAttackBonus: useAttackBonus);
        }
    }
    /// <summary>
    /// Creates a sphere where any unit will be granted the selected powerUp
    /// </summary>
    /// <param name="power">Power of the power up granted to units in the sphere</param>
    /// <param name="radius">Radius of the sphere</param>
    /// <param name="duration">Duration of the sphere</param>
    /// <param name="powerUpType">The type of powerup granted to units in the sphere</param>
    /// <param name="targets">Units that will interact with the sphere</param>
    /// <param name="powerUpDeltaTime">Minimum time interval of the power up given to a unit.</br
    /// The little, the better, but the more costly in performances.</br>
    /// I don't think a value very close to zero will work.</param>
    /// <returns></returns>
    protected IEnumerator CreatePowerUpSphere(float power, float radius, float duration, PowerUpType powerUpType, bool isMultiplier = true, TargetType targets = TargetType.All, float powerUpDeltaTime = .5f)
    {
        float _startTime = Time.time;

        PowerUp<Unit> powerUp = new(powerUpType, power, powerUpDeltaTime, isMultiplier, this);


        while (Time.time - _startTime < duration)
        {
            Collider[] cols = new Collider[15];
            Physics.OverlapSphereNonAlloc(transform.position, radius, cols);

            foreach (Collider col in cols)
            {
                if (col == null) continue;

                if (col.gameObject.TryGetComponent(out Unit unit))
                {
                    switch (targets)
                    {
                        case TargetType.All:
                            unit.ApplyBonus(powerUp);
                            break;

                        case TargetType.AlliesOnly:
                            if(unit.IsAttacker == this.IsAttacker)
                                unit.ApplyBonus(powerUp);
                            break;

                        case TargetType.EnemiesOnly:
                            if (unit.IsAttacker != this.IsAttacker)
                                unit.ApplyBonus(powerUp);
                            break;
                    }
                }
            }
            yield return new WaitForSeconds(powerUpDeltaTime / 1.2f);
        }
    }
    /// <summary>
    /// Creates a damage cone in where specified units take damage overtime.
    /// </summary>
    /// <param name="target">The target targetted. He might not be the only one to take damage.</param>
    /// <param name="damageOverTime">Damage done to every unit inside the cone per tick.</param>
    /// <param name="duration">How much time it lasts.</param>
    /// <param name="angle">Angle from the vector between the unit and the target, and the unit that will be touched by the flame thrower.</param>
    /// <param name="maxDistance">Max distance from the unit the flame thrower will go.</param>
    /// <param name="targets">What kind of targets can be damaged ?</param>
    /// <param name="excludeAttackBonus">Does this flame thrower ignores bonuses granted to the casting unit ?</param>
    /// <param name="ignoreDefense">Ignore the units defenses ?</param>
    /// <param name="damageDeltaTime">How much time a tick takes.</param>
    /// <returns></returns>
    protected IEnumerator CreateDamageCone(GameObject target, float damageOverTime, float duration, float angle, float maxDistance, TargetType targets = TargetType.All, bool excludeAttackBonus = false, bool ignoreDefense = false, float damageDeltaTime = .2f)
    {
        float _startTime = Time.time;

        DamageData dd = new(excludeAttackBonus ? damageOverTime : (damageOverTime+attackBonusAdd)*attackBonusMultiplier);

        Vector3 direction = (target.transform.position - transform.position).normalized;
        //Vector3 perpendicularDirection = Vector3.Cross(Vector3.up, direction);

        SetUpFlameThrowerParticles(duration, maxDistance, angle);

        Vector3 perpendicularDirection = Vector3.Cross(Vector3.up, direction).normalized;

        while (Time.time - _startTime < duration)
        {
            Collider[] cols = new Collider[15];

            //Physics.OverlapBoxNonAlloc(transform.position + direction * maxDistance / 2, direction * maxDistance / 2 + Mathf.Sin(angle * Mathf.PI * 180) * maxDistance * perpendicularDirection, cols);
            Physics.OverlapBoxNonAlloc(transform.position + direction * maxDistance / 2, Mathf.Sin(angle * Mathf.PI / 180) * maxDistance * Vector3.right + maxDistance * Vector3.forward + Vector3.up, cols, transform.rotation);

            foreach (Collider col in cols)
            {
                if (col == null)
                    continue;

                //Debug.Log(Vector3.Angle(direction, col.bounds.center - transform.position));
                Debug.Log(col.gameObject.name);

                if (Mathf.Abs(Vector3.Angle(direction, col.bounds.center - transform.position)) > angle)
                    continue;

                Debug.Log("one unit in flamethrower");
                
                if (col.gameObject.TryGetComponent(out HealthModule unit))
                {

                    switch (targets)
                    {
                        case TargetType.All:
                            unit.Damage(dd);
                            break;

                        case TargetType.AlliesOnly:
                            if (unit.IsAttacker == this.IsAttacker)
                                unit.Damage(dd);
                            break;

                        case TargetType.EnemiesOnly:
                            if (unit.IsAttacker != this.IsAttacker)
                                unit.Damage(dd);
                            break;
                    }
                }
            }
            yield return new WaitForSeconds(damageDeltaTime);
        }
    }
    #endregion


    private void SetUpFlameThrowerParticles(float duration, float maxDistance, float angle)
    {
        float _SPEED = 9;

        if (flamethrowerParticles == null)
        {
            Debug.LogError("No flamethrower particles");
            return;
        }
        ParticleSystem.MainModule main = flamethrowerParticles.main;
        ParticleSystem.ShapeModule shape = flamethrowerParticles.shape;
        main.duration = duration;
        shape.angle = angle;
        main.startLifetime = maxDistance/_SPEED;
        main.startSpeed = _SPEED;

        flamethrowerParticles.Play();
    }

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

        bonusSpeedMaintainers = new();
        bonusAttackMaintainers = new();
        bonusDefenseMaintainers = new();

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

        speedBonusMultiplier = 1;
        attackBonusMultiplier = 1;
        defenseBonusMultiplier = 1;
        speedBonusAdd = 0;
        attackBonusAdd = 0;
        defenseBonusAdd = 0;

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

    public void ActivateSpecialAbility()
    {
        SpecialAction();
    }

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

    #region Helper functions
    /// <summary>
    /// Can the unit attack the target according to the AttackTargets property of the unit ?
    /// </summary>
    /// <param name="target">The target to test</param>
    /// <param name="specialAttack">Set true to look for the SpecialAttackTargets property instead</param>
    /// <returns>If the unit can target the target</returns>
    public bool CanTarget(Unit target, bool specialAttack = false)
    {
        bool selfAttackRuleRespected = this != target || (specialAttack ? unitData.CanSelfSpecialAttack : unitData.CanSelfAttack);

        return (specialAttack ? unitData.SpecialAttackTargets : unitData.AttackTargets) switch
        {
            TargetType.All => selfAttackRuleRespected,
            TargetType.EnemiesOnly => isAttacker != target.IsAttacker && selfAttackRuleRespected,
            TargetType.AlliesOnly => isAttacker == target.IsAttacker && selfAttackRuleRespected,
            TargetType.SelfOnly => isAttacker == target, // self attack rule is ignored with this target type
            TargetType.None => false,
            _ => false,
        };
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

            case UnitState.MOVING_ALERT:

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

        if (currentOrder != UnitState.MOVING_ALERT)
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

                if (!collider.TryGetComponent(out Unit unit) || !CanTarget(unit))
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
            return TargetType.All == unitData.AttackTargets ||
                (unit.IsAttacker == IsAttacker && unitData.AttackTargets == TargetType.AlliesOnly) ||
                (unit.IsAttacker != IsAttacker && unitData.AttackTargets == TargetType.EnemiesOnly);
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

    protected float speedBonusMultiplier = 1;
    protected float attackBonusMultiplier = 1;
    protected float defenseBonusMultiplier = 1;
    protected float speedBonusAdd = 0;
    protected float attackBonusAdd = 0;
    protected float defenseBonusAdd = 0;

    private int attackBoostPowerUpsActive = 0;
    private int speedBoostPowerUpsActive = 0;
    private int defenseBoostPowerUpsActive = 0;

    private int invincibilityPowerUpsActive = 0;
    private int invulnerablePowerUpsActive = 0;

    private Dictionary<Unit, int> bonusSpeedMaintainers;
    private Dictionary<Unit, int> bonusAttackMaintainers;
    private Dictionary<Unit, int> bonusDefenseMaintainers;

    public void ApplyBonuses(PowerUp<Unit>[] powerUps)
    {
        foreach (PowerUp<Unit> powerUp in powerUps)
        {
            ApplyBonus(powerUp);
        }
    }
    public void ApplyBonus(PowerUp<Unit> powerUp)
    {
        switch (powerUp.type)
        {
            case PowerUpType.SpeedBoost:
                if (!bonusSpeedMaintainers.ContainsKey(powerUp.reference))
                    bonusSpeedMaintainers.Add(powerUp.reference, 0);
                break;

            case PowerUpType.AttackBoost:
                if (!bonusAttackMaintainers.ContainsKey(powerUp.reference))
                    bonusAttackMaintainers.Add(powerUp.reference, 0);
                break;

            case PowerUpType.DefenseBoost:
                if (!bonusDefenseMaintainers.ContainsKey(powerUp.reference))
                    bonusDefenseMaintainers.Add(powerUp.reference, 0);
                break;
        }
        
        StartCoroutine(ApplyPowerUp(powerUp));
    }
    private IEnumerator ApplyPowerUp(PowerUp<Unit> powerUp)
    {
        switch (powerUp.type)
        {
            case PowerUpType.SpeedBoost:

                AddRawBonus(powerUp);

                if (powerUp.hasExitCondition)
                    yield return new WaitUntil(powerUp.endCondition);
                else
                    yield return new WaitForSeconds(powerUp.duration);

                RemoveRawBonus(powerUp);

                break;

            case PowerUpType.AttackBoost:

                AddRawBonus(powerUp);

                if (powerUp.hasExitCondition)
                    yield return new WaitUntil(powerUp.endCondition);
                else
                    yield return new WaitForSeconds(powerUp.duration);

                RemoveRawBonus(powerUp);


                break;

            case PowerUpType.DefenseBoost:

                AddRawBonus(powerUp);


                if (powerUp.hasExitCondition)
                    yield return new WaitUntil(powerUp.endCondition);
                else
                    yield return new WaitForSeconds(powerUp.duration);

                RemoveRawBonus(powerUp);


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

                invulnerablePowerUpsActive++;
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

    private void AddRawBonus(PowerUp<Unit> powerUp)
    {
        switch (powerUp.type)
        {
            case PowerUpType.SpeedBoost:

                if (powerUp.reference == null || bonusSpeedMaintainers[powerUp.reference] == 0)
                {
                    if (powerUp.isMultiplier)
                        speedBonusMultiplier *= powerUp.value;
                    else
                        speedBonusAdd += powerUp.value;
                    speedBoostPowerUpsActive++;
                }

                bonusSpeedMaintainers[powerUp.reference] += 1;

                break;

            case PowerUpType.AttackBoost:

                if (powerUp.reference == null || bonusAttackMaintainers[powerUp.reference] == 0)
                {
                    if (powerUp.isMultiplier)
                        attackBonusMultiplier *= powerUp.value;
                    else
                        attackBonusAdd += powerUp.value;
                    attackBoostPowerUpsActive++;
                }

                bonusAttackMaintainers[powerUp.reference] += 1;

                break;

            case PowerUpType.DefenseBoost:

                if (powerUp.reference == null || bonusDefenseMaintainers[powerUp.reference] == 0)
                {
                    if (powerUp.isMultiplier)
                        defenseBonusMultiplier *= powerUp.value;
                    else
                        defenseBonusAdd += powerUp.value;
                    defenseBoostPowerUpsActive++;
                }

                bonusDefenseMaintainers[powerUp.reference] += 1;

                break;
        }
    }

    private void RemoveRawBonus(PowerUp<Unit> powerUp)
    {
        switch (powerUp.type)
        {
            case PowerUpType.SpeedBoost:

                bonusSpeedMaintainers[powerUp.reference] -= 1;

                if (powerUp.reference == null || bonusSpeedMaintainers[powerUp.reference] == 0)
                {
                    if (powerUp.isMultiplier)
                        speedBonusMultiplier /= powerUp.value;
                    else
                        speedBonusAdd -= powerUp.value;
                    speedBoostPowerUpsActive--;
                }

                break;

            case PowerUpType.AttackBoost:

                bonusAttackMaintainers[powerUp.reference] -= 1;

                if (powerUp.reference == null || bonusAttackMaintainers[powerUp.reference] == 0)
                {
                    if (powerUp.isMultiplier)
                        attackBonusMultiplier /= powerUp.value;
                    else
                        attackBonusAdd -= powerUp.value;
                    attackBoostPowerUpsActive--;
                }


                break;

            case PowerUpType.DefenseBoost:

                bonusDefenseMaintainers[powerUp.reference] -= 1;

                if (powerUp.reference == null || bonusDefenseMaintainers[powerUp.reference] == 0)
                {
                    if (powerUp.isMultiplier)
                        defenseBonusMultiplier /= powerUp.value;
                    else
                        defenseBonusAdd -= powerUp.value;
                    defenseBoostPowerUpsActive--;
                }

                break;
        }
    }

    #endregion
}
