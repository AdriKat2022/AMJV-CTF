using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/New UnitData", order = 1)]
public class UnitData : ScriptableObject
{
    [Header("Stats")]
    public float maxHp;
    public float speed;
    public float armor;

    [Header("Attacks and targets")]

    public float attackRange;
    public PossibleTargets teamTarget;
    public PossibleTargets specialTeamTargets;

    [Header("End lag")]
    public float attackEndLag;
    public float specialAttackEndLag;
}