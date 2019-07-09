using UnityEngine;
using System.Collections;


public class PlayerMovement : MonoBehaviour {
    public Vector2 hookPosition;
    public float swingForce = 80f;
    public bool isSwinging = false;
    public Rigidbody2D rBody;
    private bool grounded = false;
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
    }

    // frame by frame update
    void Update() {
        grounded = Physics2D.OverlapBox(groundCheck.position, collider.size, 0, groundLayerMask);
    }

    // physics update
    void FixedUpdate() {
        Vector2 velocity = rBody.velocity;
        float horizontalInput = Input.GetAxis("Horizontal");
        // Grounded horiziontal movement: Instant acceleration to max speed
        if (grounded && !isSwinging) {
            if (horizontalInput < 0) velocity.x = -MAX_MOVEMENT_SPEED;
            else if (horizontalInput > 0) velocity.x = MAX_MOVEMENT_SPEED;
            else velocity.x = 0;
        }
        // Non-grounded non-swinging horizontal movement: Acceleration
        if (!grounded && !isSwinging) {
            if (horizontalInput < 0) {
                velocity.x -= MOVEMENT_ACCELERATION * Time.deltaTime;
                velocity.x = Mathf.Max(-MAX_MOVEMENT_SPEED, velocity.x);
            } else {
                velocity.x += MOVEMENT_ACCELERATION * Time.deltaTime;
                velocity.x = Mathf.Min(MAX_MOVEMENT_SPEED, velocity.x);
            }
        }
        // Swinging movement
        if (horizontalInput != 0f && isSwinging) {
            // 1 - Get a normalized direction vector from the player to the hook point
            var playerToHookDirection = (hookPosition - (Vector2)transform.position).normalized;
            // 2 - Inverse the direction to get a perpendicular direction
            Vector2 perpendicularDirection;
            if (horizontalInput < 0) {
                perpendicularDirection = new Vector2(-playerToHookDirection.y, playerToHookDirection.x);
            } else {
                perpendicularDirection = new Vector2(playerToHookDirection.y, -playerToHookDirection.x);
            }
            rBody.AddForce(perpendicularDirection * swingForce, ForceMode2D.Force);
        }
        // Gravity
        if (!grounded) {
            velocity.y -= GRAVITY_ACCELERATION * Time.deltaTime;
            velocity.y = Mathf.Max(-MAX_FALL, velocity.y);
        }
        // Jumping
        if (Input.GetKey(KeyCode.W) && grounded) {
            velocity.y = JUMP_VELOCITY;
        }
        // Hard Fall
        if (Input.GetKey(KeyCode.S) && !grounded) {
            velocity.y = -MAX_FALL;
        }
        rBody.velocity = velocity;
    }
}
