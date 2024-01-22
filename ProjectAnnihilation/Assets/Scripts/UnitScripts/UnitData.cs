using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/New UnitData", order = 1)]
public class UnitData : ScriptableObject
{
    [field: SerializeField] public string UnitName { get; private set; }

    [field: Header("Stats")]
    [field: SerializeField] public float MaxHP { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float Attack { get; private set; }
    [field: SerializeField] public float Armor { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }

    [field : Header("Standard attack")]
    [field: SerializeField] public PossibleTargets TeamTarget { get; private set; }
    [field: SerializeField] public bool CanSelfAttack { get; private set; }
    [field: SerializeField] public float AttackEndLag { get; private set; }
    [field: SerializeField] public float AttackRechargeTime { get; private set; }

    [field: Header("Special attack")]
    [field: SerializeField] public string SpecialAttackName { get; private set; }
    [field: SerializeField] public PossibleTargets SpecialTeamTargets { get; private set; }
    [field: SerializeField] public bool IsSpecialAttackPassive { get; private set; }
    [field: SerializeField] public float SpecialAttackEndLag { get; private set; }
    [field: SerializeField] public float SpecialAttackRechargeTime { get; private set; }


    [field: Header("Colors")]
    [field: SerializeField] public Color SelectedColor { get; private set; } = Color.red;
    [field: SerializeField] public bool BlinkOnSelected { get; private set; } = true;
    [field: SerializeField] public bool UseFullBlink { get; private set; } = true;
    [field: SerializeField] public float BlinkSpeed { get; private set; } = 0.5f;
    [field: SerializeField] public Color NothingColor { get; private set; } = Color.black;
    [field: SerializeField] public Color IdleColor { get; private set; } = Color.gray;
    [field: SerializeField] public Color MovingColor { get; private set; } = Color.blue;
    [field: SerializeField] public Color MovingFocusedColor { get; private set; } = Color.yellow;
    [field: SerializeField] public Color ChaseColor { get; private set; } = Color.white;


    [field: Header("Others")]
    [field: SerializeField] public LayerMask UnitLayer { get; private set; }

    [field: Header("Meta State")]
    [field: SerializeField] public UnitData OtherStateUnitData { get; private set; }
}