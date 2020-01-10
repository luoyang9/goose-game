using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputActionMapper: MonoBehaviour {
    public PlayerInput playerInput;

    private InputActionAsset actions;
    private InputAction jump;
    private InputAction move;
    private InputAction arrowShoot;
    private InputAction hookShoot;
    private InputAction melee;
    private InputAction aim;

    private void Awake()
    {
        actions = playerInput.actions;
        jump = actions["Jump"];
        move = actions["Move"];
        arrowShoot = actions["ArrowShoot"];
        hookShoot = actions["HookShoot"];
        melee = actions["Melee"];
        aim = actions["Aim"];
    }
    
    public bool JumpPressed {
        get { return jump.ReadValue<float>() > 0.5f; }
    }

    /**
     * Returns -1 if left pressed, 1 if right pressed, else 0
     */
    public int HorizontalDirection {
        get
        {
            var direction = move.ReadValue<Vector2>();
            int ret;

            if (direction.x > 0) ret = 1;
            else if (direction.x < 0) ret = -1;
            else ret = 0;

            return ret;
        }
    }

    public bool HookShootPressed {
        get { return hookShoot.ReadValue<float>() > 0.5f; }
    }

    public bool ArrowShootPressed {
        get { return arrowShoot.ReadValue<float>() > 0.5f; }
    }

    public bool FallPressed {
        get
        {
            var direction = move.ReadValue<Vector2>();
            return direction.y < -0.5f;
        }
    }

    public bool MeleePressed {
        get { return melee.ReadValue<float>() > 0.5f; }
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
                    var stickDirection = aim.ReadValue<Vector2>();
                    direction = stickDirection == Vector2.zero ? new Vector2(0, 1) : stickDirection;
                    break;
            }
            return direction;
        }
    }
}
