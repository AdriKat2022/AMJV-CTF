using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GardienDeZoo : Unit
{
    [Header("Gardien de Zoo")]
    [SerializeField]
    private ProjectileLauncher launcher;
    [Header("Darts")]
    [SerializeField]
    private int nDarts;
    //[SerializeField]
    //private float damagePerDart;
    [SerializeField]
    private float dartSpeed;
    [SerializeField]
    private float dartRange;
    [SerializeField]
    private float timeBetweenDarts;

    [Header("Trap")]
    [SerializeField]
    private float trap; // To replace with trap

    protected override bool Action(GameObject target = null)
    {
        if (!base.Action(target))
            return false;

        if (target == null)
            return false;

        animator.SetBool("Attack", true);
        StartCoroutine(LaunchSeveralBullets(target, nDarts, timeBetweenDarts));

        return true;
    }

    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction(target))
            return false;

        return true;
    }

    private IEnumerator LaunchSeveralBullets(GameObject target, int nBullets, float dT)
    {
        DamageData dd = new(unitData.Attack/nBullets);

        for (int i = 0; i < nBullets; i++)
        {
            if (target == null)
                yield break;
            launcher.LaunchRect((target.transform.position - transform.position).normalized * dartSpeed, dd, dartRange / dartSpeed);
            yield return new WaitForSeconds(dT);
        }
    }
}
