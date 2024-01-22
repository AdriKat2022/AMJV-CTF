using UnityEngine;

public class ExampleUnit : Unit
{
    private bool firstFrame = true;

    protected override bool Action(GameObject target = null)
    {
        if(!base.Action())
            return false;

        // This will be executed only if the action is not on cooldown (automatically managed by the base script)

        // You generally want to check it the target is not null if the attack requires a target.

        // Example code to simply deal damage to the target :

        /*
        if(target == null)
            return false;

        DealDamage(target, unitData.damage)

        return true;
        */

        // You generally want to return true if the attack "succeeded" and false if the attack "failed"

        firstFrame = true;

        return true;
    }
    protected override bool SpecialAction(GameObject target = null)
    {
        if(!base.SpecialAction())
            return false;

        return true;
    }

    protected override void OnActionEndLag()
    {
        base.OnActionEndLag();

        // You can make your unit do something during its endlag, or completely replace the automatic management of the base script by not calling base.OnActionEndLag()
        // But by doing so, beware of decreasing manually the endLagTimer !

        if(firstFrame)
        {
            //Debug.Log("On end lag !");
            firstFrame = false;
        }
    }
}
