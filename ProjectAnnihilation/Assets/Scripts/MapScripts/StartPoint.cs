using UnityEngine;

public class StartPoint : Tile
{
    #region variables
    
    #endregion

    void Start()
    {
        tileType = 4;
    }


    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Unit unit))
        {
            if (unit.IsKing)
                Debug.Log("Won ");
        }
    }
}
