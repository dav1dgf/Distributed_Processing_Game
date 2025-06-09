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

    public void TakeDamage(float damage)
    {
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
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(FloorController.position, boxDimensions);
    }
}


/*
import socket
import threading

PORT = 12345
MAX_CLIENTS = 2
client_sockets = []
lock = threading.Lock()

def handle_client(sock, client_id):
    while True:
        try:
            data = sock.recv(1024)
            if not data:
                break

            with lock:
                # Send the message to the other client
                for i, other_sock in enumerate(client_sockets):
                    if other_sock != sock:
                        try:
                            other_sock.sendall(data)
                        except:
                            pass
        except:
            break

    print(f"Client {client_id} disconnected.")
    sock.close()

def main():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('', PORT))
    server_socket.listen(MAX_CLIENTS)
    print(f"Server listening on port {PORT}")

    while len(client_sockets) < MAX_CLIENTS:
        client_sock, addr = server_socket.accept()
        with lock:
            client_sockets.append(client_sock)
        print(f"Player {len(client_sockets)} connected from {addr}")

    # Send START to all clients
    for sock in client_sockets:
        sock.sendall(b'START')

    # Create threads for each client
    for i, sock in enumerate(client_sockets):
        thread = threading.Thread(target=handle_client, args=(sock, i+1), daemon=True)
        thread.start()

    try:
        while True:
            pass  # Keep the server running
    except KeyboardInterrupt:
        print("Server shutting down.")
        for sock in client_sockets:
            sock.close()
        server_socket.close()

if __name__ == '__main__':
    main()

*/