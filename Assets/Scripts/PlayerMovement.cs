using UnityEngine;
using System.Collections;


public class PlayerMovement : MonoBehaviour {
    public Vector2 hookPosition;
    public bool isSwinging = false;
    public Rigidbody2D rBody;
    private bool grounded = false;
    public Transform groundCheck;
    public new BoxCollider2D collider;
    public LayerMask groundLayerMask;

    public const float PULL_FORCE = 180f;
    public const float JUMP_VELOCITY = 20f;
    public const float MAX_MOVEMENT_SPEED = 15f;
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
                velocity.x = -MAX_MOVEMENT_SPEED;
                velocity.x = Mathf.Max(-MAX_MOVEMENT_SPEED, velocity.x);
            } else if (horizontalInput > 0) {
                velocity.x = MAX_MOVEMENT_SPEED;
                velocity.x = Mathf.Min(MAX_MOVEMENT_SPEED, velocity.x);
            }
        }

        // terminal speed
        if (!grounded) {
            velocity.y = Mathf.Max(-MAX_FALL, velocity.y);
        }
        // Jumping
        if (Input.GetKey(KeyCode.W) && grounded && !isSwinging) {
            velocity.y = JUMP_VELOCITY;
        }
        // Hard Fall
        if (Input.GetKey(KeyCode.S) && !grounded && !isSwinging) {
            velocity.y = -MAX_FALL;
        }
        rBody.velocity = velocity;

        AutoRappel();
    }

    void AutoRappel() {
        var playerToHookDirection = (hookPosition - (Vector2)transform.position).normalized;
        if (isSwinging) {
            // force-based
            rBody.AddForce(PULL_FORCE * playerToHookDirection);
        }
    }
}
