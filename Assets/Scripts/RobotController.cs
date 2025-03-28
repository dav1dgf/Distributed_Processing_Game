using UnityEngine;

public class RobotController : MonoBehaviour
{
    public float moveDistance = 1f;
    public float arenaLeftLimit = -5f;  // Left boundary
    public float arenaRightLimit = 5f;  // Right boundary
    public Transform enemyRobot;  // The opponent
    public RobotController enemyController;  // Reference to the opponent's script
    public int numActions = 0;
    public float maxHealth = 100f;
    private float currentHealth;
    public float attackDamage = 20f;
    public HealthBar healthBar;


    public void Move(Vector2 direction)
    {
        float newPositionX = transform.position.x + (direction.x * moveDistance);

        // Prevent moving out of bounds or colliding with the enemy
        if (newPositionX > arenaRightLimit || newPositionX < arenaLeftLimit || Mathf.Abs(newPositionX - enemyRobot.position.x) < moveDistance)
        {
            Debug.Log(gameObject.name + " cannot move to " + newPositionX);
        }
        else
        {
            transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);
            Debug.Log(gameObject.name + " moved to " + transform.position);
        }
    }

    public void Attack()
    {
        if (enemyController != null)
        {
            Debug.Log(gameObject.name + " Attacks!");
            enemyController.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = healthBar.getHealth();
        Debug.Log("Health b4 damage: " + currentHealth);
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);  // Prevents health from going below 0
        healthBar.UpdateHealth(currentHealth);  // Update UI

        Debug.Log(gameObject.name + " took " + damage + " damage! Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " has been destroyed!");
        gameObject.SetActive(false);  // Disables the robot
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EvaluateCollision(collision.gameObject);
    }

    public void EvaluateCollision(GameObject collision)
    {
        if (collision.CompareTag("Spike"))
        {
            TakeDamage(100);
        }
    }
}
