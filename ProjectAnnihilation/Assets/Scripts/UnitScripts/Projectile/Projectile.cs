using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region variables
    public bool isAttacker;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float g = 9.8f;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        isAttacker = gameObject.GetComponent<Unit>().IsAttacker;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Launch(Vector3 hitpoint, int damageDone = 0)
    {
        Vector3 projectilePos = gameObject.transform.position + new Vector3(0, 0, 0);
        float distance = Vector3.Distance(projectilePos, hitpoint);
        //Projectile will be shot at 45°

        float alpha = 45f*2*Mathf.PI/360;

        float initialXSpeed = Mathf.Sqrt((distance * g) / Mathf.Sin(2 * alpha))*Mathf.Cos(alpha);
        float initialYSpeed = Mathf.Sqrt((distance*g)/ Mathf.Sin(2 * alpha))*Mathf.Sin(alpha);


        // Appliquer la vitesse initiale à l'objet
        GameObject clone = Instantiate(projectile,projectilePos, Quaternion.identity);
        ProjectileManager pM = clone.GetComponent<ProjectileManager>();
        pM.damageDone = damageDone;
        pM.isAttacker = isAttacker;
        Rigidbody rb = clone.GetComponent<Rigidbody>();
        rb.velocity = transform.forward * initialXSpeed + transform.up * initialYSpeed;
    }


}
