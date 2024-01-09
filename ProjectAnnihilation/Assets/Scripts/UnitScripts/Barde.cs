using System.Collections;
using System.Collections.Generic;
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

        if (TryGetComponent(out IDamageable damageableTarget))
        {
            DamageData dd = new DamageData(5);
            damageableTarget.Damage(dd);
        }
    }

    protected override void SpecialAction(GameObject target = null)
    {
        base.SpecialAction();
        // Throw a special attack (or special action
    }
}
