using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputActionMapper: MonoBehaviour {
    public PlayerInput playerInput;

    private Vector2 lastAimDirection;
    private InputActionAsset actions;
    private InputAction jump;
    private InputAction move;
    private InputAction arrowShoot;
    private InputAction hookShoot;
    private InputAction melee;
    private InputAction aim;

    public event Action Jump;
    public event Action ArrowShoot;
    public event Action HookShoot;
    public event Action Melee;

    private void Awake()
    {
        lastAimDirection = new Vector2(0, 1);
        actions = playerInput.actions;
        jump = actions["Jump"];
        move = actions["Move"];
        arrowShoot = actions["ArrowShoot"];
        hookShoot = actions["HookShoot"];
        melee = actions["Melee"];
        aim = actions["Aim"];
    }

    private void OnEnable() {
        jump.performed += OnJump;
        arrowShoot.performed += OnArrowShoot;
        hookShoot.performed += OnHookShoot;
        melee.performed += OnMelee;
    }

    private void OnDisable() {
        jump.performed -= OnJump;
        arrowShoot.performed -= OnArrowShoot;
        hookShoot.performed -= OnHookShoot;
        melee.performed -= OnMelee;
    }

    private void OnJump(InputAction.CallbackContext c) {
        Jump?.Invoke();
    }

    private void OnArrowShoot(InputAction.CallbackContext c) {
        ArrowShoot?.Invoke();
    }

    private void OnHookShoot(InputAction.CallbackContext c) {
        HookShoot?.Invoke();
    }

    private void OnMelee(InputAction.CallbackContext c) {
        Melee?.Invoke();
    }

    /**
     * Returns -1 if left pressed, 1 if right pressed, else 0
     */
    public int HorizontalDirection {
        get
        {
            var direction = move.ReadValue<Vector2>();
            int ret;

            if (direction.x > 0.5f) ret = 1;
            else if (direction.x < -0.5f) ret = -1;
            else ret = 0;

            return ret;
        }
    }

    public bool DownPressed {
        get
        {
            var direction = move.ReadValue<Vector2>();
            return direction.y < -0.5f;
        }
    }

    /**
     * returns unit vector of aimed direction from player
     */
    public Vector2 Aim {
        get
        {
            Vector2 direction;
            var controlScheme = playerInput.currentControlScheme;
            switch (controlScheme)
            {
                // keyboard and stick must be treated differently
                case "Keyboard&Mouse":
                    var position = aim.ReadValue<Vector2>();
                    var worldMousePosition =
                            Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 0f));
                    direction = ((Vector2)worldMousePosition - (Vector2)transform.position).normalized;
                    break;
                default:
                    var aimDirection = aim.ReadValue<Vector2>();
                    var moveDirection = move.ReadValue<Vector2>();
                    direction = aimDirection == Vector2.zero 
                        ? (moveDirection == Vector2.zero ? lastAimDirection : moveDirection)
                        : aimDirection;
                    direction = direction.normalized;
                    lastAimDirection = direction;
                    break;
            }
            return direction;
        }
    }
}