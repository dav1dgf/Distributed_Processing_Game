using UnityEngine;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    public TurnManager turnManager;
    public Button moveForwardButton;
    public Button moveBackwardButton;
    public Button attackButton;

    private RobotController activeRobot;

    void Start()
    {
        // Assign button functions
        moveForwardButton.onClick.AddListener(MoveForward);
        moveBackwardButton.onClick.AddListener(MoveBackward);
        attackButton.onClick.AddListener(Attack);
    }

    void Update()
    {
        // Determine which robot's turn it is
        activeRobot = (turnManager.isPlayerTurn)
            ? turnManager.Robot1.GetComponent<RobotController>()
            : turnManager.Robot2.GetComponent<RobotController>();

        /*
        bool isActive = (activeRobot != null);
        moveForwardButton.interactable = isActive;
        moveBackwardButton.interactable = isActive;
        attackButton.interactable = isActive;
        */
    }

    void MoveForward()
    {
        if (activeRobot != null) 
            if (turnManager.CanMakeMove()) {
                activeRobot.Move(Vector2.right);
            }
        turnManager.UseMove();
                
    }

    void MoveBackward()
    {
        if (activeRobot != null) 
            if (turnManager.CanMakeMove())
            {
                activeRobot.Move(Vector2.left);
            }
        turnManager.UseMove();
    }

    void Attack()
    {
        if (activeRobot != null) 
            if (turnManager.CanMakeMove())
            {
                activeRobot.Attack();
            }
        turnManager.UseMove();
    }
}
