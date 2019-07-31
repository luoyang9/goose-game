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

    void Start() {
        machine = gameObject.GetComponent<StateMachine>();
        machine.SwitchState<FallState>();
    }

    void FixedUpdate() {
    }

    void Update() {
        HandleDirection();
        if (actions.ArrowShootPressed() && nextArrowFire < Time.time) {
            FireArrow();
            nextArrowFire = Time.time + ARROW_COOLDOWN;
        }
    }
    private void HandleDirection() {
        facing = actions.GetHorizontalDirection();
    }
    
    public bool ReachedHook() {
        return Vector2.Distance(transform.position, hookPosition) < RopeSystem.MIN_ROPE_LENGTH;
    }

    public void Idle() {
        // friction
        Vector2 velocity = rBody.velocity;
        if(velocity.x != 0) {
            velocity.x -= FRICTION * velocity.x;
        }
        rBody.velocity = velocity;
    }

    public void Run() {
        Vector2 velocity = rBody.velocity;
        velocity.x = facing * MAX_MOVEMENT_SPEED;
        rBody.velocity = velocity;
    }

    public void HandleJump() {
        if (actions.JumpPressed()) {
            machine.SwitchState<JumpState>();
        }
    }

    public void Jump() {
        Vector2 velocity = rBody.velocity;
        velocity.y = JUMP_VELOCITY;
        rBody.velocity = velocity;
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

    public void AutoRappel() {
        var hookVelocity = (hookPosition - (Vector2)transform.position).normalized * PULL_SPEED;
        rBody.velocity = hookVelocity;
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
