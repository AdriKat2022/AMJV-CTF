using UnityEngine;

public class Barde : Unit
{
    protected override bool Action(GameObject target = null)
    {
        if(!base.Action())
            return false;

        if (target == null)
            return false;

        Vector3 knockbackDealt = (target.transform.position - transform.position).normalized * 20;

        DealDamage(target, unitData.Attack + attackBonus, knockback: knockbackDealt);

        return true;
    }

    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction())
            return false;

        return true;
    }
}
