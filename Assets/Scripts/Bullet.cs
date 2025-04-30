using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // Bullet speed
    public float lifetime = 3f; // How long before the bullet disappears
    Weapon weapon;
    void Start()
    {
        // Destroy bullet after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the bullet forward
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    public void setWeapon(Weapon weapon) { 
     this.weapon = weapon;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // If the bullet hits a robot (or any other object), destroy the bullet
        if (collision.gameObject.CompareTag("Player"))
        {
            // Do damage, or trigger an event (like destroy the robot)

            RobotController robotScript = collision.gameObject.GetComponent<RobotController>();
            if (robotScript != null)
            {
                robotScript.TakeDamage(weapon.damage);  // Assuming Bullet script has a method to receive the weapon reference
            }
            Destroy(gameObject);  // Destroy the bullet after collision
        }
    }
}
