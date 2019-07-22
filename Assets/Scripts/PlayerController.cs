using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour {
    private StateMachine machine;
    public int direction = 0;

    public Vector2 hookPosition;
    public Rigidbody2D rBody;
    public string controller = "K";
    public int numArrows = START_ARROWS;
    public float nextArrowFire = 0f;
    public GameObject arrowPrefab;

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
        if (Input.GetButtonDown(controller + "_Jump")) {
            machine.SwitchState<JumpState>();
        }
        if (Input.GetAxis(controller + "_Fire2") > 0.50 && nextArrowFire < Time.time) {
            FireArrow();
            nextArrowFire = Time.time + ARROW_COOLDOWN;
        }
    }
    private void HandleDirection() {
        float horizontalInput = Input.GetAxis(controller + "_Horizontal");
        if (horizontalInput < 0) direction = -1;
        else if (horizontalInput > 0) direction = 1;
        else direction = 0;
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
        velocity.x = direction * MAX_MOVEMENT_SPEED;
        rBody.velocity = velocity;
    }

    public void HardFall() {
        Vector2 velocity = rBody.velocity;
        velocity.y = -MAX_FALL;
        rBody.velocity = velocity;
    }

    public void Jump() {
        Vector2 velocity = rBody.velocity;
        velocity.y = JUMP_VELOCITY;
        rBody.velocity = velocity;
    }

    public void Airborne() {
        Vector2 velocity = rBody.velocity;
        if (direction != 0) {
            // Horizontal air movement
            velocity.x = direction * MAX_MOVEMENT_SPEED;
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
        Vector2 aimDirection;
        if (controller == "K") {
            var worldMousePosition =
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            aimDirection = ((Vector2)worldMousePosition - (Vector2)transform.position).normalized;
        }
        else
        {
            var horizontalAxis = Input.GetAxisRaw(controller + "_X");
            var verticalAxis = Input.GetAxisRaw(controller + "_Y");
            if (horizontalAxis == 0f && verticalAxis == 0f) {
                aimDirection = new Vector2(0, 1).normalized;
            } else {
                aimDirection = new Vector2(horizontalAxis, -verticalAxis).normalized;
            }
        }
        GameObject obj = Instantiate(
            arrowPrefab,
            new Vector2(transform.position.x, transform.position.y) + aimDirection * ARROW_START_DIST,
            Quaternion.identity);
        Arrow arrow = obj.GetComponent<Arrow>();
        arrow.direction = aimDirection;
        numArrows--;
    }
}
