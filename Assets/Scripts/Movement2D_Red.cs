using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement2D_Red : MonoBehaviour
{
    public ControlsRED controls;

    public Vector2 direction;

    public Rigidbody2D rb2D;

    public float movementVelocity;

    public Weapon weapon; 

    public bool lookRight;

    public float jumpPower;

    public LayerMask isFloor;
    public Transform FloorController;
    public Vector3 boxDimensions;

    public bool inFloor;

    private void Awake()
    {
        controls = new();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Movement.Jump.started += _ => Jump();
        controls.Movement.Attack.started += _ => Attack(); // New
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.Movement.Jump.started -= _ => Jump();
        controls.Movement.Attack.started -= _ => Attack(); // New
    }

    private void Attack()
    {
        if (weapon != null)
        {
            weapon.Fire();
        }
    }

    private void Update()
    {
        direction = controls.Movement.Move.ReadValue<Vector2>();
        AdjustRotation(direction.x);

        inFloor = Physics2D.OverlapBox(FloorController.position, boxDimensions, 0f, isFloor);
    }

    private void FixedUpdate()
    {
        rb2D.linearVelocity = new Vector2(direction.x * movementVelocity, rb2D.linearVelocity.y);
    }
    public void AdjustRotation(float directionX)
    {
        if (directionX > 0 && lookRight)
        {
            ChangeDirection();
        }
        else if (directionX < 0 && !lookRight)
        {
            ChangeDirection();
        }
    }
    public void ChangeDirection()
    {
        lookRight = !lookRight;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(FloorController.position, boxDimensions);
    }
}
