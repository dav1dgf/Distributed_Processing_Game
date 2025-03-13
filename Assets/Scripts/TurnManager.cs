using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GameObject Robot1;
    public GameObject Robot2;

    public bool isPlayerTurn = true;
    private int movesLeft = 3;  // Max 3 moves per turn

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))  // Press Space to switch turns
        {
            EndTurn();
        }
    }

    public bool CanMakeMove()
    {
        return movesLeft > 0;
    }

    public void UseMove()
    {
        if (movesLeft > 0)
        {
            movesLeft--;
            Debug.Log("Move used! Moves left: " + movesLeft);

        }

        if (movesLeft == 0)
        {
            Debug.Log("No more moves left! Press Space to end turn.");
        }
    }

    void EndTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        movesLeft = 3;  // Reset moves for new turn
        Debug.Log("Turn switched! Player turn: " + isPlayerTurn);
    }
}
