using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour {
    public PlayerInput input;
    private InputAction navMenu;

    void Awake() {
        navMenu = input.actions["Menu"];
    }

    private void OnEnable() {
        navMenu.performed += OnMenu;
    }

    private void OnDisable() {
        navMenu.performed -= OnMenu;
    }

    private void OnMenu(InputAction.CallbackContext c) {
        SceneManager.LoadScene("MainMenu");
    }
}
