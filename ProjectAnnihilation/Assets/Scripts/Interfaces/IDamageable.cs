using System.Collections;
using UnityEngine;

public struct DamageData
{
    public Vector3? knockback; // Can be null
    public float damage;
    public float hitStun;

    public DamageData(float damage,  float hitStun = 0f, Vector3? knockback = null)
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
