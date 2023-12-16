using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool PlayerIsAttacker => playerIsAttacker;
    private bool playerIsAttacker;

    static public GameManager Instance {
        get {
            return instance;
        }
    }
    static private GameManager instance;

    [Header("Game settings")]
    public float timeBeforeTargettingUnit;


    void Awake()
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
}
