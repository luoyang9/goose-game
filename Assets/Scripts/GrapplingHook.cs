using UnityEngine;
using System.Collections;

public class GrapplingHook : MonoBehaviour {
    const int LAYER_DEFAULT = 0;
    public bool hooked;
    public Rigidbody2D rbody;
    public float HOOK_ACCELERATION = 25f;
    public float MAX_HOOK_SPEED = 120f;
    public Vector2 direction;
    public RopeSystem ropeSystem;

    void Awake() {
        hooked = false;
        rbody = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        // if it's environment, latch on
        if (collider.gameObject.layer == LAYER_DEFAULT) {
            hooked = true;
            direction = Vector2.zero;
            ropeSystem.LatchHook(transform.position);
        }
    }

    void FixedUpdate() {
        // TODO: rewrite with state machine
        if (!hooked) {
            rbody.velocity += direction * HOOK_ACCELERATION * Time.deltaTime;
        }
        rbody.velocity = Mathf.Max(MAX_HOOK_SPEED, rbody.velocity.magnitude) * direction;
    }
}
