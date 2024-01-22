using UnityEngine;

public class Jongleur : Unit
{
    #region Variables
    [Header("Jongleur")]
    [SerializeField] private Projectile projectileScript;

    private bool specialState;
    #endregion

    protected override void Initialize() // A bit like the Start() function to initialize the unit
    {
        base.Initialize();

        specialState = false;
    }

    protected override void Action(GameObject target = null)
    {
        base.Action();
        // Box detection for melee attacks

        // Or projectile launch

        // Or even just applying buffs to allies
        if (target == null)
        {
            return;
        }
        transform.LookAt(target.transform.position);
        if (!specialState)
        {
            projectileScript.Launch(target.transform.position, unitData.Attack);
        }
        else
        {
            DealDamage(target, unitData.Attack); // Melee attack if specialState is activated
        }
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
