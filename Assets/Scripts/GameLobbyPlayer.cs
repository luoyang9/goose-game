using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GameLobbyPlayer : MonoBehaviour {
    // input
    public PlayerInput playerInput;
    private InputActionAsset actions;
    private InputAction cancel;
    private InputAction longCancel;
    private InputAction submit;
    private InputAction navigate;

    public PlayerSelection Selection { get; set; }
    private PlayerManager playerManager;

    private void Awake() {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

        actions = playerInput.actions;
        cancel = actions["Cancel"];
        longCancel = actions["LongCancel"];
        submit = actions["Submit"];
        navigate = actions["Navigate"];
    }

    private void OnEnable() {
        cancel.performed += OnCancel;
        longCancel.performed += OnLongCancel;
        submit.performed += OnSubmit;
        navigate.performed += OnNavigate;
    }

    private void OnDisable() {
        cancel.performed -= OnCancel;
        longCancel.performed -= OnLongCancel;
        submit.performed -= OnSubmit;
        navigate.performed -= OnNavigate;
    }
    
    public void OnCancel(InputAction.CallbackContext c) {
        Selection.OnCancel();
    }

    // todo: move callback methods to playerSelection
    public void OnLongCancel(InputAction.CallbackContext c) {
        playerManager.OnPressBack();
    }

    public void OnSubmit(InputAction.CallbackContext c) {
        Selection.OnSubmit();
    }

    public void OnNavigate(InputAction.CallbackContext c) {
        var nav = navigate.ReadValue<Vector2>();
        Selection.ChangeCharacter((int)nav.x);
    }
}
