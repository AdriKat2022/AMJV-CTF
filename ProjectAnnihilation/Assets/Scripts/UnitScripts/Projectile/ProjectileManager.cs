using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    //Useful variables 
    public float damageDone;
    public bool isAttacker;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= 0)
        {
            Destroy(gameObject);
        }
        transform.rotation = Quaternion.LookRotation(gameObject.GetComponent<Rigidbody>().velocity);
    }

    private void OnTriggerEnter(Collider other)
    {   
        if (other.TryGetComponent<IDamageable>( out IDamageable damage))
        {
            if (isAttacker != other.gameObject.GetComponent<Unit>().IsAttacker)
            {
                DamageData dmgData = new(damageDone);
                damage.Damage(dmgData);
            }
        }
    }
}
