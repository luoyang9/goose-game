using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour {
    // VARIABLES
    public InputActionMapper actions;
    public Vector2 hookPosition;
    public RopeSystem ropeSystem;
    public Transform crosshair;
    public Animator animator;
    public Melee melee;
    public SpriteRenderer sprite;
    public StateMachine machine;
    public WallCheck backWallCheck;
    public WallCheck frontWallCheck;
    public GroundCheck groundCheck;
    public GameObject effect;
    public PlayerMapping PlayerChoice { get; set; }
    public AudioSource jumpAudioSource;
    public AudioSource arrowShootAudioSource;
    public AudioSource hookShootAudioSource;
    public AudioSource meleeAudioSource;
    public AudioSource hurtAudioSource;
    public AudioSource hookLandAudioSource;
    public TMP_Text arrowCount;
    public TMP_Text playerLabel;
    public Transform playerUI;
    public GameObject runDust;
    public GameObject wallSlideDust;
    public GameObject landDust;

    private Camera gameCamera;
    // game manager variable
    public int playerIndex = 0;

    // lives
    public int lives = 5;
    public bool Alive { get { return lives > 0; } }

    // movement
    public int moveX = 0;
    private bool enableMovement = false;
    private int forceMoveX;
    private float forceMoveXTimer;
    public Rigidbody2D rBody;
    private int forceFacing;
    private float forceFacingTimer;
    public int Facing { get; private set; } = 1; // either -1 or 1
    private WallCheck LeftWallCheck { get { return (Facing < 0) ? frontWallCheck : backWallCheck; } }
    private WallCheck RightWallCheck { get { return (Facing > 0) ? frontWallCheck : backWallCheck; } }
    private bool MoveIntoWall {
        get {
            return moveX == -1 && LeftWallCheck.Touching
                || moveX == 1 && RightWallCheck.Touching;
        }
    }
    // arrows
    public int numArrows = START_ARROWS;
    public float nextArrowFire = 0f;
    public Arrow arrowPrefab;
    private bool CanShoot { get { return Time.time > nextArrowFire; } }
    // melee
    private float nextSwingTime = 0;
    private bool CanAttack { get { return Time.time > nextSwingTime; } }
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
    public const float MOVEMENT_ACCELERATION = 2f;
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
    public const float SWING_COOLDOWN = 0.3f;
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
    public const int WALL_SLIDE_STATE = 8;

    // EVENTS
    public delegate void PlayerDeathHandler(PlayerController player);
    public static event PlayerDeathHandler OnPlayerDeath;

    // ANIMATION
    private Vector2 RUN_DUST_OFFSET = new Vector2(-1.3f, 0.45f);
    private Vector2 WALL_DUST_OFFSET = new Vector2(-0.3f, 0);
    private Vector2 LAND_DUST_OFFSET = Vector2.zero;

    private IEnumerator wallDustCoro;

    void Start() {
        gameCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        machine = gameObject.GetComponent<StateMachine>();
        machine.RegisterState(IDLE_STATE, IdleUpdate, null, null);
        machine.RegisterState(RUN_STATE, RunUpdate, RunBegin, null);
        machine.RegisterState(JUMP_STATE, JumpUpdate, JumpBegin, null);
        machine.RegisterState(FALL_STATE, FallUpdate, null, null);
        machine.RegisterState(WALL_SLIDE_STATE, WallSlideUpdate, WallSlideBegin, WallSlideEnd);
        machine.RegisterState(HOOK_PULL_STATE, HookPullUpdate, null, null);
        machine.RegisterState(HOOK_END_STATE, HookEndUpdate, HookEndBegin, null);
        machine.RegisterState(FALL_THROUGH_PLATFORM_STATE, FallThroughUpdate, FallThroughBegin, FallThroughEnd);
        machine.RegisterState(FORCE_FIELD_STATE, ForceFieldUpdate, ForceFieldBegin, null);

        forceMoveX = 0;
        forceMoveXTimer = 0;
        playerLabel.text = PlayerChoice.PlayerTag;
    }

    private void OnEnable() {
        EnableControls();
    }

    private void OnDisable() {
        DisableControls();
    }

    public void EnableControls()
    {
        actions.Jump += OnJump;
        actions.ArrowShoot += OnArrowShoot;
        actions.HookShoot += OnHookShoot;
        actions.Melee += OnMeleeAttack;
        enableMovement = true;
    }

    public void DisableControls()
    {
        actions.Jump -= OnJump;
        actions.ArrowShoot -= OnArrowShoot;
        actions.HookShoot -= OnHookShoot;
        actions.Melee -= OnMeleeAttack;
        enableMovement = false;
    }

    void Update() {
        // actions that are always possible
        CheckIfOffScreen();
        if (enableMovement)
        {
            HandleDirection();
        }
        HandleCrosshair();

        // force moving direction
        if (forceMoveXTimer > 0) {
            forceMoveXTimer -= Time.deltaTime;
            moveX = forceMoveX;
        }
        if (forceFacingTimer > 0) {
            forceFacingTimer -= Time.deltaTime;
            Facing = forceFacing;
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

    private void HandleFacingScale() {
        Vector3 scale = transform.localScale;
        scale.x = Facing;
        transform.localScale = scale;
        // don't want ui to flip
        Vector3 uiScale = playerUI.localScale;
        uiScale.x = Facing;
        playerUI.localScale = uiScale;
    }

    private void HandleDirection() {
        moveX = actions.HorizontalDirection;
        if (moveX != 0 && forceFacingTimer <= 0) {
            Facing = moveX;
        }
        HandleFacingScale();
    }

    private int IdleUpdate() {
        // friction
        Vector2 velocity = rBody.velocity;
        if (velocity.x != 0) {
            velocity.x -= FRICTION * velocity.x;
        }
        rBody.velocity = velocity;
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

    private void RunBegin() {
        MakeDust(runDust, RUN_DUST_OFFSET);
    }

    private int RunUpdate() {
        // move
        Vector2 velocity = rBody.velocity;
        velocity.x += moveX * MOVEMENT_ACCELERATION;
        velocity.x = ClampMoveSpeed(velocity.x);
        rBody.velocity = velocity;
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

    private void FallThroughBegin() {
        Vector2 velocity = rBody.velocity;
        velocity.y = -MAX_FALL/2;
        rBody.velocity = velocity;
        Debug.Log(rBody.velocity.y);
    }

    private int FallThroughUpdate() {
        if (fallThroughTimer <= 0) {
            return FALL_STATE;
        }
        gameObject.layer = LayerMask.NameToLayer("PlayerDropping");
        fallThroughTimer -= Time.deltaTime;
        return FALL_THROUGH_PLATFORM_STATE;
    }

    public void FallThroughEnd() {
        gameObject.layer = LayerMask.NameToLayer("Player");
        fallThroughTimer = FALL_THROUGH_PLATFORM_DURATION;
    }

    private void JumpBegin() {
        Vector2 velocity = rBody.velocity;
        velocity.y = JUMP_VELOCITY;
        rBody.velocity = velocity;
        jumpAudioSource.Play();
    }

    private int JumpUpdate() {
        Airborne();
        if (rBody.velocity.y < 0) {
            return FALL_STATE;
        }

        return JUMP_STATE;
    }

    private int FallUpdate() {
        if (Mathf.Abs(rBody.velocity.y) < 0.001 && groundCheck.TouchingGround) {
            MakeDust(landDust, LAND_DUST_OFFSET);
            return IDLE_STATE;
        }
        Airborne();
        // wall slide
        if (MoveIntoWall) {
            return WALL_SLIDE_STATE;
        }
        return FALL_STATE;
    }

    private void WallSlideBegin() {
        // decelerate up to a max sliding velocity
        rBody.drag = WALL_SLIDE_DRAG;
        forceFacing = -moveX;
        wallDustCoro = MakeWallSlideDust();
        StartCoroutine(wallDustCoro);
    }

    private int WallSlideUpdate() {
        if (!MoveIntoWall) return FALL_STATE;

        Airborne();
        forceFacingTimer = 0.1f; // should be long enough to cover till next frame
        return WALL_SLIDE_STATE;
    }

    private void WallSlideEnd() {
        rBody.drag = 0;
        StopCoroutine(wallDustCoro);
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

    private void HookEndBegin() {
        if (LeftWallCheck.Touching) {
            forceFacing = 1;
        } else if (RightWallCheck.Touching) {
            forceFacing = -1;
        } else {
            forceFacing = 0;
        }
    }

    private int HookEndUpdate() {
        if (forceFacing != 0) {
            forceFacingTimer = 0.1f;
        }
        return HOOK_END_STATE;
    }

    private void ForceFieldBegin() {
        rBody.velocity = Vector2.zero;
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

    IEnumerator MakeWallSlideDust() {
        while (true) {
            yield return new WaitForSeconds(0.2f);
            MakeDust(wallSlideDust, WALL_DUST_OFFSET);
        }
    }

    private void MakeDust(GameObject dustPrefab, Vector2 offset) {
        var scale = transform.localScale;
        offset.x *= scale.x;
        var pos = transform.position + (Vector3) offset;
        var dust = Instantiate(
            dustPrefab,
            pos,
            Quaternion.identity
        );
        dust.transform.localScale = scale;
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
        forceFacing = dir;
        forceFacingTimer = WALL_JUMP_FORCE_TIME;
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
        ropeSystem.ResetRope();
        OnPlayerDeath?.Invoke(this);
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
        Facing = aimDirection.x > 0 ? 1: -1;
        ShootArrowAnimation(aimDirection);
    }

    private void ShootArrowAnimation(Vector2 aim) {
        // animation
        string trigger;
        var horizontal = aim.x != 0 && Math.Abs(aim.y / aim.x) <= 1;
        if (horizontal) {
            trigger = "Attack";
        } else if (aim.y > 0) {
            trigger = "UpAttack";
        } else {
            trigger = "DownAttack";
        }
        animator.SetTrigger(trigger);
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
                // fall through platforms
                if (actions.DownPressed && groundCheck.isTouchingPlatform()) {
                    nextState = FALL_THROUGH_PLATFORM_STATE;
                } else {
                    nextState = JUMP_STATE;
                }
                break;
            case FALL_STATE:
            case WALL_SLIDE_STATE:
                nextState = CheckDoWallJump();
                break;
            case HOOK_PULL_STATE:
                ropeSystem.ResetRope();
                nextState = FALL_STATE;
                break;
            case HOOK_END_STATE:
                // wall jump
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
                if (CanShoot) {
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
        if (InLag || !CanAttack) return;

        switch (machine.CurrentState) {
            case FORCE_FIELD_STATE:
                break;
            default:
                // attack
                nextSwingTime = Time.time + SWING_COOLDOWN;
                forceFacing = Facing;
                forceFacingTimer = Melee.ATTACK_DURATION;
                meleeAudioSource.Play();
                if (actions.VerticalDirection > 0) {
                    // up
                    animator.SetTrigger("UpAttack");
                    melee.Attack(Melee.AttackDirection.UP);
                } else if (actions.VerticalDirection < 0) {
                    // down
                    animator.SetTrigger("DownAttack");
                    melee.Attack(Melee.AttackDirection.DOWN);
                } else {
                    // forward
                    animator.SetTrigger("Attack");
                    melee.Attack(Melee.AttackDirection.FORWARD);
                }
                break;
        }
    }
}