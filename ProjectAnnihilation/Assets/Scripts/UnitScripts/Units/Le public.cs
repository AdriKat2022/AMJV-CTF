using UnityEngine;

public class Lepublic : Unit
{
    [Header("Le public")]
    [SerializeField]
    private float radius;
    [SerializeField]
    private float knockbackForce;

    [Header("Protection")]
    [SerializeField]
    private float defenseBoost;
    [SerializeField]
    private float duration;


    protected override bool Action(GameObject target = null)
    {
        if (!base.Action(target))
            return false;

        if (target == null)
            return false;

        CreateRepulsiveSphere(radius, knockbackForce, useAttackBonus: true);

        return true;
    }


    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction(target))
            return false;
        StartCoroutine(
                CreatePowerUpSphere(defenseBoost, radius, duration, PowerUpType.DefenseBoost, isMultiplier: false, TargetType.AlliesOnly));
        return true;
    }
}
