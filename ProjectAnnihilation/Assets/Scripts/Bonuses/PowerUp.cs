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

    public PowerUp(PowerUpType powerUpType, float value, float duration)
    {
        this.powerUpType = powerUpType;
        this.value = value;
        this.duration = duration;
        hasExitCondition = false;
    }
    public PowerUp(PowerUpType powerUpType, float value, float duration, Func<bool> condition)
    {
        this.powerUpType = powerUpType;
        this.value = value;
        this.duration = duration;
        this.endCondition = condition;
        hasExitCondition = true;
    }

}