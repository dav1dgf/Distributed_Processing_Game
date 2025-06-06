using UnityEngine;

public class LadderMovement : MonoBehaviour
{
    private float vertical;
    private float speed = 3.5f;
    private bool isLadder;
    private bool isClimbing;

    [SerializeField] private Rigidbody2D rb;

    // Update is called once per frame
    void Update()
    {
        vertical = Input.GetAxis("Vertical");

        if(isLadder && Mathf.Abs(vertical) >= 0f)
        {
            isClimbing = true;
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * speed);
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Ladder"))
        {
            isLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimbing = false;
        }
    }
}
