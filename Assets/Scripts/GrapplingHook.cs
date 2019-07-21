using UnityEngine;
using System.Collections;

public class GrapplingHook : MonoBehaviour {
    public float HOOK_SPEED;
    public float MAX_ROPE_DISTANCE;

    private const int ST_SHOOT = 0;
    private const int ST_HOOKED = 1;
    private const int ST_RETURN = 2;

    private int defaultLayer;
    private int playerLayer;

    private int currentState;
    public bool hooked;
    public Rigidbody2D rbody;
    public Vector2 direction;
    public RopeSystem ropeSystem;
    public Transform playerTransform;

    void Awake() {
        currentState = ST_SHOOT;
        defaultLayer = LayerMask.NameToLayer("Default");
        playerLayer = LayerMask.NameToLayer("Player");
    }

    void OnTriggerEnter2D(Collider2D collider) {
        var collideLayer = collider.gameObject.layer;
        if (collideLayer == defaultLayer) {
            // if it's environment, latch on
            currentState = ST_HOOKED;
            direction = Vector2.zero;
            ropeSystem.LatchHook(transform.position);
        } else if (collideLayer == playerLayer) {
            if (playerTransform.gameObject != collider.gameObject) {
                // hit someone
                Destroy(collider.gameObject);
            }
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
