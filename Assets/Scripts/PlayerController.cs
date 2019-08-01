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

    public const int IdleState = 0;
    public const int RunState = 1;
    public const int JumpState = 2;
    public const int FallState = 3;
    public const int HookPullState = 4;
    public const int HookEndState = 5;

    void Start() {
        machine = gameObject.GetComponent<StateMachine>();
        machine.RegisterState(IdleState, idleUpdate, null, null);
        machine.RegisterState(RunState, runUpdate, null, null);
        machine.RegisterState(JumpState, jumpUpdate, jumpBegin, null);
        machine.RegisterState(FallState, fallUpdate, null, null);
        machine.RegisterState(HookPullState, hookPullUpdate, null, null);
        machine.RegisterState(HookEndState, hookEndUpdate, null, null);
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

    private int idleUpdate() {
        // friction
        Vector2 velocity = rBody.velocity;
        if (velocity.x != 0) {
            velocity.x -= FRICTION * velocity.x;
        }
        rBody.velocity = velocity;
        
        // jump
        if (actions.JumpPressed()) {
            return JumpState;
        }

        if (facing != 0) {
            return RunState;
        }

        return IdleState;
    }

    private int runUpdate() {
        // move
        Vector2 velocity = rBody.velocity;
        velocity.x = facing * MAX_MOVEMENT_SPEED;
        rBody.velocity = velocity;

        // jump
        if (actions.JumpPressed()) {
            return JumpState;
        }

        if (Mathf.Abs(rBody.velocity.x) < 0.001) {
            return IdleState;
        }

        return RunState;
    }

    private int jumpUpdate() {
        Airborne();
        if (rBody.velocity.y < 0) {
            return FallState;
        }

        return JumpState;
    }

    private int fallUpdate() {
        if (Mathf.Abs(rBody.velocity.y) < 0.001) {
            return IdleState;
        }
        Airborne();
        return FallState;
    }

    private int hookPullUpdate() {
        // autorappel
        var hookVelocity = (hookPosition - (Vector2)transform.position).normalized * PULL_SPEED;
        rBody.velocity = hookVelocity;

        // reached hook
        if (Vector2.Distance(transform.position, hookPosition) < RopeSystem.MIN_ROPE_LENGTH) {
            if (ropeSystem.hitPlatform) {
                ropeSystem.ResetRope();
                return FallState;
            }
            return HookEndState;
        }

        return HookPullState;
    }

    private int hookEndUpdate() {
        return HookEndState;
    }

    private void jumpBegin() {
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
