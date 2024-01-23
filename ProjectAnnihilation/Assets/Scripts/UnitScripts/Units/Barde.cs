using UnityEngine;

public class Barde : Unit
{
    [Header("Royal music")]
    [SerializeField]
    private float radius;
    [SerializeField]
    private float duration;
    [SerializeField]
    private float attackMultiplierBonus = 1.3f;

    #region Debug Gizmoz

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

#endif

    #endregion

    protected override bool Action(GameObject target = null)
    {
        if(!base.Action())
            return false;

        if (target == null)
            return false;

        DealDamage(target, unitData.Attack);

        return true;
    }

    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction())
            return false;

        CreateMusicalSphere();

        return true;
    }

    private void CreateMusicalSphere()
    {
        StartCoroutine(CreatePowerUpSphere(attackMultiplierBonus, radius, duration, PowerUpType.AttackBoost));
    }
}
