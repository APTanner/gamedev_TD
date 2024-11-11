using UnityEngine;
using UnityEngine.VFX;

public class Bullet : MonoBehaviour
{
    public float speed = 100f;
    public float damage = 1f;
    public float lifetime = 5f;
    public float ExplosionRadius = 0f;
    public float ExplosionDamage = 0f;

    public VisualEffect explosionEffectPrefab;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ExplosionRadius > 0)
        {
            foreach (var collider in Physics.OverlapSphere(transform.position, ExplosionRadius, 1 << Defines.SwarmerLayer))
            {
                //Debug.Log("Hit Guy");
                if (collider.gameObject.TryGetComponent<SwarmerController>(out var balls))
                {
                    balls.TakeDamage(ExplosionDamage);
                }
            };
        }

        if (collision.gameObject.TryGetComponent<SwarmerController>(out var enemy))
        {
            enemy.TakeDamage(damage);
        }

        if (explosionEffectPrefab != null)
        {
            VisualEffect explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            explosion.Play(); 

            Destroy(explosion.gameObject, explosion.GetFloat("Duration")); 
        }

        Destroy(gameObject);
    }
}
