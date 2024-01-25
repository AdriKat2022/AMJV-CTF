using System.Collections;
using UnityEngine;

public class StrongMan : Unit
{
    [Header("Super Hero")]
    [SerializeField]
    private float lastInvulnerability = 20;

    protected override bool Action(GameObject target = null)
    {
        if (!base.Action())
            return false;

        animator.SetBool("Attack", true);
        DealDamage(target,unitData.Attack);
        return true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        StartCoroutine(PassiveInvulnerability());
    }

    private IEnumerator PassiveInvulnerability()
    {
        StatusEffect<Unit> pow = new(PowerUpType.Invulnerability, 0, .1f, false);

        yield return new WaitForSeconds(.5f);

        while(healthModule.CurrentHp > 1)
        {
            ApplyStatus(pow);
            yield return null;
        }
        
        pow = new(PowerUpType.Invulnerability, 0, lastInvulnerability, false);
        ApplyStatus(pow);
    
        yield return new WaitForSeconds(lastInvulnerability);
    }
}
