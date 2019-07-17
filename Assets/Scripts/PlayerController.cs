using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour {
    private StateMachine machine;
    public bool grounded = false;
    public int direction = 0;

    public Vector2 hookPosition;
    public float swingForce = 80f;
    public bool isSwinging = false;
    public Rigidbody2D rBody;
    public Transform groundCheck;
    public new BoxCollider2D collider;
    public LayerMask groundLayerMask;

    public const float GRAVITY_ACCELERATION = 50f;
    public const float JUMP_VELOCITY = 30f;
    public const float MOVEMENT_ACCELERATION = 500f;
    public const float MAX_MOVEMENT_SPEED = 20f;
    public const float MAX_FALL = 30f;

    void Start() {
        collider = gameObject.GetComponent<BoxCollider2D>();
        machine = gameObject.GetComponent<StateMachine>();
        machine.SwitchState<IdleState>();
    }
    
    void FixedUpdate() {
        grounded = Physics2D.OverlapBox(groundCheck.position, collider.size, 0, groundLayerMask);

        // update direction
        float horizontalInput = Input.GetAxis("Horizontal");
        if (horizontalInput < 0) direction = -1;
        else if (horizontalInput > 0) direction = 1;
        else direction = 0;
    }
    
    void Update() {
    }

    public void Run() {
        Vector2 velocity = rBody.velocity;
        velocity.x = direction * MAX_MOVEMENT_SPEED;
        rBody.velocity = velocity;
    }

    public void HardFall() {
        Vector2 velocity = rBody.velocity;
        velocity.y = -MAX_FALL;
        // Air friction
        if(direction == 0) {
            if (velocity.x > 0) {
                velocity.x -= MOVEMENT_ACCELERATION * Time.deltaTime;
                velocity.x = Mathf.Max(0, velocity.x);
            } else if (velocity.x < 0) {
                velocity.x += MOVEMENT_ACCELERATION * Time.deltaTime;
                velocity.x = Mathf.Min(0, velocity.x);
            }
        }
        rBody.velocity = velocity;
    }

    public void Jump() {
        Vector2 velocity = rBody.velocity;
        velocity.y = JUMP_VELOCITY;
        rBody.velocity = velocity;
    }

    public void Airborne() {
        Vector2 velocity = rBody.velocity;
        if(direction != 0) {
            // Horizontal air movement
            velocity.x += direction * MOVEMENT_ACCELERATION * Time.deltaTime;
            if(direction > 0) velocity.x = Mathf.Min(MAX_MOVEMENT_SPEED, velocity.x);
            else if(direction < 0) velocity.x = Mathf.Max(-MAX_MOVEMENT_SPEED, velocity.x);
        } else {
            // Air friction
            if(velocity.x > 0) {
                velocity.x -= MOVEMENT_ACCELERATION * Time.deltaTime;
                velocity.x = Mathf.Max(0, velocity.x);
            } else if(velocity.x < 0) {
                velocity.x += MOVEMENT_ACCELERATION * Time.deltaTime;
                velocity.x = Mathf.Min(0, velocity.x);
            }
        }
        // Gravity
        velocity.y -= GRAVITY_ACCELERATION * Time.deltaTime;
        velocity.y = Mathf.Max(-MAX_FALL, velocity.y);
        rBody.velocity = velocity;
    }

    public void Swing() {
        // 1 - Get a normalized direction vector from the player to the hook point
        var playerToHookDirection = (hookPosition - (Vector2)transform.position).normalized;
        // 2 - Inverse the direction to get a perpendicular direction
        Vector2 perpendicularDirection = new Vector2(direction * playerToHookDirection.y, -1 * direction * playerToHookDirection.x);
        rBody.AddForce(perpendicularDirection * swingForce, ForceMode2D.Force);
    }
}
