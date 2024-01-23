using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [Header("Default values")]
    [SerializeField]
    private DamageData damageData;
    [SerializeField]
    private float gravityScale = 9.8f;

    private float timespan = 1f;
    private bool isAttacker;

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
        CheckTimespan();

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

    private void CheckTimespan()
    {
        if (timespan <= 0)
            Destroy(gameObject);

        timespan -= Time.deltaTime;
    }

    #region Setter functions
    public void SetDamage(DamageData damage) => damageData = damage;
    public void SetTeam(bool isAttacker) => this.isAttacker = isAttacker;
    public void SetVelocity(Vector3 vel) => rb.velocity = vel;
    public void SetGravityScale(float gravityScale) => this.gravityScale = gravityScale;
    public void SetUpVector(Vector3 upVector) => this.upVector = upVector;
    public void SetTimespan(float timespan) => this.timespan = timespan;

    #endregion

    private void OnTriggerEnter(Collider other)
    {   
        if (other.TryGetComponent(out IDamageable damage))
        {
            if (isAttacker != other.gameObject.GetComponent<Unit>().IsAttacker)
            {
                damage.Damage(damageData);
                Destroy(gameObject);
            }
        }
    }
}
