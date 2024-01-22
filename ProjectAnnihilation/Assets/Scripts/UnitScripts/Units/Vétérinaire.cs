using System.Collections;
using UnityEngine;

public class Vétérinaire : Unit
{
    private const float INVULNERABILITY_DELTA_TIME = 0.5f;
    private const int MAX_DETECTED_UNITS = 10;

    [Header("Divine Area")]
    [SerializeField]
    private float duration;
    [SerializeField]
    private float radius;


    private bool specialAttackActivable = false;

    protected override void Action(GameObject target = null)
    {
        base.Action();
        if (target == null)
            return;

        if(target.TryGetComponent(out IDamageable targetHealthModule))
            targetHealthModule.Heal(unitData.Attack);
    }

    protected override void SpecialAction(GameObject target = null)
    {
        base.SpecialAction();

        if (specialAttackActivable)
        {
            specialAttackActivable = false;
            StartCoroutine(DivineSphere());
        }
    }

    private IEnumerator DivineSphere()
    {
        // DivineSphere gives invulnerability (hp not going below 1) bonuses to units located in sphere
        // It achieves this by scanning every INVULNERABILITY_DELTA_TIME seconds the close by units to give the a bonus that lasts INVULNERABILITY_DELTA_TIME
        // This is to avoid calling Physics Overlap and TryGetComponent() at every frame



        float _startTime = Time.time;

        PowerUp invulnerability = new(PowerUpType.Invulnerability, 1, INVULNERABILITY_DELTA_TIME);

        while(Time.time - _startTime < duration)
        {
            Collider[] cols = new Collider[MAX_DETECTED_UNITS];
            Physics.OverlapSphereNonAlloc(transform.position, radius, cols);

            foreach (Collider col in cols)
            {
                if(col.gameObject.TryGetComponent(out Unit unit))
                {
                    unit.ApplyBonus(invulnerability);
                }
            }
            yield return new WaitForSeconds(INVULNERABILITY_DELTA_TIME/1.2f);
        }

        yield return new WaitForSeconds(unitData.SpecialAttackRechargeTime);

        specialAttackActivable = true;
    }
}
