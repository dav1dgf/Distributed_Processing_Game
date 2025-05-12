using UnityEngine;
using System.Collections;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public GameObject Robot1;
    public GameObject Robot2;
    public bool isBlueTurn = true;
    public TMP_Text text;
    [SerializeField] private NetworkManager networkManager;
    private bool gameStarted = false;

    public void Start()
    {

    }
    public void Update()
    {

    }
    public void StartGame()
    {

        Robot1.GetComponent<RobotController>().StartGame();
        Robot2.GetComponent<RobotController>().StartGame();
        GameObject enemyRobot = networkManager.playerId == 0 ? Robot2 : Robot1;
        Vector2 pos1 = Robot1.transform.position;
        Vector2 pos2 = Robot2.transform.position;
        gameStarted = true;
        StartTurn(enemyRobot.transform.position, 100);

        

        // Get health (assuming your RobotController has a public int Health)
        float health1 = Robot1.GetComponent<RobotController>().currentHealth;
        float health2 = Robot2.GetComponent<RobotController>().currentHealth;

        if (networkManager != null)
        {
            if (networkManager.playerId == 0)  // Robot1 is controlled by player 0
                networkManager.SendPlayerInfoToServer(pos1.x, pos1.y, health2);  // Send enemy health (Robot2)
            else
                networkManager.SendPlayerInfoToServer(pos2.x, pos2.y, health1);  // Send enemy health (Robot1)
        }

    }

  

    public void GameEnd(string loser)
    {
        // Stop the turn coroutine
        StopAllCoroutines();

        // Disable both robot controllers
        Robot1.GetComponent<RobotController>().enabled = false;
        Robot2.GetComponent<RobotController>().enabled = false;

        // Show winner text
        string winner = loser == "Red Robot" ? "Blue Robot" : "Red Robot";
        text.text = $"{winner} wins!";
        Debug.Log($"{winner} wins!");
        if (networkManager != null)
        {
            networkManager.EndGame();
        }
        // Optional: Call another method or open a UI to restart or go to menu
    }
    public void StartTurn(Vector2 enemyPos, float health)
    {
        GameObject enemyRobot = networkManager.playerId == 0 ? Robot2 : Robot1;
        if (enemyPos != null && health!= null) { 
            GameObject friendRobot = networkManager.playerId == 0 ? Robot1 : Robot2;
            enemyRobot.transform.position = enemyPos;
            friendRobot.GetComponent<RobotController>().TakeDamage(friendRobot.GetComponent<RobotController>().currentHealth - health);
        }
        string currentRobot = isBlueTurn ? "Robot1" : "Robot2";
        Debug.Log($"{currentRobot}'s turn!");
        text.text = currentRobot + " turn!";

        // Enable movement for the current player, disable for the other
        Robot1.GetComponent<RobotController>().enabled = isBlueTurn;
        Robot2.GetComponent<RobotController>().enabled = !isBlueTurn;

        // Stop movement of the robot not in turn
        Rigidbody2D rb1 = Robot1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = Robot2.GetComponent<Rigidbody2D>();
        //NOT OPTIMAL (CHANGE)


        enemyRobot.GetComponent<RobotController>().enabled = false;

        if (!isBlueTurn && rb1 != null)
        {
            rb1.linearVelocity = Vector2.zero;
            rb1.bodyType = RigidbodyType2D.Kinematic;
            rb2.bodyType = RigidbodyType2D.Dynamic;



        }
        else if (isBlueTurn && rb2 != null)
        {
            rb2.linearVelocity = Vector2.zero;
            rb2.bodyType = RigidbodyType2D.Kinematic;
            rb1.bodyType = RigidbodyType2D.Dynamic;

        }
    }

    public void EndTurn()
    {
        isBlueTurn = !isBlueTurn;

        // Get positions
        Vector2 pos1 = Robot1.transform.position;
        Vector2 pos2 = Robot2.transform.position;

        // Get health (assuming your RobotController has a public int Health)
        float health1 = Robot1.GetComponent<RobotController>().currentHealth;
        float health2 = Robot2.GetComponent<RobotController>().currentHealth;

        Debug.Log($"[EndTurn] Robot1 at {pos1}, Health: {health1}");
        Debug.Log($"[EndTurn] Robot2 at {pos2}, Health: {health2}");

        // Send data to server using NetworkManager
        if (networkManager != null)
        {
            if (networkManager.playerId == 0)  // Robot1 is controlled by player 0
                networkManager.SendPlayerInfoToServer(pos1.x, pos1.y, health2);  // Send enemy health (Robot2)
            else
                networkManager.SendPlayerInfoToServer(pos2.x, pos2.y, health1);  // Send enemy health (Robot1)
        }
        //StartTurn();
    }

}
