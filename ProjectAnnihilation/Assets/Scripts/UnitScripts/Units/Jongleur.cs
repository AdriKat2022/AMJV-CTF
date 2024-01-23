using UnityEngine;

public class Jongleur : Unit
{
    #region Variables
    [Header("Jongleur")]
    [SerializeField] private ProjectileLauncher projectileScript;

    private bool specialState;
    #endregion

    protected override void Initialize() // A bit like the Start() function to initialize the unit
    {
        base.Initialize();

        specialState = false;
    }

    protected override bool Action(GameObject target = null)
    {
        if (!base.Action())
            return false;

        if (target == null)
            return false;
        
        transform.LookAt(target.transform.position);
        if (!specialState)
        {
            projectileScript.LaunchArc(target.transform.position, new DamageData(unitData.Attack));
        }
        else
        {
            DealDamage(target, unitData.Attack); // Melee attack if specialState is activated
        }

        return true;
    }
    
    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction())
            return false;

        // Begins with false (specialState)
        // Switches when using special Action

        specialState = !specialState;

        unitData = unitData.OtherStateUnitData; // Allows to switch stats
        UpdateUnitData();

        return true;
    }
}
