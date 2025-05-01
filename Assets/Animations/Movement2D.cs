using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement2D : MonoBehaviour
{
    // Reference to the weapon
    public Weapon weapon;

    // Input controls (dynamically set in Inspector)
    public InputActionAsset controls;

    public Vector2 direction;

    public Rigidbody2D rb2D;

    public float movementVelocity;

    public bool lookLeft;

    public float jumpPower;

    public LayerMask isFloor;
    public Transform FloorController;
    public Vector3 boxDimensions;

    public bool inFloor;

    private InputActionMap movementActions;

    private void Awake()
    {
        // Enable the controls based on the assigned controls input
        if (controls != null)
        {
            movementActions = controls.FindActionMap("Movement");
        }
        if (lookLeft) ChangeDirection();
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

    // Handle robot rotation based on movement direction
    public void AdjustRotation(float directionX)
    {
        if (directionX > 0 && !lookLeft)
        {
            ChangeDirection();
        }
        else if (directionX < 0 && lookLeft)
        {
            ChangeDirection();
        }
    }

    // Change the facing direction of the robot
    public void ChangeDirection()
    {
        lookLeft = !lookLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Jump the robot
    private void Jump()
    {
        if (inFloor)
        {
            rb2D.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
        }
    }

    // Handle attack
    private void Attack()
    {
        if (weapon != null)
        {
            Debug.Log("Bullet fired!");
            weapon.Fire();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(FloorController.position, boxDimensions);
    }
}
