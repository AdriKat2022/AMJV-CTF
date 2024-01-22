using System;
public enum PowerUpType
{
    SpeedBoost,
    AttackBoost,
    DefenseBoost,

    Invincibility,
    Invulnerability
}


[Serializable]
public class PowerUp
{
    public PowerUpType powerUpType;
    public float value;
    public float duration;

    public bool hasExitCondition;
    public Func<bool> endCondition; // EndCondition is too complicated to put it into a delegate (so it's deprecated)

    /// <summary>
    /// Build your power up with this constructor.
    /// </summary>
    /// <param name="powerUpType">The type of power up you want to give.</param>
    /// <param name="value">The influence of the power up (generally ignored if the type is invulnerable or invincible, since this makes no sense for those bonuses)</param>
    /// <param name="duration">How long is the power up gonna last</param>
    public PowerUp(PowerUpType powerUpType, float value, float duration)
    {
        this.powerUpType = powerUpType;
        this.value = value;
        this.duration = duration;
        hasExitCondition = false;
    }

    /// <summary>
    /// Deprecated, use the constructor without the condition
    /// </summary>
    public PowerUp(PowerUpType powerUpType, float value, float duration, Func<bool> condition)
    {
        this.powerUpType = powerUpType;
        this.value = value;
        this.duration = duration;
        this.endCondition = condition;
        hasExitCondition = true;
    }

}