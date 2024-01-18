using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public bool PlayerIsAttacker => playerIsAttacker;
    private bool playerIsAttacker;
    public UnityEvent onEnemyDeath;
    public UnityEvent onFinalMoove;
    static public GameManager Instance {
        get {
            return instance;
        }
    }
    static private GameManager instance;

    [Header("Game settings")]
    public float timeBeforeTargettingUnit;


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

    public void TriggerDeath()
    {
        onEnemyDeath.Invoke();
    }

    public void TriggerFinalMoove()
    {
        onFinalMoove.Invoke();
    }
}
