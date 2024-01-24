using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField]
    private DamageNumber damageNumber;

    public void SpawnDamageNumber(float number)
    {
        DamageNumber dn = Instantiate(damageNumber, transform.position, transform.rotation);
        dn.Initialize(number);
    }
}
