using System;
public enum PowerUpType
{
    SpeedBoost,
    AttackBoost,
    DefenseBoost,

    Invincibility,
    Invulnerability,
    Invisibility,

    Stun
}


[Serializable]
public class StatusEffect<Ref> where Ref : class
{
    public PowerUpType type;
    public float value;
    public float duration;
    public bool isMultiplier;
    public Ref reference;

    /// <summary>
    /// Build your power up with this constructor.
    /// </summary>
    /// <param name="powerUpType">The type of power up you want to give.</param>
    /// <param name="value">The influence of the power up (generally ignored if the type is invulnerable or invincible, since this makes no sense for those bonuses).</param>
    /// <param name="duration">How long is the power up gonna last.</param>
    /// <param name="isMultiplier">False: the parameter will be ADDED. True: the parameter will be a MULTIPLIER.</param>
    /// <param name="reference">Put null for single use. This is to ensure the same referenced power up is not applied twice to the same entity.</param>
    public StatusEffect(PowerUpType powerUpType, float value, float duration, bool isMultiplier, Ref reference = null)
    {
        this.type = powerUpType;
        this.value = value;
        this.duration = duration;
        this.isMultiplier = isMultiplier;
        this.reference = reference;
    }
}