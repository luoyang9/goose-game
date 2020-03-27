using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public const float ROTATION_SPEED = 20f;
    public const float ARROW_SPEED = 40f;
    // States
    public const int IN_AIR = 0;
    public const int GROUNDED = 1;
    public const int STOPPED = 2;

    private float timer = 0.3f;
    private int wallLayer;
    private int platformLayer;
    private int playerLayer;

    public Vector2 direction;
    public Rigidbody2D rbody;
    public PlayerController firingPlayer;
    public AudioSource landAudioSource;
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
        if (timer > 0) {
            timer -= Time.deltaTime;
        }
        UpdateScale();
        UpdateAngle();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.layer == wallLayer || collider.gameObject.layer == platformLayer) {
            // if it's environment, stop
            state = GROUNDED;
            rbody.velocity = Vector2.zero;
            rbody.gravityScale = 0;
            landAudioSource.Play();
        } else if (collider.gameObject.layer == playerLayer) {
            DummyController dummy = collider.gameObject.GetComponent<DummyController>();
            if (dummy && state == IN_AIR) {
                dummy.Kill();
                return;
            }
            PlayerController player = collider.gameObject.GetComponent<PlayerController>();
            if (player == firingPlayer && timer > 0) {
                return;
            }
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
        if (direction.x * transform.localScale.x < 0) {
            var scale = transform.localScale;
            scale.x = -transform.localScale.x;
            transform.localScale = scale;
        }
    }

    private void UpdateAngle() {
        if (state == IN_AIR) {
            transform.Rotate(0, 0, ROTATION_SPEED);
        }
    }

    void OnBecameInvisible() {
        Destroy(gameObject);
    }
}
