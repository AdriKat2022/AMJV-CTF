using System.Collections;
using UnityEngine;

public class Clown : Unit
{
    [Header("Diversion")]
    [SerializeField]
    private float maxDuration;

    private bool acted = false;

    protected override bool Action(GameObject target = null)
    {
        if(!base.Action(target))
            return false;

        if(target == null)
            return false;

        acted = true;

        StartCoroutine(Hits(target, unitData.Attack, 2f, 0.4f));

        return true;
    }

    protected override bool SpecialAction(GameObject target = null)
    {
        if(!base.SpecialAction(target))
            return false;

        acted = false;

        StartCoroutine(BeInvisible(maxDuration));

        return true;
    }

    private IEnumerator Hits(GameObject target, float totalDmg, float nHits, float totalDuration)
    {
        for(int i = 0; i < nHits; i++)
        {
            DealDamage(target, totalDmg / nHits);
            yield return new WaitForSeconds(totalDuration/nHits);
        }
    }

    private IEnumerator BeInvisible(float maxTime)
    {
        StatusEffect<Unit> pu = new(PowerUpType.Invisibility, 0, .2f, false);
        float _startTime = Time.time;

        while (!acted && Time.time - _startTime < maxTime)
        {
            ApplyStatus(pu);
            yield return new WaitForSeconds(.2f);
        }
    }
}
