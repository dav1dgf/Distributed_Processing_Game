using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;    // Bullet speed
    public float lifetime = 3f;  // How long before the bullet disappears
    Weapon weapon;

    Rigidbody2D rb;

    void Start()
    {
        // 1) Tomamos la referencia al Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        // 2) Modo físico continuo (para objetos rápidos)
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // 3) Le damos la velocidad una sola vez
        rb.velocity = transform.right * speed;

        // Mantengo la autodestrucción igual
        Destroy(gameObject, lifetime);
    }

    // Ya no necesitamos Update(), Unity moverá la bala por física:
    // void Update()
    // {
    //     transform.Translate(Vector2.right * speed * Time.deltaTime);
    // }

    public void setWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Primer log: nos dice qué objeto ha colisionado
        Debug.Log($"Bala colisionó con: Name='{collision.gameObject.name}' Tag='{collision.gameObject.tag}' Layer='{LayerMask.LayerToName(collision.gameObject.layer)}'");

        // Ahora tu lógica normal
        if (collision.gameObject.CompareTag("FloorGun"))
        {
            Debug.Log("  → ¡Detectado suelo por tag FloorGun!");
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            var robot = collision.gameObject.GetComponent<RobotController>();
            if (robot != null)
                robot.TakeDamage(weapon.damage);
            Destroy(gameObject);
        }
    }



}
