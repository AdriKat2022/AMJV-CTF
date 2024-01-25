using UnityEngine;

public class Cracheurdefeu : Unit
{
    private const float deltaTimeTick = .2f;

    [Header("Flame Thrower")]
    [SerializeField]
    private float duration;
    [SerializeField]
    private float angle;
    [SerializeField]
    private float maxDistance;

    [Header("Last Resort")]
    [SerializeField]
    private float s_damageOverTime;
    [SerializeField]
    private float s_duration;
    [SerializeField]
    private float s_radius;

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

        animator.SetBool("Attack", true);
        StartCoroutine(CreateDamageCone(target, unitData.Attack, duration, angle, maxDistance, ignoreDefense:true, damageDeltaTime: deltaTimeTick));

        return true;
    }

    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction(target))
            return false;

        isInvulnerable = true;
        StartCoroutine(CreateDamageSphere(null, s_damageOverTime, s_duration, s_radius));
        Destroy(gameObject, s_duration);

        return true;
    }
}
