using UnityEngine;

public class Barde : Unit
{
    protected override void Action(GameObject target = null)
    {
        base.Action();
        // Box detection for melee attacks

        // Or projectile launch

        // Or even just applying buffs to allies
        if (target == null)
            return;

        Vector3 knockbackDealt = (target.transform.position - transform.position).normalized * 20;

        //DealDamage(target, unitData.attack + attackBonus, knockback: knockbackDealt);

        CreateRepulsiveSphere(4, 10);
    }

    protected override void SpecialAction(GameObject target = null)
    {
        base.SpecialAction();
        // Throw a special attack (or special action
    }
}
