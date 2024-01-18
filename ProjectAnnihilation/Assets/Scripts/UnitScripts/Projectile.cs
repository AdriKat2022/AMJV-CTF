using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region variables
    [SerializeField] private GameObject projectile; 
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Launch(Vector3 hitpoint, Vector3 initialSpeed)
    {
        GameObject clone = Instantiate(gameObject, transform.position, transform.rotation);

        clone.GetComponent<Rigidbody>().AddRelativeForce(initialSpeed);

    }
}
