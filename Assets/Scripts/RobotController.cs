using UnityEngine;
using UnityEngine.InputSystem;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class RobotController : MonoBehaviour
{
    // Movement and Attack variables
    public Weapon weapon;
    public InputActionAsset controls;
    public float moveDistance = 1f;
    public float movementVelocity = 5f;
    public float jumpPower = 10f;
    public bool facingRight;
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

    private TcpClient client;
    private NetworkStream stream;
    private bool gameStarted = false;

    public Vector3 startPositionPlayer;
    public bool isBlueRobot;
    
     
      private async void Start()
    {
        ConnectToServer();
        await ListenForServerMessages();
    }

    void ConnectToServer()
    {
        client = new TcpClient("127.0.0.1", 12345);
        stream = client.GetStream();
    }

    private async Task ListenForServerMessages()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int length = await stream.ReadAsync(buffer, 0, buffer.Length);
            string msg = Encoding.ASCII.GetString(buffer, 0, length);
            if (msg == "START")
            {
                gameStarted = true;
                transform.position = startPositionPlayer;
            }
            else
            {
                Debug.Log("Server message: " + msg);
            }
        }
    }
    private void SendToServer(string msg)
    {
        byte[] data = Encoding.ASCII.GetBytes(msg);
        stream.Write(data, 0, data.Length);
    }
     
     
    private void Awake()
    {
        // Enable the controls based on the assigned controls input
        if (controls != null)
        {
            movementActions = controls.FindActionMap("Movement");
        }
        if (isBlueRobot) facingRight = true; else facingRight = false;


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
        if (directionX > 0 && !facingRight)
        {
            Flip();
        }
        else if (directionX < 0 && facingRight)
        {
            Flip();
        }
    }

    public void Flip()
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