using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    //Useful variables 
    public int damageDone;
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
    }

    private void OnTriggerEnter(Collider other)
    {   
        if (other.TryGetComponent<HealthModule>( out HealthModule hM))
        {
            if (isAttacker != other.gameObject.GetComponent<Unit>().IsAttacker)
            {
                hM.takeDamage(damageDone);
            }
        }
    }
}
