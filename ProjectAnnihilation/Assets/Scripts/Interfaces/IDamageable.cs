using System;
using UnityEngine;

[Serializable]
public struct DamageData
{
    public Vector3? knockback; // Can be null
    public float damage;
    public float hitStun;
    public bool ignoreDefense;

    public DamageData(float damage,  float hitStun = 0f, Vector3? knockback = null, bool ignoreDefense = false)
    {
        this.damage = damage;
        this.knockback = knockback;
        this.hitStun = hitStun;
        this.ignoreDefense = ignoreDefense;
    }
}

public interface IDamageable
{
    public void Damage(DamageData damageData, IDamageable from = null);

    public void Heal(float heal);
}
