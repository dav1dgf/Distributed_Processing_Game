using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    // Movement and Attack variables
    public Weapon weapon;
    public InputActionAsset controls;
    public float moveDistance = 1f;
    public float movementVelocity = 5f;
    public float jumpPower = 10f;
    public bool lookLeft;
    public float maxHealth = 100f;
    private float currentHealth;
    public HealthBar healthBar;

    // Arena boundaries and enemy robot
    public float arenaLeftLimit = -5f;
    public float arenaRightLimit = 5f;
    public Transform enemyRobot;
    //public RobotController enemyController;

    // Physics and Collision
    public Rigidbody2D rb2D;
    public LayerMask isFloor;
    public Transform FloorController;
    public Vector3 boxDimensions;

    private Vector2 direction;
    private bool inFloor;
    private InputActionMap movementActions;

    private void Awake()
    {
        // Enable the controls based on the assigned controls input
        if (controls != null)
        {
            movementActions = controls.FindActionMap("Movement");
        }
        //if (lookLeft) ChangeDirection();
    }

    private void OnEnable()
    {
        movementActions.Enable();
        movementActions["Jump"].started += _ => Jump();
        movementActions["Attack"].started += _ => Attack();
    }

    private void OnDisable()
    {
        movementActions.Disable();
        movementActions["Jump"].started -= _ => Jump();
        movementActions["Attack"].started -= _ => Attack();
    }

    private void Update()
    {
        direction = movementActions["Move"].ReadValue<Vector2>();
        AdjustRotation(direction.x);

        // Check if the robot is on the floor using an OverlapBox
        inFloor = Physics2D.OverlapBox(FloorController.position, boxDimensions, 0f, isFloor);
    }

    private void FixedUpdate()
    {
        rb2D.linearVelocity = new Vector2(direction.x * movementVelocity, rb2D.linearVelocity.y);
    }




    public void AdjustRotation(float directionX)
    {
        if (directionX > 0 && !lookLeft)
        {
            ChangeDirection();
        }
        else if (directionX < 0 && lookLeft)
        {
            ChangeDirection();
        }
    }

    public void ChangeDirection()
    {
        lookLeft = !lookLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Jump()
    {
        if (inFloor)
        {
            rb2D.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
        }
    }

    private void Attack()
    {
        if (weapon != null)
        {
            Debug.Log("Bullet fired!");
            weapon.Fire();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = healthBar.getHealth();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(FloorController.position, boxDimensions);
    }
}
