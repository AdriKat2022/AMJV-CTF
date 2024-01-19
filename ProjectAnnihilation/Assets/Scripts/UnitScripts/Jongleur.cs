using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class Jongleur : Unit
{
    #region variables
    [SerializeField] private Projectile script;

    #endregion
    protected override void Action(GameObject target = null)
    {
        base.Action();
        // Box detection for melee attacks

        // Or projectile launch

        // Or even just applying buffs to allies
        if (target == null)
            return;
        script.Launch(target.transform.position);
    }

    protected override void SpecialAction(GameObject target = null)
    {
        base.SpecialAction();
        // Throw a special attack (or special action
    }
}
