using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour {
    public PlayerInput input;
    private InputAction navMenu;
    public DummyController dummy;
    public Transform dummySpawn;

    void Awake() {
        navMenu = input.actions["Menu"];
    }

    private void OnEnable() {
        navMenu.performed += OnMenu;
        dummy.OnDummyDeath += OnDummyDeath;
    }

    private void OnDisable() {
        navMenu.performed -= OnMenu;
        dummy.OnDummyDeath -= OnDummyDeath;
    }

    private void OnMenu(InputAction.CallbackContext c) {
        MusicPlayer.Instance.PlayMenuMusic();
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator RespawnDummyCoro() {
        yield return new WaitForSeconds(1);
        dummy.transform.position = dummySpawn.position;
        dummy.gameObject.SetActive(true);
    }

    private void OnDummyDeath() {
        dummy.gameObject.SetActive(false);
        StartCoroutine(RespawnDummyCoro());
    }
}
