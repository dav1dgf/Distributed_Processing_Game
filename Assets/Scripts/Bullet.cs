using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    Weapon weapon;
    private float direction = 1;
    Rigidbody2D rb;

    

    public void setWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public void setDirection(float dir)
    {
        direction = dir;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(dir * speed, 0f);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;  // <-- IMPORTANTE: marcar como trigger para que no empuje

        rb.linearVelocity = new Vector2(direction * speed, 0f);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bala impactó con: {other.gameObject.name}");

        if (other.CompareTag("FloorGun"))
        {
            Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            var robot = other.GetComponent<RobotController>();
            if (robot != null)
                robot.TakeDamage(weapon.damage);
            Destroy(gameObject);
        }
    }
}
