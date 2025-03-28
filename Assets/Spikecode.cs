using UnityEngine;

public class Spikecode : MonoBehaviour
{
    private void OnCollisionEnteder2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<RobotController>())
        {
            Debug.Log(gameObject.name + " has been destroyed!");
            gameObject.SetActive(false);  // Disables the robot;
        }
    }
}
