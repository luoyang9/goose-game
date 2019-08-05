using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public const float ARROW_SPEED = 40f;
    // States
    public const int IN_AIR = 0;
    public const int GROUNDED = 1;

    private int wallLayer;
    private int platformLayer;
    private int playerLayer;

    public Vector2 direction;
    public Rigidbody2D rbody;
    private int state = IN_AIR;

    void Start() {
        wallLayer = LayerMask.NameToLayer("Wall");
        platformLayer = LayerMask.NameToLayer("Platform");
        playerLayer = LayerMask.NameToLayer("Player");
        rbody.velocity = direction * ARROW_SPEED;
        UpdateScale();
        UpdateAngle();
    }

    void FixedUpdate() {
        UpdateAngle();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.layer == wallLayer || collider.gameObject.layer == platformLayer) {
            // if it's environment, stop
            state = GROUNDED;
            rbody.velocity = Vector2.zero;
            rbody.gravityScale = 0;
        } else if (collider.gameObject.layer == playerLayer) {
            PlayerController player = collider.gameObject.GetComponent<PlayerController>();
            if (player == null) return;
            if (state == IN_AIR) {
                player.Kill();
            } else {
                // pick up
                player.numArrows++;
                Destroy(gameObject);
            }
        }
    }

    private void UpdateScale() {
        if (direction.x < 0) {
            var scale = transform.localScale;
            scale.x = -transform.localScale.x;
            transform.localScale = scale;
        }
    }

    private void UpdateAngle() {
        if (state == IN_AIR) {
            var zeroAngle = direction.x < 0 ? Vector2.left : Vector2.right;
            var angle = Vector2.SignedAngle(zeroAngle, rbody.velocity);
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(rotation.x, rotation.y, angle);
            transform.rotation = rotation;
        }
    }
    void OnBecameInvisible() {
        Destroy(gameObject);
    }
}
