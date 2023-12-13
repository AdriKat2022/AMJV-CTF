using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*public enum AttackType
{
    melee,
    distance,
    knockback,
    trap,
    build
}*/


public abstract class Unit : MonoBehaviour
{
    [SerializeField]
    private float maxHp;
    [SerializeField]
    private float speed;
    [SerializeField]
    private float armor;

    public bool IsAttacker => isAttacker;
    private bool isAttacker; // Defines the team of the unit

    private float hp;
    private float speedBonus;

    private bool isInvincible = false;
    private bool isInvulnerable = false;


    /*[SerializeField]
    private AttackType attackType;*/

    public abstract void Action();
    public abstract void SpecialAttack();


    public void Damage(float damage)
    {
        if(!isInvincible)
            hp -= damage;
        else
        {
            // Do some fancy block effect
        }

        if(hp <= 0)
            KnockedDown();
    }

    public void Heal(float heal)
    {
        hp += heal;
        hp = Mathf.Clamp(hp, 0, maxHp);
    }

    private void KnockedDown()
    {
        if (!isInvulnerable) {
            Destroy(gameObject);
            // Or launch a fancy coroutine to show it died idk
        }
    }

    private void MoveUnit()
    {
        
    }
}
