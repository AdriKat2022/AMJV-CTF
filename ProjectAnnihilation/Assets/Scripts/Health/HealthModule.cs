using System;
using System.Collections;
using Unity.VisualScripting;
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



    private Unit unit;
    private UnitData unitData;

    [SerializeField]
    private float currentHP;
    private bool canTakeDamage;
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
        canTakeDamage = true;

        TryGetComponent(out unit);

        unitData = unit.UnitData;
        currentHP = unitData.maxHP;

        InitializeHPBarVisual();
    }

    #region IDamageable
    public void Damage(DamageData damageData, IDamageable from = null)
    {
        if (canTakeDamage)
        {
            currentHP -= damageData.damage;

            if(damageData.knockback != null)
                ApplyKnockback((Vector3)damageData.knockback);
        }
        else
        {
            // Some shield animation ? on the hp bar itself for example ?
        }

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

        currentHP += heal;
        currentHP = Mathf.Clamp(currentHP, 0, unitData.maxHP);
    }
    #endregion

    public void KnockedDown()
    {
        isAlive = false;
        GameManager.Instance.TriggerDeath();
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
