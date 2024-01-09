using UnityEngine;

public struct DamageData
{
    public float damage;
    public Vector3? knockback; // Can be null
    public float hitStun;

    public DamageData(float damage, Vector3? knockback = null, float hitStun = 0f)
    {
        this.damage = damage;
        this.knockback = knockback;
        this.hitStun = hitStun;
    }
}

public interface IDamageable
{
    public void Damage(DamageData damageData, IDamageable from = null);

    public void Heal(float heal);
}
