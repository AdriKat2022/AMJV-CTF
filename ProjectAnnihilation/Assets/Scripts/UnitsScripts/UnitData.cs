using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/New UnitData", order = 1)]
public class UnitData : ScriptableObject
{
    public float maxHp;
    public float speed;
    public float armor;
}
