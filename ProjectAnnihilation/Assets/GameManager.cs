using System.Collections;
using System.Collections.Generic;
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



    void Start()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogWarning("Cannot have multiple instances of GameManager !");
            Destroy(this);
        }
    }


    void Update()
    {
        
    }
}
