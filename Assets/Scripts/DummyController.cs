﻿using UnityEngine;
using System;
using System.Collections;

public class DummyController : MonoBehaviour {
    // VARIABLES
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
    public GameObject runDust;
    public GameObject wallSlideDust;
    public GameObject landDust;
    public AudioClip hurtAudioClip;

    private Camera gameCamera;
    // game manager variable
    public int playerIndex = 0;

    // lives
    public int Lives { get; private set; } = 5;
    public bool Alive { get { return Lives > 0; } }

    // movement
    public int moveX = 0;
    private bool enableMovement = false;
    private float dashTimeLeft = 0.0f;
    private int forceMoveX;
    private float forceMoveXTimer;
    public Rigidbody2D rBody;
    private int forceFacing;
    private int ForceFacing {
        get { return forceFacing; }
        set {
            forceFacing = value;
            Facing = value;
        }
    }
    private float forceFacingTimer;
    private int facing = 1;
    public int Facing {
        get { return facing; }
        private set {
            facing = value;
            HandleFacingScale();
        }
    } // either -1 or 1
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
    public GameObject forceField; // disabled for now
    private float forceFieldTimer = MAX_FORCE_FIELD_DURATION;
    private float fallThroughTimer = FALL_THROUGH_PLATFORM_DURATION;
    private float lagTimer;
    private bool InLag { get { return lagTimer > 0; } }
    // dash
    private float dashInitialVelocityX;
    private float dashLagTimer;
    private bool InDashLag { get { return dashLagTimer > 0; } }

    // CONSTANTS
    // Movement
    public const float FRICTION = 0.2f;
    public const float PULL_SPEED = 30f;
    public const float JUMP_VELOCITY = 30f;
    public const float MOVEMENT_ACCELERATION = 2f;
    public const float MAX_MOVEMENT_SPEED = 15f;
    private const float WALL_JUMP_H_SPEED = 8f;
    private const float WALL_JUMP_FORCE_TIME = 0.1f;
    private const float WALL_SLIDE_DRAG = 20f;
    public const float MAX_FALL = 18f;
    public const float AIR_DRAG = 1f;
    public const float DASH_VELOCITY = 50f;
    public const float DASH_TIME = 0.075f;
    public const float DASH_COOLDOWN = 0.5f;
    // Arrows
    public const float ARROW_COOLDOWN = 0.5f;
    public const float ARROW_START_DIST = 0.5f;
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
    public const int DASH_STATE = 9;

    // EVENTS
    public delegate void DummyDeathHandler();
    public event DummyDeathHandler OnDummyDeath;

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
        machine.RegisterState(HOOK_END_STATE, HookEndUpdate, null, HookEndEnd);
        machine.RegisterState(FALL_THROUGH_PLATFORM_STATE, FallThroughUpdate, FallThroughBegin, FallThroughEnd);
        //machine.RegisterState(FORCE_FIELD_STATE, ForceFieldUpdate, ForceFieldBegin, null);
        machine.RegisterState(DASH_STATE, DashUpdate, DashBegin, DashEnd);

        forceMoveX = 0;
        forceMoveXTimer = 0;
    }

    private void FixedUpdate() {
        CheckIfOffScreen();

        // force moving direction
        if (forceMoveXTimer > 0) {
            forceMoveXTimer -= Time.deltaTime;
            moveX = forceMoveX;
        }
        if (forceFacingTimer > 0) {
            forceFacingTimer -= Time.deltaTime;
            Facing = ForceFacing;
        }

        if (InLag) {
            lagTimer -= Time.deltaTime;
        }

        if (InDashLag) {
            dashLagTimer -= Time.deltaTime;
        }
    }

    void Update() {
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
        velocity.y = -MAX_FALL / 2;
        rBody.velocity = velocity;
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
        ForceFacing = -moveX;
        forceFacingTimer = 0.1f; // should be long enough to cover till next frame

        wallDustCoro = MakeWallSlideDust();
        StartCoroutine(wallDustCoro);
    }

    private int WallSlideUpdate() {
        Airborne();
        if (!MoveIntoWall) return FALL_STATE;

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

    private int HookEndUpdate() {
        if (LeftWallCheck.Touching) {
            ForceFacing = 1;
            forceFacingTimer = 0.1f;
        } else if (RightWallCheck.Touching) {
            ForceFacing = -1;
            forceFacingTimer = 0.1f;
        } else if (groundCheck.TouchingGround) {
            return IDLE_STATE;
        }
        return HOOK_END_STATE;
    }

    private void HookEndEnd() {
        ropeSystem.ResetRope();
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
        var pos = transform.position + (Vector3)offset;
        var dust = Instantiate(
            dustPrefab,
            pos,
            Quaternion.identity
        );
        dust.transform.localScale = scale;
    }

    private void DashEnd() {
        Vector2 velocity = rBody.velocity;
        velocity.x = dashInitialVelocityX;
        rBody.velocity = velocity;
        rBody.gravityScale = 10f;
    }

    private int DashUpdate() {
        dashTimeLeft -= Time.deltaTime;
        if (dashTimeLeft > 0.01 && dashTimeLeft < (DASH_TIME - 0.01)) {
            gameObject.layer = LayerMask.NameToLayer("PlayerDashing");
        } else {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        if (rBody.velocity.y < -0.001) {
            return FALL_STATE;
        }

        if (dashTimeLeft <= 0) {
            return dashInitialVelocityX != 0 ? RUN_STATE : IDLE_STATE;
        }

        return DASH_STATE;
    }

    private void DashBegin() {
        MakeDust(runDust, RUN_DUST_OFFSET);
        Vector2 velocity = rBody.velocity;
        dashInitialVelocityX = velocity.x;
        velocity.x = Facing * DASH_VELOCITY;
        velocity.y = 0f;
        rBody.gravityScale = 0f;
        rBody.velocity = velocity;
        dashTimeLeft = DASH_TIME;
        dashLagTimer = DASH_TIME + DASH_COOLDOWN;
        return;
    }
    
    public void Airborne() {
        Vector2 velocity = rBody.velocity;
        if (forceMoveXTimer <= 0) {
            if (moveX != 0) {
                // Horizontal air movement
                velocity.x += moveX * MOVEMENT_ACCELERATION;
                velocity.x = ClampMoveSpeed(velocity.x);
            } else if (velocity.x != 0) {
                var airDrag = Mathf.Min(Mathf.Abs(velocity.x), AIR_DRAG);
                velocity.x -= velocity.x > 0 ? airDrag : -airDrag;
            }
        }
        velocity.y = Mathf.Max(-MAX_FALL, velocity.y);
        rBody.velocity = velocity;
    }

    public void Kill() {
        AudioSource.PlayClipAtPoint(hurtAudioClip, gameCamera.transform.position);
        ropeSystem.ResetRope();
        MakeBlood(transform.position);
        OnDummyDeath?.Invoke();
    }

    private void MakeBlood(Vector3 newPosition) {
        newPosition.y += 0.3f;
        Instantiate(effect, newPosition, Quaternion.identity);
    }

    public void Spawn(Vector3 pos) {
        transform.position = pos;
        melee.ClearSprite();
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

    private float ClampMoveSpeed(float velocityX) {
        return (velocityX > 0 ? 1f : -1f) * Mathf.Min(Mathf.Abs(velocityX), MAX_MOVEMENT_SPEED);
    }

}
