using JetBrains.Annotations;
using System;
using System.Data.SqlTypes;
public enum PowerUpType
{
    SpeedBoost,
    AttackBoost,
    DefenseBoost,

    Invincibility,
    Invulnerability,
    Invisibility
}


[Serializable]
public class PowerUp<Ref> where Ref : class
{
    public PowerUpType type;
    public float value;
    public float duration;
    public bool isMultiplier;
    public bool hasExitCondition;
    public Ref reference;
    public Func<bool> endCondition; // EndCondition is too complicated to put it into a delegate (so it's deprecated)

    /// <summary>
    /// Build your power up with this constructor.
    /// </summary>
    /// <param name="powerUpType">The type of power up you want to give.</param>
    /// <param name="value">The influence of the power up (generally ignored if the type is invulnerable or invincible, since this makes no sense for those bonuses).</param>
    /// <param name="duration">How long is the power up gonna last.</param>
    /// <param name="isMultiplier">False: the parameter will be ADDED. True: the parameter will be a MULTIPLIER.</param>
    /// <param name="reference">Put null for single use. This is to ensure the same referenced power up is not applied twice to the same entity.</param>
    public PowerUp(PowerUpType powerUpType, float value, float duration, bool isMultiplier, Ref reference = null)
    {
        this.type = powerUpType;
        this.value = value;
        this.duration = duration;
        this.isMultiplier = isMultiplier;
        this.reference = reference;
        hasExitCondition = false;
    }

    /// <summary>
    /// Deprecated, use the constructor without the condition
    /// </summary>
    /// <param name="powerUpType">Test<br>Test</br></param>
    public PowerUp(PowerUpType powerUpType, float value, float duration, bool isMultiplier, Unit reference, Func<bool> endCondition)
    {
        this.type = powerUpType;
        this.value = value;
        this.duration = duration;
        this.isMultiplier = isMultiplier;
        this.endCondition = endCondition;
        hasExitCondition = true;
    }
}