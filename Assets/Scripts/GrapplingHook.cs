using UnityEngine;
using System.Collections;

public class GrapplingHook : MonoBehaviour {
    const int LAYER_WALL = 11;
    const int LAYER_PLATFORM = 10;
    public bool hooked;
    public Rigidbody2D rbody;
    public float HOOK_SPEED;
    public float MAX_ROPE_DISTANCE;
    public Vector2 direction;
    public RopeSystem ropeSystem;
    public Transform playerTransform;

    public int currentState;
    public const int ST_SHOOT = 0;
    public const int ST_HOOKED = 1;
    public const int ST_RETURN = 2;
    
    void Awake() {
        currentState = ST_SHOOT;
        rbody = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        // if it's environment, latch on
        if (currentState == ST_SHOOT && (collider.gameObject.layer == LAYER_PLATFORM || collider.gameObject.layer == LAYER_WALL)) {
            currentState = ST_HOOKED;
            direction = Vector2.zero;
            ropeSystem.LatchHook(transform.position);
            ropeSystem.hitPlatform = collider.gameObject.layer == LAYER_PLATFORM;
        }
    }
    
    void UpdateHookPosition() {
        var currentPos = (Vector2)transform.position;
        switch (currentState) {
            case ST_SHOOT:
                rbody.MovePosition(currentPos + direction * HOOK_SPEED * Time.deltaTime);
                if (ropeSystem.RopeDistance > MAX_ROPE_DISTANCE && currentState == ST_SHOOT) {
                    currentState = ST_RETURN;
                }
                break;
            case ST_RETURN:
                direction = ((Vector2)playerTransform.position - currentPos).normalized;
                rbody.MovePosition(currentPos + direction * HOOK_SPEED * Time.deltaTime);
                break;
        }
    }

    void HandleReturn() {
        if (currentState == ST_RETURN && ropeSystem.RopeDistance < RopeSystem.MIN_ROPE_LENGTH) {
            ropeSystem.ResetRope();
        }
    }

    void FixedUpdate() {
        UpdateHookPosition();
        HandleReturn();
    }
}
