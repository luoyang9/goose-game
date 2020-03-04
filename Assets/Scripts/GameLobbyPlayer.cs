using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
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

    private Coroutine backProgress;

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
        longCancel.started += OnBeginLongCancel;
        longCancel.canceled += OnCancelLongCancel;
        longCancel.performed += OnLongCancel;
        submit.performed += OnSubmit;
        navigate.performed += OnNavigate;
    }

    private void OnDisable() {
        cancel.performed -= OnCancel;
        longCancel.started -= OnBeginLongCancel;
        longCancel.canceled -= OnCancelLongCancel;
        longCancel.performed -= OnLongCancel;
        submit.performed -= OnSubmit;
        navigate.performed -= OnNavigate;
    }
    
    public void OnCancel(InputAction.CallbackContext c) {
        Selection.OnCancel();
    }

    private void OnBeginLongCancel(InputAction.CallbackContext c) {
        var start = (float) c.startTime;
        var interaction = (HoldInteraction) c.interaction;
        var totalCancelTime = Mathf.Max(interaction.duration, InputSystem.settings.defaultHoldTime);
        backProgress = StartCoroutine(UpdateBackProgress(start, totalCancelTime));
    }

    IEnumerator UpdateBackProgress(float start, float totalTime) {
        while (true) {
            var elapsed = Time.realtimeSinceStartup - start;
            if (elapsed > totalTime) break;

            var pct = elapsed / totalTime;
            playerManager.UpdateBackProgress(pct);
            yield return new WaitForSeconds(.05f);
        }
    }

    private void OnCancelLongCancel(InputAction.CallbackContext c) {
        StopCoroutine(backProgress);
        playerManager.UpdateBackProgress(0);
    }

    public void OnLongCancel(InputAction.CallbackContext c) {
        playerManager.NavigateBack();
    }

    public void OnSubmit(InputAction.CallbackContext c) {
        Selection.OnSubmit();
    }

    public void OnNavigate(InputAction.CallbackContext c) {
        var nav = navigate.ReadValue<Vector2>();
        Selection.ChangeCharacter((int)nav.x);
    }
}
