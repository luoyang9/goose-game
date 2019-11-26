using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionMapper: MonoBehaviour {
    public string controller = "K";

    private string jumpAxis;
    private string horizontalAxis;
    private string verticalAxis;
    private string fire1Axis;
    private string fire2Axis;
    private string aimJoystickXAxis;
    private string aimJoystickYAxis;
    private string meleeAxis;

    void Awake() {
        jumpAxis = controller + "_Jump";
        horizontalAxis = controller + "_Horizontal";
        verticalAxis = controller + "_Vertical";
        fire1Axis = controller + "_Fire1";
        fire2Axis = controller + "_Fire2";
        aimJoystickXAxis = controller + "_X";
        aimJoystickYAxis = controller + "_Y";
        meleeAxis = controller + "_Attack";
    }

    #region messages sent by PlayerInput

    public void OnMove(InputValue input)
    {
        Vector2 direction = (Vector2)input.Get();
        HorizontalDirection = (int)direction.x;
    }

    public void OnJump(InputValue input)
    {
        JumpPressed = (bool)input.Get();
    }

    #endregion

    #region exposed input actions

    public bool JumpPressed { get; private set; }

    /**
     * Returns -1 if left pressed, 1 if right pressed, else 0
     */
    public int HorizontalDirection { get; private set; }

    public bool HookShootPressed() {
        return Input.GetAxis(fire1Axis) > 0.5f;
    }

    public bool ArrowShootPressed() {
        return Input.GetAxis(fire2Axis) > 0.5f;
    }

    public bool HookReleasePressed() {
        return Input.GetAxis(verticalAxis) < -0.5f;
    }

    public bool MeleePressed() {
        return Input.GetAxis(meleeAxis) > 0.5f;
    }

    /**
     * returns unit vector of aimed direction from player
     */
    public Vector2 CalculateAim() {
        Vector2 aimDirection;
        if (controller == "K") {
            // keyboard
            var worldMousePosition =
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
            aimDirection = (Vector2)worldMousePosition - (Vector2)transform.position;
        } else {
            // joystick
            var horizontalAxis = Input.GetAxisRaw(aimJoystickXAxis);
            var verticalAxis = Input.GetAxisRaw(aimJoystickYAxis);
            aimDirection = new Vector2(horizontalAxis, -verticalAxis);
            if (aimDirection == Vector2.zero) {
                aimDirection = new Vector2(0, 1);
            }
        }
        return aimDirection.normalized;
    }

    #endregion
}
