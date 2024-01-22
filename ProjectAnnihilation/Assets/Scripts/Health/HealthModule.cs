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
    private float currentHP;
    private bool isAlive = true;

    private Func<Vector3, Rigidbody, IEnumerator> knockback_CR;
    private Rigidbody rb_unit;


#if UNITY_EDITOR

    private void OnValidate()
    {
        InitializeHPBarVisual();
    }

#endif

    private void Start()
    {
        TryGetComponent(out unit);

        unitData = unit.UnitData;
        currentHP = unitData.MaxHP;

        InitializeHPBarVisual();
    }

    #region IDamageable
    public void Damage(DamageData dmgData, IDamageable from = null)
    {
        if (CanTakeDamage())
        {
            currentHP -= ComputeDamage(dmgData);

            if(dmgData.knockback != null)
                ApplyKnockback((Vector3)dmgData.knockback);
        }
            // Some shield animation ? on the hp bar itself for example ?
        

        UpdateHPBarVisual();

        if(currentHP <= 0)
        {
            KnockedDown();
        }
    }

    public void Heal(float heal)
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
            return false;
        
        return true;
    }

    private float ComputeDamage(DamageData dmgData)
    {
        if (unit.IsInvulnerable)
            return Mathf.Clamp(dmgData.damage, 0, currentHP - 1);

        return dmgData.damage;
    }

    public void KnockedDown()
    {
        isAlive = false;
        if(gameObject.GetComponent<Unit>().IsKing == true) {
            GameManager.Instance.TriggerDeathOfTheKing();
        }
        if (gameObject.CompareTag("Enemy"))
        {
            GameManager.Instance.TriggerEnemyDeath();
        }
        if (gameObject.CompareTag("Ally"))
        {
            GameManager.Instance.TriggerAllyDeath();
        }
        Destroy(gameObject);
        // Or launch a fancy coroutine to show it died idk
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
