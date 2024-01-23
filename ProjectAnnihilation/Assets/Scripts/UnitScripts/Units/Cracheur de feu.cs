using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Cracheurdefeu : Unit
{
    [Header("Flame Thrower")]
    [SerializeField]
    private float damageOverTime;
    [SerializeField]
    private float duration;
    [SerializeField]
    private float angle;
    [SerializeField]
    private float maxDistance;

    [Header("Debug")]
    [SerializeField]
    private Vector3 direction;

    private Vector3 dir;

    #region Debug

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        dir = direction.normalized;
        Vector3 perpendicularDirection = Vector3.Cross(Vector3.up, dir);

        //Gizmos.matrix = transform.localToWorldMatrix;
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCube(transform.InverseTransformPoint(transform.position + dir * maxDistance / 2), Mathf.Sin(angle * Mathf.PI / 180) * maxDistance * Vector3.right + maxDistance * Vector3.forward + Vector3.up);
        //transform.position + direction * maxDistance / 2, direction* maxDistance / 2 + Mathf.Sin(angle * Mathf.PI * 180) * maxDistance * perpendicularDirection
    }

#endif

    #endregion

    protected override bool Action(GameObject target = null)
    {
        if(!base.Action(target))
            return false;

        if(target == null)
            return false;

        StartCoroutine(CreateDamageCone(target, damageOverTime, duration, angle, maxDistance, unitData.AttackTargets));

        return true;
    }

    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction(target))
            return false;

        return true;
    }
}
