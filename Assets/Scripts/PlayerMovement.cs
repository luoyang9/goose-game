using UnityEngine;
using System.Collections;


public class PlayerMovement : MonoBehaviour {
    public Vector2 hookPosition;
    public float swingForce = 10f;
    public bool isSwinging = false;
    public Rigidbody2D rBody;
    private bool grounded = false;
    public Transform groundCheck;
    public new BoxCollider2D collider;
    public LayerMask groundLayerMask;

    public const float GRAVITY_ACCELERATION = 3;
    public const float JUMP_VELOCITY = 40;
    public const float MOVEMENT_ACCELERATION = 5;
    public const float MAX_MOVEMENT_SPEED = 15;
    public const float MAX_FALL = 40;

    void Start() {
        collider = gameObject.GetComponent<BoxCollider2D>();
    }

    // frame by frame update
    void Update() {
        grounded = Physics2D.OverlapBox(groundCheck.position, collider.size, 0, groundLayerMask);
    }

    // physics update
    void FixedUpdate() {
        // moving on ground
        // Default acceleration (with gravity)
        //Vector2 acceleration = new Vector2(0, -GRAVITY_ACCELERATION);
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector2 velocity = rBody.velocity;
        if (grounded && !isSwinging) {
            if (horizontalInput < 0) {
                velocity.x = -MAX_MOVEMENT_SPEED;
            } else if (horizontalInput > 0) {
                velocity.x = MAX_MOVEMENT_SPEED;
            } else {
                velocity.x = 0;
            }
            rBody.velocity = velocity;
        }
        // TODO: jumping?
        //if (Input.GetKey(KeyCode.S)) {
        //    acceleration += Vector2.down * MOVEMENT_ACCELERATION;
        //}
        //Vector2 velocity = rBody.velocity + acceleration;
        //velocity.x = Mathf.Min(Mathf.Max(velocity.x, -MAX_MOVEMENT_SPEED), MAX_MOVEMENT_SPEED);
        //velocity.y = Mathf.Max(velocity.y, -MAX_FALL);
        //if (Input.GetKey(KeyCode.W) && onGround) // Jump
        //{
        //    velocity.y = JUMP_VELOCITY;
        //}
        //rBody.velocity = velocity;

        // on grappling hook in air
        if (horizontalInput < 0f || horizontalInput > 0f) {
            if (isSwinging) {
                // 1 - Get a normalized direction vector from the player to the hook point
                var playerToHookDirection = (hookPosition - (Vector2)transform.position).normalized;
                // 2 - Inverse the direction to get a perpendicular direction
                //Quaternion.AngleAxis
                Vector2 perpendicularDirection;
                if (horizontalInput < 0) {
                    perpendicularDirection = new Vector2(-playerToHookDirection.y, playerToHookDirection.x);
                } else {
                    perpendicularDirection = new Vector2(playerToHookDirection.y, -playerToHookDirection.x);
                }

                var force = perpendicularDirection * swingForce;
                rBody.AddForce(force, ForceMode2D.Force);
            } else {
                //if (grounded) {
                //    // hit ground, stop velocity
                //    var groundForce = speed * 2f;
                //    rBody.AddForce(new Vector2((horizontalInput * groundForce - rBody.velocity.x) * groundForce, 0));
                //    rBody.velocity = new Vector2(rBody.velocity.x, rBody.velocity.y);
                //}
            }
        }

        //if (!isSwinging) {
        //    if (!grounded) return;

        //    isJumping = jumpInput > 0f;
        //    if (isJumping) {
        //        rBody.velocity = new Vector2(rBody.velocity.x, jumpSpeed);
        //    }
        //}
    }
}
