using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField]
    private DamageNumber damageNumber;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private float posVariance;

    public void SpawnDamageNumber(float number)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-posVariance, posVariance), 0, Random.Range(-posVariance, posVariance));
        DamageNumber dn = Instantiate(damageNumber, transform.position + offset + randomOffset, transform.rotation);
        dn.Initialize(number);
    }
}
