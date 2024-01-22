using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region Variables
    [SerializeField] private GameObject projectile;
    [SerializeField] private float gravityConstant = 9.8f;

    // Didn't do it but free tip : you can reference a component instead of "GameObject" so upon instantiating, you save up one GetComponent call. Neat uh ?
    //
    // [SerializeField] private ProjectileManager projectile;
    // ...
    // ProjectileManager clone = Instantiate(projectile, ...);

    [SerializeField] private float alpha = 45f; // I put 70 in the prefab (it looks better)
    [SerializeField] private float timeBeforeCrash; // Best practice: put everything in private, and switch to public only if necessary ;)

    private float distance;
    private bool isAttacker; // (Better to keep it private and not show it in the editor)
    #endregion


    void Start()
    {
        isAttacker = gameObject.GetComponent<Unit>().IsAttacker;
        alpha = alpha * 2 * Mathf.PI / 360;
    }

    public void Launch(Vector3 hitpoint, float damageDone = 0)
    {
        //Permet de prévoir un éventuel décalage pour les visuels
        Vector3 projectilePos = gameObject.transform.position + new Vector3(0f, 0f, 0f);
        distance = Vector3.Distance(projectilePos, hitpoint);
        Vector3 direction = Vector3.Normalize(hitpoint - projectilePos);
        float initialSpeed = Mathf.Sqrt((distance * gravityConstant) / Mathf.Sin(2 * alpha));
        timeBeforeCrash = distance / (initialSpeed*Mathf.Cos(alpha));

        float initialXSpeed = initialSpeed*Mathf.Cos(alpha);
        float initialYSpeed = initialSpeed*Mathf.Sin(alpha);


        // Appliquer la vitesse initiale à l'objet
        GameObject clone = Instantiate(projectile,projectilePos, Quaternion.Euler(transform.forward * initialXSpeed + transform.up * initialYSpeed));
        ProjectileManager pM = clone.GetComponent<ProjectileManager>();
        pM.damageDone = damageDone;
        pM.isAttacker = isAttacker;
        Rigidbody rb = clone.GetComponent<Rigidbody>(); // Instead of accessing the rigidbody here, maybe make a function in ProjectileManager ? (reduces getcomponents call to 0 here)
        rb.transform.rotation = Quaternion.LookRotation(direction);
        rb.velocity = rb.transform.forward * initialXSpeed + rb.transform.up * initialYSpeed;
    }
}
