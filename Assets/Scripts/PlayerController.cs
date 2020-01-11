using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    // VARIABLES
    public InputActionMapper actions;
    public Vector2 hookPosition;
    public RopeSystem ropeSystem;
    public Transform crosshair;
    public Animator animator;
    public bool alive = true;
    public Melee melee;
    public SpriteRenderer sprite;
    public StateMachine machine;
    public WallCheck leftWallCheck;
    public WallCheck rightWallCheck;
    public GroundCheck groundCheck;
    public PlayerMapping PlayerChoice { get; set; }
    // movement
    public int moveX = 0;
    private int forceMoveX;
    private float forceMoveXTimer;
    public Rigidbody2D rBody;
    public int Facing { get; private set; } = 1; // either -1 or 1
    // arrows
    public int numArrows = START_ARROWS;
    public float nextArrowFire = 0f;
    public Arrow arrowPrefab;
    // melee
    private float nextSwingTime = 0;

    // CONSTANTS
    // Movement
    public const float FRICTION = 0.2f;
    public const float PULL_SPEED = 30f;
    public const float JUMP_VELOCITY = 30f;
    public const float MAX_MOVEMENT_SPEED = 18f;
    private const float WALL_JUMP_H_SPEED = 8f;
    private const float WALL_JUMP_FORCE_TIME = 0.1f;
    private const float WALL_SLIDE_DRAG = 20f;
    public const float MAX_FALL = 25f;
    public const float FALL_THROUGH_SPEED = 0.5f;
    // Arrows
    public const float ARROW_COOLDOWN = 0.5f;
    public const float ARROW_START_DIST = 2f;
    public const int START_ARROWS = 5;
    // Melee
    public const float SWING_COOLDOWN = 0.5f;
    public const float SWING_TIME = 0.1f;
    // states
    public const int IDLE_STATE = 0;
    public const int RUN_STATE = 1;
    public const int JUMP_STATE = 2;
    public const int FALL_STATE = 3;
    public const int HOOK_PULL_STATE = 4;
    public const int HOOK_END_STATE = 5;
    public const int FALL_THROUGH_PLATFORM_STATE = 6;

    // EVENTS
    public delegate void PlayerDeathHandler(int tag);
    public static event PlayerDeathHandler OnPlayerDeath;

    void Start() {
        machine = gameObject.GetComponent<StateMachine>();
        machine.RegisterState(IDLE_STATE, IdleUpdate, null, null);
        machine.RegisterState(RUN_STATE, RunUpdate, null, null);
        machine.RegisterState(JUMP_STATE, JumpUpdate, JumpBegin, null);
        machine.RegisterState(FALL_STATE, FallUpdate, null, FallEnd);
        machine.RegisterState(HOOK_PULL_STATE, HookPullUpdate, null, null);
        machine.RegisterState(HOOK_END_STATE, HookEndUpdate, null, null);
        machine.RegisterState(FALL_THROUGH_PLATFORM_STATE, FallThroughUpdate, null, null);
        forceMoveX = 0;
        forceMoveXTimer = 0;
    }

    void Update() {
        // actions that are always possible
        HandleDirection();
        HandleCrosshair();
        HandleArrowShoot();
        HandleMeleeAttack();

        // force moving direction
        if (forceMoveXTimer > 0) {
            forceMoveXTimer -= Time.deltaTime;
            moveX = forceMoveX;
        }

        UpdateAnimator();
    }

    private void UpdateAnimator() {
        animator.SetInteger("CurrentState", machine.CurrentState);
    }

    private void HandleDirection() {
        moveX = actions.HorizontalDirection;
        if (moveX != 0) {
            Facing = moveX;
        }
        sprite.flipX = Facing < 0;
    }

    private void HandleArrowShoot() {
        if (actions.ArrowShootPressed && nextArrowFire < Time.time) {
            FireArrow();
            nextArrowFire = Time.time + ARROW_COOLDOWN;
        }
    }

    private void HandleMeleeAttack() {
        if (actions.MeleePressed && Time.time > nextSwingTime) {
            nextSwingTime = Time.time + SWING_COOLDOWN;
            melee.Attack();
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
        if (actions.JumpPressed) {
            return JUMP_STATE;
        }
        // fall through platformsF
        if (actions.DownPressed && groundCheck.isTouchingPlatform()) {
            return FALL_THROUGH_PLATFORM_STATE;
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
        if (actions.JumpPressed) {
            return JUMP_STATE;
        }
        // fall through platforms
        if (actions.DownPressed && groundCheck.isTouchingPlatform()) {
            return FALL_THROUGH_PLATFORM_STATE;
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

    private int FallThroughUpdate() {
        if (!groundCheck.isTouchingPlatform()) {
            return FALL_STATE;
        }
        Vector2 position = rBody.position;
        position.y -= FALL_THROUGH_SPEED;
        rBody.position = position;
        return FALL_THROUGH_PLATFORM_STATE;
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
        if (actions.JumpPressed) {
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
        if (actions.DownPressed) {
            ropeSystem.ResetRope();
            return FALL_STATE;
        }
        // autorappel
        var hookVelocity = (hookPosition - (Vector2)transform.position).normalized * PULL_SPEED;
        rBody.velocity = hookVelocity;
        // reached hook
        if (Vector2.Distance(transform.position, hookPosition) < RopeSystem.MIN_ROPE_LENGTH) {
            // one-way platform
            if (ropeSystem.hitPlatform) {
                ropeSystem.ResetRope();
                return FALL_STATE;
            }
            return HOOK_END_STATE;
        }

        return HOOK_PULL_STATE;
    }

    private int HookEndUpdate() {
        if (actions.DownPressed) {
            ropeSystem.ResetRope();
            return FALL_STATE;
        }

        if (actions.JumpPressed) {
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
        Facing = dir;
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
        alive = false;
        OnPlayerDeath?.Invoke(PlayerChoice.PlayerTag);
    }

    private void FireArrow() {
        if (numArrows == 0) {
            return;
        }
        Vector2 aimDirection = actions.Aim;
        var arrowStartPos = (Vector2)transform.position + aimDirection * ARROW_START_DIST;
        Arrow arrow = Instantiate(arrowPrefab, arrowStartPos, Quaternion.identity);
        arrow.direction = aimDirection;
        numArrows--;
    }

    private void HandleCrosshair() {
        Vector2 aimDirection = actions.Aim;
        var crossHairPosition = transform.position + (Vector3)aimDirection * 2;
        crosshair.transform.position = crossHairPosition;
    }
}
