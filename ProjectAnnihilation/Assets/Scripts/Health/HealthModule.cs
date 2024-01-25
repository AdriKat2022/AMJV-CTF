using System;
using System.Collections;
using UnityEngine;

public class HealthModule : MonoBehaviour, IDamageable
{
    [Header("Looks")]
    [SerializeField]
    private HealthBarModule healthBarModule;
    [SerializeField]
    private Color hpColor;
    [SerializeField]
    private Color hpBgColor;
    [SerializeField, Tooltip("If true, scales the health bar according to the max hp value of the unit.")]
    private bool adaptativeHealthBar;
    [SerializeField]
    private ParticleSystem healParticles;

    private Unit unit;
    private UnitData unitData;

    [SerializeField]
    private int currentHP;
    private bool isAlive = true;

    private Func<Vector3, Rigidbody, IEnumerator> knockback_CR;
    private Rigidbody rb_unit;
    private UnitUIManager unitUIManager;

    public bool IsAttacker => unit.IsAttacker;
    public int CurrentHp => currentHP;

#if UNITY_EDITOR

    private void OnValidate()
    {
        InitializeHPBarVisual();
    }

#endif

    private void Start()
    {
        TryGetComponent(out unit);
        TryGetComponent(out unitUIManager);

        unitData = unit.UnitData;
        currentHP = unitData.MaxHP;

        InitializeHPBarVisual();
    }

    #region IDamageable
    public void Damage(DamageData dmgData, IDamageable from = null)
    {
        if (CanTakeDamage())
        {
            int dmg = ComputeDamage(dmgData);
            currentHP -= dmg;
            unitUIManager.ShowDamage(dmg);

            if(dmgData.knockback != null)
                ApplyKnockback((Vector3)dmgData.knockback);

            ApplyStun(dmgData);
        }
            // Some shield animation ? on the hp bar itself for example ?
        

        UpdateHPBarVisual();

        if(currentHP <= 0)
        {
            KnockedDown();
        }
    }

    public void Heal(int heal)
    {
        if (!isAlive)
            return;

        currentHP = Mathf.Clamp(currentHP + heal, 0, unitData.MaxHP);
        UpdateHPBarVisual();
        healParticles.Play();
    }
    #endregion

    private bool CanTakeDamage()
    {
        if (unit.IsInvincible)
        {
            unitUIManager.PulseInvincibilityIcon();
            return false;
        }
        
        return true;
    }

    private int ComputeDamage(DamageData dmgData)
    {
        dmgData.damage -= dmgData.ignoreDefense ? 0 : unit.GetArmor();

        if (unit.IsInvulnerable)
        {
            if (dmgData.damage > currentHP)
                unitUIManager.PulseInvulnerableIcon();
            return Mathf.Clamp(dmgData.damage, 0, currentHP - 1);
        }

        return Mathf.Max(0, dmgData.damage);
    }

    public void KnockedDown()
    {
        isAlive = false;

        if (unit.IsKing)
            GameManager.Instance.TriggerDeathOfTheKing();

        Destroy(gameObject);
        // Or launch a fancy coroutine to show it died idk
    }

    private void ApplyStun(DamageData dd)
    {
        if (dd.hitStun <= 0)
            return;

        StatusEffect<Unit> po = new(PowerUpType.Stun, 0, dd.hitStun, false);

        unit.ApplyStatus(po);
    }

    #region HPBar Visual
    private void InitializeHPBarVisual()
    {
        if (healthBarModule != null && unitData != null)
            healthBarModule.Initialize(unitData, hpBgColor, hpColor, adaptativeHealthBar);
        else if (healthBarModule != null)
            healthBarModule.SoftInitialize(hpBgColor, hpColor);
    }
    private void UpdateHPBarVisual()
    {
        healthBarModule.SetCurrentHP(currentHP);
    }
    #endregion


    #region Knockback
    public void SetKnockbackCoroutine(Func<Vector3, Rigidbody, IEnumerator> knockbackCoroutine, Rigidbody rb)
    {
        knockback_CR = knockbackCoroutine;
        rb_unit = rb;
    }
    private void ApplyKnockback(Vector3 knockback)
    {
        if(knockback_CR == null)
        {
            Debug.LogWarning("No knockback defined, but a health module needs to use one.");
            return;
        }

        try
        {
            StartCoroutine(knockback_CR(knockback, rb_unit));
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            Debug.LogError("Knockback coroutine generator function was baddly written and couldn't be executed.\nAre you missing arguments ?");
        }
    }
    #endregion

}
