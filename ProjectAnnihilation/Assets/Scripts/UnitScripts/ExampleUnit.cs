using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleUnit : Unit
{
    private bool firstFrame = true;

    protected override void Action(GameObject target = null)
    {
        base.Action();
        firstFrame = true;
    }
    protected override void SpecialAction(GameObject target = null)
    {
        base.SpecialAction();
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
