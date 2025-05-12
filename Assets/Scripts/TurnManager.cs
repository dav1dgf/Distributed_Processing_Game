using UnityEngine;
using System.Collections;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public GameObject Robot1;
    public GameObject Robot2;
    public bool isBlueTurn = true;
    public TMP_Text text;

    private void Start()
    {
        StartCoroutine(HandleTurn());
    }

    IEnumerator HandleTurn()
    {
        while (true)
        {
            string currentRobot = isBlueTurn ? "Robot1" : "Robot2";
            Debug.Log($"{currentRobot}'s turn!");
            text.text = currentRobot + " turn!";

            // Enable movement for the current player, disable for the other
            Robot1.GetComponent<RobotController>().enabled = isBlueTurn;
            Robot2.GetComponent<RobotController>().enabled = !isBlueTurn;

            // Stop movement of the robot not in turn
            Rigidbody2D rb1 = Robot1.GetComponent<Rigidbody2D>();
            Rigidbody2D rb2 = Robot2.GetComponent<Rigidbody2D>();

            if (!isBlueTurn && rb1 != null)
            {
                rb1.linearVelocity = Vector2.zero;
                rb1.gravityScale = 0f;
                rb2.gravityScale = 1f;



            }
            else if (isBlueTurn && rb2 != null)
            {
                rb2.linearVelocity = Vector2.zero;
                rb2.gravityScale = 0f;
                rb1.gravityScale = 1f;
            }

            //rb2.gravityScale = isBlueTurn ? 0f : 1f;



            // Wait 3 seconds during this turn
            yield return new WaitForSeconds(4f);

            EndTurn();
        }
    }

    void checkGameEnd()
    {
        // Check for victory
        
    }

    void EndTurn()
    {
        isBlueTurn = !isBlueTurn;
    }
}
