using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour {
    private StateMachine machine;
    public int direction = 0;

    public Vector2 hookPosition;
    public Rigidbody2D rBody;
    public BoxCollider2D playerCollider;
    public Transform groundCheck;
    public LayerMask groundLayerMask;

    public const float PULL_FORCE = 140f;
    public const float GRAVITY_ACCELERATION = 250;
    public const float JUMP_VELOCITY = 60f;
    public const float MAX_MOVEMENT_SPEED = 20f;
    public const float MAX_FALL = 30f;

    void Start() {
        machine = gameObject.GetComponent<StateMachine>();
        machine.SwitchState<FallState>();
    }

    void FixedUpdate() {
        // update direction
        float horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput < 0) direction = -1;
        else if (horizontalInput > 0) direction = 1;
        else direction = 0;
    }

    void Update() {
    }

    public bool isGrounded() {
        return Physics2D.OverlapBox(groundCheck.position, playerCollider.size, 0, groundLayerMask);
    }

    public void Run() {
        Vector2 velocity = rBody.velocity;
        velocity.x = direction * MAX_MOVEMENT_SPEED;
        rBody.velocity = velocity;
    }

    public void HardFall() {
        Vector2 velocity = rBody.velocity;
        velocity.y = -MAX_FALL;
        rBody.velocity = velocity;
    }

    public void Jump() {
        Vector2 velocity = rBody.velocity;
        velocity.y = JUMP_VELOCITY;
        rBody.velocity = velocity;
    }

    public void Airborne() {
        Vector2 velocity = rBody.velocity;
        if (direction != 0) {
            // Horizontal air movement
            velocity.x = direction * MAX_MOVEMENT_SPEED;
        }
        // Gravity
        velocity.y -= GRAVITY_ACCELERATION * Time.deltaTime;
        velocity.y = Mathf.Max(-MAX_FALL, velocity.y);
        rBody.velocity = velocity;
    }

    public void AutoRappel() {
        var playerToHookDirection = (hookPosition - (Vector2)transform.position).normalized;
        rBody.AddForce(PULL_FORCE * playerToHookDirection);
    }
}
