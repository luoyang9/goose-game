using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour {
    private StateMachine machine;
    public int facing = 0;

    public Vector2 hookPosition;
    public Rigidbody2D rBody;
    public int numArrows = START_ARROWS;
    public float nextArrowFire = 0f;
    public Arrow arrowPrefab;
    public InputActionMapper actions;
    public RopeSystem ropeSystem;

    public const int START_ARROWS = 5;
    public const float FRICTION = 0.2f;
    public const float PULL_SPEED = 30f;
    public const float JUMP_VELOCITY = 20f;
    public const float MAX_MOVEMENT_SPEED = 20f;
    public const float MAX_FALL = 30f;
    public const float ARROW_COOLDOWN = 0.5f;
    public const float ARROW_START_DIST = 2f;

    public const int IDLE_STATE = 0;
    public const int RUN_STATE = 1;
    public const int JUMP_STATE = 2;
    public const int FALL_STATE = 3;
    public const int HOOK_PULL_STATE = 4;
    public const int HOOK_END_STATE = 5;

    void Start() {
        machine = gameObject.GetComponent<StateMachine>();
        machine.RegisterState(IDLE_STATE, IdleUpdate, null, null);
        machine.RegisterState(RUN_STATE, RunUpdate, null, null);
        machine.RegisterState(JUMP_STATE, JumpUpdate, JumpBegin, null);
        machine.RegisterState(FALL_STATE, FallUpdate, null, null);
        machine.RegisterState(HOOK_PULL_STATE, HookPullUpdate, null, null);
        machine.RegisterState(HOOK_END_STATE, HookEndUpdate, null, null);
    }

    void FixedUpdate() {
    }

    void Update() {
        HandleDirection();
        if (actions.ArrowShootPressed() && nextArrowFire < Time.time) {
            FireArrow();
            nextArrowFire = Time.time + ARROW_COOLDOWN;
        }
        name = "Player - " + machine.CurrentState;
    }

    private int IdleUpdate() {
        // friction
        Vector2 velocity = rBody.velocity;
        if (velocity.x != 0) {
            velocity.x -= FRICTION * velocity.x;
        }
        rBody.velocity = velocity;
        
        // jump
        if (actions.JumpPressed()) {
            return JUMP_STATE;
        }

        if (facing != 0) {
            return RUN_STATE;
        }

        return IDLE_STATE;
    }

    private int RunUpdate() {
        // move
        Vector2 velocity = rBody.velocity;
        velocity.x = facing * MAX_MOVEMENT_SPEED;
        rBody.velocity = velocity;

        // jump
        if (actions.JumpPressed()) {
            return JUMP_STATE;
        }

        if (Mathf.Abs(rBody.velocity.x) < 0.001) {
            return IDLE_STATE;
        }

        return RUN_STATE;
    }

    private int JumpUpdate() {
        Airborne();
        if (rBody.velocity.y < 0) {
            return FALL_STATE;
        }

        return JUMP_STATE;
    }

    private int FallUpdate() {
        if (Mathf.Abs(rBody.velocity.y) < 0.001) {
            return IDLE_STATE;
        }
        Airborne();
        return FALL_STATE;
    }

    private int HookPullUpdate() {
        // autorappel
        var hookVelocity = (hookPosition - (Vector2)transform.position).normalized * PULL_SPEED;
        rBody.velocity = hookVelocity;

        // reached hook
        if (Vector2.Distance(transform.position, hookPosition) < RopeSystem.MIN_ROPE_LENGTH) {
            if (ropeSystem.hitPlatform) {
                ropeSystem.ResetRope();
                return FALL_STATE;
            }
            return HOOK_END_STATE;
        }

        return HOOK_PULL_STATE;
    }

    private int HookEndUpdate() {
        return HOOK_END_STATE;
    }

    private void JumpBegin() {
        Vector2 velocity = rBody.velocity;
        velocity.y = JUMP_VELOCITY;
        rBody.velocity = velocity;
    }

    private void HandleDirection() {
        facing = actions.GetHorizontalDirection();
    }

    public void Airborne() {
        Vector2 velocity = rBody.velocity;
        if (facing != 0) {
            // Horizontal air movement
            velocity.x = facing * MAX_MOVEMENT_SPEED;
        }
        // Gravity
        velocity.y = Mathf.Max(-MAX_FALL, velocity.y);
        rBody.velocity = velocity;
    }

    public void Kill() {
        Destroy(gameObject);
    }

    private void FireArrow() {
        if (numArrows == 0) {
            return;
        }
        Vector2 aimDirection = actions.CalculateAim();
        var arrowStartPos = (Vector2)transform.position + aimDirection * ARROW_START_DIST;
        Arrow arrow = Instantiate(arrowPrefab, arrowStartPos, Quaternion.identity);
        arrow.direction = aimDirection;
        numArrows--;
    }
}
