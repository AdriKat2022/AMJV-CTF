using UnityEngine;

public class Barde : Unit
{
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

        return true;
    }
}
