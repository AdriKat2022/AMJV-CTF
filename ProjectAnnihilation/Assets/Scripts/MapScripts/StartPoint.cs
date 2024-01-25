using UnityEngine;

public class StartPoint : Tile
{
    void Start()
    {
        tileType = 4;
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
