using UnityEngine;

public class Vétérinaire : Unit
{
    private const int MAX_DETECTED_UNITS = 10;

    [Header("Divine Area")]
    [SerializeField]
    private float radius;
    [SerializeField]
    private float duration;


    #region Debug Gizmoz

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

#endif

    #endregion

    protected override bool Action(GameObject target = null)
    {
        if (!base.Action())
            return false;


        if (target == null)
            return false;

        Heal(target, unitData.Attack);

        return true;
    }

    protected override bool SpecialAction(GameObject target = null)
    {
        if (!base.SpecialAction())
            return false;

        DivineSphere();

        return true;
    }

    private void DivineSphere()
    {
        // DivineSphere gives invulnerability (hp not going below 1) bonuses to units located in sphere
        // It achieves this by scanning every INVULNERABILITY_DELTA_TIME seconds the close by units to give the a bonus that lasts INVULNERABILITY_DELTA_TIME
        // This is to avoid calling Physics Overlap and TryGetComponent() at every frame

        StartCoroutine(CreatePowerUpSphere(1, radius, duration, PowerUpType.Invulnerability));
    }
}
