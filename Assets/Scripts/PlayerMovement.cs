using UnityEngine;
using System.Collections;


public class PlayerMovement : MonoBehaviour {
    public Vector2 ropeHook;
    public float swingForce = 4f;
    public bool isSwinging = false;
    private float horizontalInput = 0;
    public Rigidbody2D rBody;
    private bool grounded = false;

    void Awake() {
        // 2
    }

    // frame by frame update
    void Update() {
        horizontalInput = Input.GetAxis("Horizontal");
        //grounded = 
    }

    // physics update
    void FixedUpdate() {
        if (horizontalInput < 0f || horizontalInput > 0f) {
            if (isSwinging) {
                // 1 - Get a normalized direction vector from the player to the hook point
                var playerToHookDirection = (ropeHook - (Vector2)transform.position).normalized;

                // 2 - Inverse the direction to get a perpendicular direction
                Vector2 perpendicularDirection;
                if (horizontalInput < 0) {
                    perpendicularDirection = new Vector2(-playerToHookDirection.y, playerToHookDirection.x);
                    var leftPerpPos = (Vector2)transform.position - perpendicularDirection * -2f;
                    Debug.DrawLine(transform.position, leftPerpPos, Color.green, 0f);
                } else {
                    perpendicularDirection = new Vector2(playerToHookDirection.y, -playerToHookDirection.x);
                    var rightPerpPos = (Vector2)transform.position + perpendicularDirection * 2f;
                    Debug.DrawLine(transform.position, rightPerpPos, Color.green, 0f);
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
