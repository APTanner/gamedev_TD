using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 100f;
    public float damage = 1f;
    public float lifetime = 5f; 

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
        if (collision.gameObject.TryGetComponent<SwarmerController>(out var enemy))
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
