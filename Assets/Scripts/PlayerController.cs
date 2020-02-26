using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour {
    // VARIABLES
    public Camera gameCamera { get; set; }
    public InputActionMapper actions;
    public Vector2 hookPosition;
    public RopeSystem ropeSystem;
    public Transform crosshair;
    public Animator animator;
    public bool alive = true;
    public Melee melee;
    public SpriteRenderer sprite;
    public StateMachine machine;
    public WallCheck backWallCheck;
    public WallCheck frontWallCheck;
    public GroundCheck groundCheck;
    public PlayerMapping PlayerChoice { get; set; }
    public AudioSource jumpAudioSource;
    public AudioSource arrowShootAudioSource;
    public AudioSource hookShootAudioSource;
    public AudioSource meleeAudioSource;
    public AudioSource hurtAudioSource;
    public AudioSource hookLandAudioSource;
    public TMP_Text arrowCount;
    public TMP_Text playerLabel;

    // movement
    public int moveX = 0;
    private int forceMoveX;
    private float forceMoveXTimer;
    public Rigidbody2D rBody;
    public int Facing { get; private set; } = 1; // either -1 or 1
    private WallCheck LeftWallCheck { get { return (Facing < 0) ? frontWallCheck : backWallCheck; } }
    private WallCheck RightWallCheck { get { return (Facing > 0) ? frontWallCheck : backWallCheck; } }
    // arrows
    public int numArrows = START_ARROWS;
    public float nextArrowFire = 0f;
    public Arrow arrowPrefab;
    // melee
    private float nextSwingTime = 0;
    // force field
    public GameObject forceField;
    private float forceFieldTimer = MAX_FORCE_FIELD_DURATION;
    private float fallThroughTimer = FALL_THROUGH_PLATFORM_DURATION;
    private float lagTimer;
    private bool InLag { get { return lagTimer > 0; } }

    // CONSTANTS
    // Movement
    public const float FRICTION = 0.2f;
    public const float PULL_SPEED = 30f;
    public const float JUMP_VELOCITY = 30f;
    public const float MOVEMENT_ACCELERATION = 6f;
    public const float MAX_MOVEMENT_SPEED = 18f;
    private const float WALL_JUMP_H_SPEED = 8f;
    private const float WALL_JUMP_FORCE_TIME = 0.1f;
    private const float WALL_SLIDE_DRAG = 20f;
    public const float MAX_FALL = 18f;
    public const float AIR_DRAG = 1f;
    // Arrows
    public const float ARROW_COOLDOWN = 0.5f;
    public const float ARROW_START_DIST = 0f;
    public const int START_ARROWS = 5;
    // Melee
    public const float SWING_COOLDOWN = 0.5f;
    public const float SWING_TIME = 0.1f;
    // force field
    public const float MAX_FORCE_FIELD_DURATION = 2;
    // fall through polatform duration
    public const float FALL_THROUGH_PLATFORM_DURATION = 0.15f;
    // states
    public const int IDLE_STATE = 0;
    public const int RUN_STATE = 1;
    public const int JUMP_STATE = 2;
    public const int FALL_STATE = 3;
    public const int HOOK_PULL_STATE = 4;
    public const int HOOK_END_STATE = 5;
    public const int FALL_THROUGH_PLATFORM_STATE = 6;
    public const int FORCE_FIELD_STATE = 7;

    // EVENTS
    public delegate void PlayerDeathHandler(string tag);
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
        machine.RegisterState(FORCE_FIELD_STATE, ForceFieldUpdate, null, null);

        forceMoveX = 0;
        forceMoveXTimer = 0;
        playerLabel.text = PlayerChoice.PlayerTag;
    }

    private void OnEnable() {
        actions.Jump += OnJump;
        actions.ArrowShoot += OnArrowShoot;
        actions.HookShoot += OnHookShoot;
        actions.Melee += OnMeleeAttack;
    }

    private void OnDisable() {
        actions.Jump -= OnJump;
        actions.ArrowShoot -= OnArrowShoot;
        actions.HookShoot -= OnHookShoot;
        actions.Melee -= OnMeleeAttack;
    }

    void Update() {
        // actions that are always possible
        CheckIfOffScreen();
        HandleDirection();
        HandleCrosshair();

        // force moving direction
        if (forceMoveXTimer > 0) {
            forceMoveXTimer -= Time.deltaTime;
            moveX = forceMoveX;
        }

        arrowCount.text = "Arrows: " + numArrows.ToString();

        if (InLag) {
            lagTimer -= Time.deltaTime;
        }

        UpdateAnimator();
    }

    private void UpdateAnimator() {
        animator.SetInteger("CurrentState", machine.CurrentState);
    }

    private void CheckIfOffScreen() {
        var pos = gameCamera.WorldToScreenPoint(transform.position);
        if (!Screen.safeArea.Contains(pos)) {
            Kill();
        }
    }

    private void HandleDirection() {
        moveX = actions.HorizontalDirection;
        if (moveX != 0) {
            Facing = moveX;
        }
        Vector3 scale = transform.localScale;
        scale.x = Facing * scale.x < 1 ? -scale.x : scale.x;
        transform.localScale = scale;
    }

    private int IdleUpdate() {
        // friction
        Vector2 velocity = rBody.velocity;
        if (velocity.x != 0) {
            velocity.x -= FRICTION * velocity.x;
        }
        rBody.velocity = velocity;
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
        // force field
        if (!InLag && actions.ForceFieldPressed) {
            return FORCE_FIELD_STATE;
        }

        return IDLE_STATE;
    }

    private int RunUpdate() {
        // move
        Vector2 velocity = rBody.velocity;
        velocity.x += moveX * MOVEMENT_ACCELERATION;
        velocity.x = ClampMoveSpeed(velocity.x);
        rBody.velocity = velocity;
        // fall through platforms
        if (actions.DownPressed && groundCheck.isTouchingPlatform()) {
            return FALL_THROUGH_PLATFORM_STATE;
        }
        // force field
        if (!InLag && actions.ForceFieldPressed) {
            return FORCE_FIELD_STATE;
        }
        // falling
        if (rBody.velocity.y < -0.001) {
            return FALL_STATE;
        }
        // idle
        if (moveX == 0) {
            return IDLE_STATE;
        }

        return RUN_STATE;
    }

    private int FallThroughUpdate() {
        if (fallThroughTimer <= 0) {
            rBody.gameObject.layer = 8;
            fallThroughTimer = FALL_THROUGH_PLATFORM_DURATION;
            return FALL_STATE;
        }
        rBody.gameObject.layer = 13;
        fallThroughTimer -= Time.deltaTime;
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
        if (moveX == -1 && LeftWallCheck.Touching || moveX == 1 && RightWallCheck.Touching) {
            // decelerate up to a max sliding velocity
            rBody.drag = WALL_SLIDE_DRAG;
        } else {
            rBody.drag = 0;
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
        return HOOK_END_STATE;
    }

    private int ForceFieldUpdate() {
        // if pressed, start timer for lag
        if (actions.ForceFieldPressed && forceFieldTimer > 0) {
            forceField.SetActive(true);
            // holding
            forceFieldTimer -= Time.deltaTime; // todo: don't subtract time on the first press

            return FORCE_FIELD_STATE;
        }
        // let go/ run out
        forceField.SetActive(false);
        forceFieldTimer = MAX_FORCE_FIELD_DURATION;
        lagTimer = 0.4f;
        return IDLE_STATE;
    }

    private void JumpBegin() {
        Vector2 velocity = rBody.velocity;
        velocity.y = JUMP_VELOCITY;
        rBody.velocity = velocity;
    }

    private int CheckDoWallJump() {
        if (backWallCheck.Touching) {
            WallJump(Facing);
            return JUMP_STATE;
        } else if (frontWallCheck.Touching) {
            WallJump(-Facing);
            return JUMP_STATE;
        }
        return FALL_STATE;
    }

    private void WallJump(int dir) {
        forceMoveX = dir;
        forceMoveXTimer = WALL_JUMP_FORCE_TIME;
        var vel = rBody.velocity;
        vel.x = WALL_JUMP_H_SPEED * dir;
        rBody.velocity = vel;
        Facing = dir;
    }

    public void Airborne() {
        Vector2 velocity = rBody.velocity;
        if (forceMoveXTimer <= 0 && moveX != 0) {
            // Horizontal air movement
            velocity.x += moveX * MOVEMENT_ACCELERATION;
            velocity.x = ClampMoveSpeed(velocity.x);
        } else if (velocity.x != 0) {
            var airDrag = Mathf.Min(Mathf.Abs(velocity.x), AIR_DRAG);
            velocity.x -= velocity.x > 0 ? airDrag : -airDrag;
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
        arrowShootAudioSource.Play();
        Vector2 aimDirection = actions.Aim;
        var arrowStartPos = (Vector2)transform.position + aimDirection * ARROW_START_DIST;
        Arrow arrow = Instantiate(arrowPrefab, arrowStartPos, Quaternion.identity);
        arrow.firingPlayer = this;
        arrow.direction = aimDirection;
        numArrows--;
    }

    private void HandleCrosshair() {
        Vector2 aimDirection = actions.Aim;
        var crossHairPosition = transform.position + (Vector3)aimDirection * 2;
        crosshair.transform.position = crossHairPosition;
    }

    private float ClampMoveSpeed(float velocityX) {
        return (velocityX > 0 ? 1f : -1f) * Mathf.Min(Mathf.Abs(velocityX), MAX_MOVEMENT_SPEED);
    }

    private void OnJump() {
        if (InLag) return;

        var nextState = -1;
        switch (machine.CurrentState) {
            case IDLE_STATE:
            case RUN_STATE:
                jumpAudioSource.Play();
                nextState = JUMP_STATE;
                break;
            case FALL_STATE:
                jumpAudioSource.Play();
                nextState = CheckDoWallJump();
                break;
            case HOOK_PULL_STATE:
                jumpAudioSource.Play();
                ropeSystem.ResetRope();
                nextState = FALL_STATE;
                break;
            case HOOK_END_STATE:
                // wall jump
                jumpAudioSource.Play();
                ropeSystem.ResetRope();
                nextState = CheckDoWallJump();
                break;
            default:
                nextState = machine.CurrentState;
                break;
        }
        machine.CurrentState = nextState;
    }

    private void OnArrowShoot() {
        if (InLag) return;

        switch (machine.CurrentState) {
            case FORCE_FIELD_STATE:
                break;
            default:
                if (nextArrowFire < Time.time) {
                    FireArrow();
                    nextArrowFire = Time.time + ARROW_COOLDOWN;
                }
                break;
        }
    }

    private void OnHookShoot() {
        if (InLag) return;

        switch (machine.CurrentState) {
            case FORCE_FIELD_STATE:
                break;
            default:
                ropeSystem.AttemptShootHook();
                break;
        }
    }

    private void OnMeleeAttack() {
        if (InLag) return;

        switch (machine.CurrentState) {
            case FORCE_FIELD_STATE:
                break;
            default:
                if (Time.time > nextSwingTime) {
                    nextSwingTime = Time.time + SWING_COOLDOWN;
                    meleeAudioSource.Play();
                    melee.Attack();
                }
                break;
        }
    }
}
