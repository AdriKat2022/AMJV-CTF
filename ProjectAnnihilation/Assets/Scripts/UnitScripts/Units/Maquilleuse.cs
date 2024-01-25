using UnityEngine;

public class Maquilleuse : Unit
{
    [Header("Maquillage")]
    [SerializeField]
    private float invisibilityDuration;


    protected override bool Action(GameObject target = null)
    {
        if (!base.Action())
            return false;

        animator.SetBool("Attack", true);
        DealDamage(target, unitData.Attack);

        return true;
    }
    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction())
            return false;

        StatusEffect<Unit> pw = new(PowerUpType.Invisibility, 0, invisibilityDuration, false);

        Unit farest = GetFarestAlly();

        if(farest != null)
            farest.ApplyStatus(pw);

        return true;
    }

    private Unit GetFarestAlly()
    {
        Collider[] cols = Physics.OverlapBox(transform.position, new Vector3(100, 2, 100), Quaternion.identity, LayerMask.GetMask("Unit"));

        Unit farestUnit = null;
        float distance = 0;

        foreach (Collider col in cols) {
            if(col == null) continue;

            if (!col.TryGetComponent(out Unit unit) || unit.IsAttacker != IsAttacker)
                continue;

            if(distance < (col.transform.position - transform.position).sqrMagnitude)
            {
                farestUnit = unit;
                distance = (col.transform.position - transform.position).sqrMagnitude;
            }
        }

        return farestUnit;
    }
}
