using UnityEngine;
using UnityEngine.InputSystem;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;

public class RobotController : MonoBehaviour
{
    // Movement and Attack variables
    public Weapon weapon;
    public InputActionAsset controls;
    public float moveDistance = 1f;
    public float movementVelocity = 2f;
    public float jumpPower = 5f;
    [SerializeField] private bool facingRight;
    private float maxHealth = 100f;
    public float currentHealth = 100f;
    public HealthBar healthBar;

    // Arena boundaries and enemy robot
    public float arenaLeftLimit = -5f;
    public float arenaRightLimit = 5f;
    public Transform enemyRobot;

    // Physics and Collision
    public Rigidbody2D rb2D;
    public LayerMask isFloor;
    public Transform FloorController;
    public Vector3 boxDimensions;

    private Vector2 direction;
    private bool inFloor;
    private InputActionMap movementActions;
    //public GameObject GameManager;
    public TurnManager turnManager;

    // Networking (como en clase)
 
    private bool gameStarted = false;
    public Vector3 startPositionPlayer;

    public void StartGame()
    {
        gameStarted = true;

        // Reset health
        currentHealth = maxHealth;
        healthBar.UpdateHealth(currentHealth);

        // Reset position, scale, and rotation
        transform.position = startPositionPlayer;
        transform.localScale = new Vector3(2f, 2f, 1f);
        // Optionally reset any velocity if using Rigidbody
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log("Game started. Player reset.");
    }

    private void Awake()
    {
        if (controls != null)
            movementActions = controls.FindActionMap("Movement");
    }

    private void OnEnable()
    {
        movementActions.Enable();
        movementActions["Jump"].started += _ => Jump();
        movementActions["Attack"].started += _ => Attack();
    }

    private void OnDisable()
    {
        movementActions["Jump"].started -= _ => Jump();
        movementActions["Attack"].started -= _ => Attack();
        movementActions.Disable();
    }

    private void Update()
    {
        

        // 2) El c�digo de lectura de input y OverlapBox� (falta por hacer aun)
        direction = movementActions["Move"].ReadValue<Vector2>();
        direction = movementActions["Move"].ReadValue<Vector2>();
        AdjustRotation(direction.x);

        // Env�a al servidor el movimiento si hay input
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            string dir = direction.x > 0 ? "MOVE RIGHT" : "MOVE LEFT";
            //SendToServer(dir);
        }

        // Comprueba si el robot est� en el suelo
        inFloor = Physics2D.OverlapBox(FloorController.position, boxDimensions, 0f, isFloor);
    }

    private void FixedUpdate()
    {
        rb2D.linearVelocity = new Vector2(direction.x * movementVelocity, rb2D.linearVelocity.y);
    }

    private void AdjustRotation(float xDirection)
    {
        if (xDirection > 0 && !facingRight)
        {
            Flip();
        }
        else if (xDirection < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Jump()
    {
        if (inFloor)
        {
            rb2D.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
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

    private bool isDead = false;

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Prevent processing if already dead

        currentHealth = healthBar.getHealth() - damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBar.UpdateHealth(currentHealth);

        Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth}");

        if (currentHealth <= 0 && gameStarted)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return; // Redundant guard, extra safety
        isDead = true;

        Debug.Log($"{gameObject.name} has been destroyed!");
        turnManager.GameEnd(gameObject.name);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Spike"))
        {
            Debug.Log($"{gameObject.name} has been destroyed!");
            healthBar.UpdateHealth(0);

            turnManager.GameEnd(gameObject.name);
            this.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(FloorController.position, boxDimensions);
    }
}
