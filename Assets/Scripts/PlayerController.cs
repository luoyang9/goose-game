using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour {
    public Vector2 hookPosition;
    public Rigidbody2D rBody;
    public int numArrows = START_ARROWS;
    public float nextArrowFire = 0f;
    public Arrow arrowPrefab;
    public InputActionMapper actions;
    public RopeSystem ropeSystem;
    public Transform crosshair;
    private WallCheck leftWallCheck;
    private WallCheck rightWallCheck;

    public const int START_ARROWS = 5;
    public const float FRICTION = 0.2f;
    public const float PULL_SPEED = 30f;
    public const float JUMP_VELOCITY = 20f;
    public const float MAX_MOVEMENT_SPEED = 20f;
    private const float WALL_JUMP_H_SPEED = 8f;
    private const float WALL_JUMP_FORCE_TIME = 0.1f;
    private const float WALL_SLIDE_DRAG = 20f;
    public const float MAX_FALL = 30f;
    public const float ARROW_COOLDOWN = 0.5f;
    public const float ARROW_START_DIST = 2f;

    public const int IDLE_STATE = 0;
    public const int RUN_STATE = 1;
    public const int JUMP_STATE = 2;
    public const int FALL_STATE = 3;
    public const int HOOK_PULL_STATE = 4;
    public const int HOOK_END_STATE = 5;

    private StateMachine machine;
    public int moveX = 0;
    private int forceMoveX;
    private float forceMoveXTimer;

    void Start() {
        machine = gameObject.GetComponent<StateMachine>();
        machine.RegisterState(IDLE_STATE, IdleUpdate, null, null);
        machine.RegisterState(RUN_STATE, RunUpdate, null, null);
        machine.RegisterState(JUMP_STATE, JumpUpdate, JumpBegin, null);
        machine.RegisterState(FALL_STATE, FallUpdate, null, FallEnd);
        machine.RegisterState(HOOK_PULL_STATE, HookPullUpdate, null, null);
        machine.RegisterState(HOOK_END_STATE, HookEndUpdate, null, null);

        // initialize wallChecks
        var wallChecks = GetComponentsInChildren<WallCheck>();
        // they have opposite dirs
        if (wallChecks[0].dir == 1) {
            rightWallCheck = wallChecks[0];
            leftWallCheck = wallChecks[1];
        } else {
            rightWallCheck = wallChecks[1];
            leftWallCheck = wallChecks[0];
        }

        forceMoveX = 0;
        forceMoveXTimer = 0;
    }

    void FixedUpdate() {
    }

    void Update() {
        HandleDirection();
        HandleCrosshair();
        HandleArrowShoot();

        // force moving direction
        if (forceMoveXTimer > 0) {
            forceMoveXTimer -= Time.deltaTime;
            moveX = forceMoveX;
        }
    }

    private void HandleDirection() {
        moveX = actions.GetHorizontalDirection();
    }

    private void HandleArrowShoot() {
        if (actions.ArrowShootPressed() && nextArrowFire < Time.time) {
            FireArrow();
            nextArrowFire = Time.time + ARROW_COOLDOWN;
        }
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
        // falling
        if (rBody.velocity.y < -0.001) {
            return FALL_STATE;
        }
        // running
        if (moveX != 0) {
            return RUN_STATE;
        }

        return IDLE_STATE;
    }

    private int RunUpdate() {
        // move
        Vector2 velocity = rBody.velocity;
        velocity.x = moveX * MAX_MOVEMENT_SPEED;
        rBody.velocity = velocity;

        // jump
        if (actions.JumpPressed()) {
            return JUMP_STATE;
        }
        // falling
        if (rBody.velocity.y < -0.001) {
            return FALL_STATE;
        }
        // idle
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
        // wall slide
        if (moveX == -1 && leftWallCheck.Touching || moveX == 1 && rightWallCheck.Touching) {
            // decelerate up to a max sliding velocity
            rBody.drag = WALL_SLIDE_DRAG;
        } else {
            rBody.drag = 0;
        }
        // wall jump
        if (actions.JumpPressed()) {
            if (WallJumpCheck(-1)) {
                WallJump(1);
                return JUMP_STATE;
            } else if (WallJumpCheck(1)) {
                WallJump(-1);
                return JUMP_STATE;
            }
        }
        return FALL_STATE;
    }

    private void FallEnd() {
        rBody.drag = 0;
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
        if (actions.JumpPressed()) {
            ropeSystem.ResetRope();
            if (WallJumpCheck(-1)) {
                WallJump(1);
            } else if (WallJumpCheck(1)) {
                WallJump(-1);
            } else {
                // todo: put jump logic here
            }
            return JUMP_STATE;
        }
        return HOOK_END_STATE;
    }

    private void JumpBegin() {
        Vector2 velocity = rBody.velocity;
        velocity.y = JUMP_VELOCITY;
        rBody.velocity = velocity;
    }

    public bool WallJumpCheck(int dir) {
        if (dir == -1) return leftWallCheck.Touching;
        else if (dir == 1) return rightWallCheck.Touching;
        else return false;
    }

    public void WallJump(int dir) {
        forceMoveX = dir;
        forceMoveXTimer = WALL_JUMP_FORCE_TIME;
        var vel = rBody.velocity;
        vel.x = WALL_JUMP_H_SPEED * dir;
        rBody.velocity = vel;
        // todo: maybe do jump logic here
    }

    public void Airborne() {
        Vector2 velocity = rBody.velocity;
        if (forceMoveXTimer <= 0 && moveX != 0) {
            // Horizontal air movement
            velocity.x = moveX * MAX_MOVEMENT_SPEED;
        }
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

    private void HandleCrosshair() {
        Vector2 aimDirection = actions.CalculateAim();
        var aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x);
        if (aimAngle < 0f) {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }
        var x = transform.position.x + 2f * Mathf.Cos(aimAngle);
        var y = transform.position.y + 2f * Mathf.Sin(aimAngle);
        var crossHairPosition = new Vector3(x, y, 0);
        crosshair.transform.position = crossHairPosition;
    }

}
