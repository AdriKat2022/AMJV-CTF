using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region variables
    [SerializeField] private GameObject projectile;
    [SerializeField] private float gravite = -9.8f;
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float highMax = 5f;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Launch(Vector3 hitpoint)
    {
        // Calculer la distance horizontale entre l'objet et la cible
        Vector3 projectilePos = gameObject.transform.position + new Vector3(2f, 2f, 0);
        float distanceHorizontale = Vector3.Distance(projectilePos, hitpoint);

        // Calculer la hauteur initiale en utilisant l'équation de mouvement en cloche
        float hauteurInitiale = transform.position.y + (distanceHorizontale * distanceHorizontale * gravite) / (2 * launchSpeed * launchSpeed);

        // Calculer l'angle de lancer
        float angleLancer = Mathf.Atan((highMax - hauteurInitiale) / distanceHorizontale);

        // Calculer les composantes de la vitesse initiale en x et z
        float vitesseInitialeX = launchSpeed * Mathf.Cos(angleLancer);
        float vitesseInitialeZ = launchSpeed * Mathf.Sin(angleLancer);

        // Appliquer la vitesse initiale à l'objet
        GameObject clone = Instantiate(projectile,projectilePos, Quaternion.identity);
        Rigidbody rb = clone.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(vitesseInitialeX, 0f, vitesseInitialeZ);
    }
}
