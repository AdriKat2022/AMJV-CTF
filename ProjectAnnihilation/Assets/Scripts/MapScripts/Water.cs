using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.AI;

public class Water : Tile
{
    #region variables
    private float speedController;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        tileType = 2;
    }

    // Update is called once per frame
    void Update()
    {

    }
}