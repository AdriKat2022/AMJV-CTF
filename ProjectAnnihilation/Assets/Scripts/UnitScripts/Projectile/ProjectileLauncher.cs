using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private ProjectileBehaviour projectile;
    [SerializeField]
    private float gravityConstant = 9.8f;

    [SerializeField]
    private float alpha = 45f; // I put 70 in the prefab (it looks better)
    [SerializeField]
    private float timeBeforeCrash; // Best practice: put everything in private, and switch to public only if necessary ;)


    private Unit unit;
    private float alphaRadians;

    #endregion


    #region Helper Functions

    //public static Vector3 GetDirectionFrom()
    //{

    //}

    #endregion


    private void Start()
    {
        if (!TryGetComponent(out unit))
        {
            Debug.Log("A Projectile launcher needs a unit to function properly");
            return;
        }

        alphaRadians = alpha * 2 * Mathf.PI / 360;
    }

    public void LaunchRect(Vector3 initialVelocity, DamageData dmgData, float timespan)
    {
        ProjectileBehaviour proj = Instantiate(projectile, transform.position + offset, transform.rotation);
        proj.SetDamage(dmgData);
        proj.SetVelocity(initialVelocity);
        proj.SetTeam(unit.IsAttacker);
        proj.SetGravityScale(0);
        proj.SetTimespan(timespan);
    }
    public void LaunchArc(Vector3 hitpoint, DamageData dmgData)
    {
        //Permet de prévoir un éventuel décalage pour les visuels
        Vector3 projectilePos = gameObject.transform.position + new Vector3(0f, 0f, 0f);
        float distance = Vector3.Distance(projectilePos, hitpoint);
        Vector3 direction = Vector3.Normalize(hitpoint - projectilePos);
        float initialSpeed = Mathf.Sqrt((distance * gravityConstant) / Mathf.Sin(2 * alphaRadians));

        float initialXSpeed = initialSpeed*Mathf.Cos(alphaRadians);
        float initialYSpeed = initialSpeed*Mathf.Sin(alphaRadians);

        timeBeforeCrash = distance / initialXSpeed;

        // GameObject clone = Instantiate(projectile,projectilePos, Quaternion.Euler(transform.forward * initialXSpeed + transform.up * initialYSpeed));
        // Appliquer la vitesse initiale à l'objet
        ProjectileBehaviour projectileInstance = Instantiate(projectile, projectilePos, Quaternion.identity);
        projectileInstance.SetDamage(dmgData);
        projectileInstance.SetTeam(unit.IsAttacker);
        projectileInstance.SetVelocity(direction * initialXSpeed + Vector3.up * initialYSpeed);
        projectileInstance.SetTimespan(10);
    }
}
