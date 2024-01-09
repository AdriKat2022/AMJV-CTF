using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : Tile
{
    #region variables

    [SerializeField] private bool isFlagAvalaible = true;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        tileType = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject entity  = other.gameObject;

        if (entity != null && entity.GetComponent<Unit>().IsAttacker == true)
        {
            if(isFlagAvalaible == true)
            {
                entity.GetComponent<Unit>().BecomeKing();
                isFlagAvalaible = false;
            }
        }
    }
}
