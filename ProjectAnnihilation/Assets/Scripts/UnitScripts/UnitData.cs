using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/New UnitData", order = 1)]
public class UnitData : ScriptableObject
{
    [Header("Stats")]
    public float maxHP;
    public float speed;
    public float attack;
    public float armor;
    public float attackRange;

    [Header("Attacks and targets")]

    public PossibleTargets teamTarget;
    public PossibleTargets specialTeamTargets;
    public bool canSelfAttack;

    [Header("End lag")]
    public float attackEndLag;
    public float specialAttackEndLag;

    [Header("Colors")]
    public Color selectedColor = Color.red;
    public bool blinkOnSelected = true;
    public bool useFullBlink = true;
    public float blinkSpeed = 0.5f;
    public Color nothingColor = Color.black;
    public Color idleColor = Color.gray;
    public Color movingColor = Color.blue;
    public Color movingFocusedColor = Color.yellow;
    public Color chaseColor = Color.white;


    [Header("Others")]
    public LayerMask unitLayer;
}