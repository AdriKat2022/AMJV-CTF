using UnityEngine;

public class Jongleur : Unit
{
    #region variables
    [SerializeField] private Projectile script;

    private bool specialState = false;
    #endregion

    protected override void Action(GameObject target = null)
    {
        base.Action();
        // Box detection for melee attacks

        // Or projectile launch

        // Or even just applying buffs to allies
        if (target == null)
            return;

        if(!specialState)
            script.Launch(target.transform.position, unitData.Attack);
        else
            DealDamage(target, unitData.Attack); // Melee attack if specialState is activated
    }

    protected override void SpecialAction(GameObject target = null)
    {
        base.SpecialAction();

        // Begins with false (specialState)
        // Switches when using special Action

        specialState = !specialState;

        unitData = unitData.OtherStateUnitData; // Allows to switch stats

        // Throw a special attack (or special action
    }
}
