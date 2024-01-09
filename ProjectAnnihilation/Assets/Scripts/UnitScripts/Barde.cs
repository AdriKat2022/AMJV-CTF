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

        DealDamage(target, unitData.attack + attackBonus);
    }

    protected override void SpecialAction(GameObject target = null)
    {
        base.SpecialAction();
        // Throw a special attack (or special action
    }
}
