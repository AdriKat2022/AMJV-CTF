using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public bool PlayerIsAttacker => playerIsAttacker;
    private bool playerIsAttacker;
    public UnityEvent onEnemyDeath;
    public UnityEvent onAllyDeath;
    public UnityEvent onFinalMoove;
    public UnityEvent onDeathOfTheKing;
    static public GameManager Instance {
        get {
            return instance;
        }
    }
    static private GameManager instance;

    //[Header("Game settings")]


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Cannot have multiple instances of GameManager !");
            Destroy(gameObject);
        }
    }

    public void TriggerEnemyDeath()
    {
        onEnemyDeath?.Invoke();
    }
    public void TriggerAllyDeath()
    {
        onAllyDeath?.Invoke();
    }

    public void TriggerFinalMoove()
    {
        onFinalMoove?.Invoke();
    }

    public void TriggerDeathOfTheKing()
    {
        onDeathOfTheKing?.Invoke();
    }
}
