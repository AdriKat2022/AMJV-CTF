using UnityEngine;

public class StartPoint : Tile
{
    #region variables
    
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        tileType = 4;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Unit>().IsKing == true)
        {
            Debug.Log("You win");
        }
    }
}
