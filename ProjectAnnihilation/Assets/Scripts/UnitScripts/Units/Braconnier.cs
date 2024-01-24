using System.Collections;
using UnityEngine;

public class Braconnier : Unit
{
    [Header("Braconnier")]
    [SerializeField]
    private ProjectileLauncher projectileScript;
    [Header("Projectile")]
    [SerializeField]
    private int projectileNumber;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float angleDispersionPerBullet;
    [SerializeField]
    private float bulletRange;

    [Header("Boost capacity")]
    [SerializeField]
    private float duration = 5f;


    private float timespan;

    protected override void Initialize()
    {
        base.Initialize();

        timespan = bulletRange/speed;
    }

    protected override bool Action(GameObject target = null)
    {
        if (!base.Action(target))
            return false;

        if (target == null)
            return false;

        LaunchProjectilesAt(projectileNumber, target, angleDispersionPerBullet, timespan);

        return true;
    }

    private void LaunchProjectilesAt(int nP, GameObject target, float angle, float timespan)
    {
        DamageData dd = new(unitData.Attack/nP);

        Vector3 baseDirection = (target.transform.position - transform.position).normalized;

        Debug.DrawLine(transform.position, transform.position + baseDirection * speed, Color.magenta, 0.2f);

        angle *= Mathf.PI / 180;

        for (int i = 0; i < nP; i++)
        {
            Vector3 direction = baseDirection;
            float angleToBaseDirection = angle * (2 * i - nP + 1) / 2;

            direction.z = Mathf.Cos(angleToBaseDirection) * baseDirection.z - Mathf.Sin(angleToBaseDirection) * baseDirection.x;
            direction.x = Mathf.Cos(angleToBaseDirection) * baseDirection.x + Mathf.Sin(angleToBaseDirection) * baseDirection.z;

            direction.Normalize();
            projectileScript.LaunchRect(direction * speed, dd, timespan);
        }
    }

    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction(target))
            return false;

        StartCoroutine(BoostFor(duration));

        return true;
    }

    private IEnumerator BoostFor(float duration)
    {
        unitData = unitData.OtherStateUnitData;
        UpdateUnitData();
        yield return new WaitForSeconds(duration);
        unitData = unitData.OtherStateUnitData;
        UpdateUnitData();
    }
}
