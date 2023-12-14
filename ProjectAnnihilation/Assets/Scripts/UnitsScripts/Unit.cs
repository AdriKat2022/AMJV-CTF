using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class Unit : MonoBehaviour, ISelectable
{

    [SerializeField]
    private UnitData unitData;

    private GameManager gameManager;

    public bool IsAttacker => isAttacker;
    private bool isAttacker; // Defines the team of the unit


    private float hp;
    private float speedBonus;

    private bool isInvincible = false;
    private bool isInvulnerable = false;

    public bool IsSelected => isSelected;
    private bool isSelected;

    public virtual void Action()
    {
        // To override
    }
    public virtual void SpecialAttack()
    {
        // To override
    }


    private enum UnitState
    {
        DEFAULT,
        MOVING,
        ATTACKING,
        FOLLOWING,
        PATROLLING
    }

    private void OnValidate()
    {
        //gameManager = GameManager
    }

    private void Start()
    {
        isSelected = false;
    }

    private void Update()
    {
        
    }


    #region Health Related
    public void Damage(float damage)
    {
        if(!isInvincible)
            hp -= damage;
        else
        {
            // Do some fancy block effect or not
        }

        if(hp <= 0)
            KnockedDown();
    }

    public void Heal(float heal)
    {
        hp += heal;
        hp = Mathf.Clamp(hp, 0, unitData.maxHp);
    }

    private void KnockedDown()
    {
        if (!isInvulnerable) {
            Destroy(gameObject);
            // Or launch a fancy coroutine to show it died idk
        }
    }

    #endregion Health Related




    public void Select()
    {
        // TODO : Check if player is in the same team than this unit
        isSelected = true;
        // Unit is selected (with pointer mouse)
    }

    public void Deselect()
    {
        isSelected = false;
    }



    private void SetDestination()
    {
        
    }
}
