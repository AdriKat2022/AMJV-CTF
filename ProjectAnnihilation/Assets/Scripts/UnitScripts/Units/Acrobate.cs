using UnityEngine;

public class Acrobate : Unit
{
    [Header("Attack")]
    [SerializeField]
    private float hitstun;

    protected override bool Action(GameObject target = null)
    {
        if(!base.Action(target))
            return false;

        if(target == null)
            return false;

        animator.SetBool("Attack", true);

        DealDamage(target, unitData.Attack, hitstun);

        return true;
    }


    protected override bool SpecialAction(GameObject target = null)
    {
        if(!base.SpecialAction(target))
            return false;

        return true;
    }
}
