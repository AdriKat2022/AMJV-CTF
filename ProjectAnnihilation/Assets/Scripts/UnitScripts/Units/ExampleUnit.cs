using UnityEngine;

public class ExampleUnit : Unit
{
    private bool firstFrame = true;

    protected override bool Action(GameObject target = null)
    {
        if(!base.Action())
            return false;

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

        if(firstFrame)
        {
            //Debug.Log("On end lag !");
            firstFrame = false;
        }
    }
}
