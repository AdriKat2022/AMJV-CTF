using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    //Useful variables 
    public float damageDone;
    public bool isAttacker;
    private float angularVelocity;
    // Start is called before the first frame update
    void Start()
    {
        angularVelocity = gameObject.GetComponent<Projectile>().alpha / gameObject.GetComponent<Projectile>().timeBeforeCrash;

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= 0)
        {

            Destroy(gameObject);
        }
        gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, angularVelocity, 0f);
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
