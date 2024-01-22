using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [Header("Default values")]
    [SerializeField]
    private float damageDone = 1f;
    [SerializeField]
    private bool isAttacker = false;
    [SerializeField]
    private float gravityScale = 9.8f;

    private Rigidbody rb;
    private Vector3 upVector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        upVector = Vector3.up;
    }

    private void Update()
    {
        if (transform.position.y <= 0)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        LookProjectileRotation();
    }

    private void ApplyGravity()
    {
        rb.AddForce(-upVector * gravityScale);
    }

    private void LookProjectileRotation()
    {
        transform.rotation = Quaternion.LookRotation(rb.velocity);
    }


    #region Setter functions
    public void SetDamage(float damage) => damageDone = damage;
    public void SetTeam(bool isAttacker) => this.isAttacker = isAttacker;
    public void SetVelocity(Vector3 vel) => rb.velocity = vel;
    public void SetGravityScale(float gravityScale) => this.gravityScale = gravityScale;
    public void SetUpVector(Vector3 upVector) => this.upVector = upVector;

    #endregion

    private void OnTriggerEnter(Collider other)
    {   
        if (other.TryGetComponent(out IDamageable damage))
        {
            if (isAttacker != other.gameObject.GetComponent<Unit>().IsAttacker)
            {
                DamageData dmgData = new(damageDone);
                damage.Damage(dmgData);
            }
        }
    }
}
