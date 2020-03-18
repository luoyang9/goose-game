using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public const float START_GAME_DELAY = 2f;
    public const float FINAL_DEATH_DELAY = 2f;
    public const float DEATH_SHAKE_DURATION = 0.3f;
    public Camera camera;
    public Transform[] spawns;
    private PlayerController[] players;
    private List<PlayerMapping> playerMappings;
    private Queue<PlayerController> dyingPlayers;

    private float shakeDurationLeft = 0f;
    private Vector3 initialCameraLocation;

    private int numPlayers;

    private void Start() {
        dyingPlayers = new Queue<PlayerController>();
    }

    private void FixedUpdate() {
        if (shakeDurationLeft > 0) {
            shakeDurationLeft -= Time.deltaTime;
            if (shakeDurationLeft <= 0) {
                shakeDurationLeft = 0;
                camera.transform.position = initialCameraLocation;
            } else {
                camera.transform.localPosition = initialCameraLocation + UnityEngine.Random.insideUnitSphere;
            }
        }
    }

    protected void OnEnable()
    {
        PlayerController.OnPlayerDeath += OnPlayerDeath;
    }

    protected void OnDisable()
    {
        PlayerController.OnPlayerDeath -= OnPlayerDeath;
    }

    private IEnumerator FreezePlayersCoroutine()
    {
        // Initially disables all controllers
        for (int i = 0; i < numPlayers; i++)
        {
            players[i].DisableControls();
        }
        yield return new WaitForSeconds(START_GAME_DELAY);
        for (int i = 0; i < numPlayers; i++)
        {
            players[i].EnableControls();
        }
    }

    public void SpawnPlayers(List<PlayerMapping> mappings)
    {
        numPlayers = mappings.Count;
        playerMappings = mappings;
        Debug.Assert(numPlayers <= PlayerManager.MAX_PLAYERS, "unsupported number of players");

        players = new PlayerController[numPlayers];

        for (int i = 0; i < numPlayers; i++)
        {
            var mapping = mappings[i];
            var devices = mapping.Controller;
            var input = PlayerInput.Instantiate(mapping.Character, pairWithDevices: devices);
            input.transform.position = spawns[i].position;
            var player = input.GetComponent<PlayerController>();
            player.playerIndex = i;
            player.PlayerChoice = mapping;
            player.gameCamera = camera;
            players[i] = player;
        }
        StartCoroutine(FreezePlayersCoroutine());
    }

    void OnPlayerDeath(PlayerController player) {
        dyingPlayers.Enqueue(player);
        StartCoroutine(RespawnCoroutine());
        initialCameraLocation = camera.transform.position;
        if (player.lives == 0) {
            numPlayers -= 1;
            if (numPlayers == 1) {
                StartCoroutine(EndGameCoroutine());
            }
        }
    }

    private IEnumerator EndGameCoroutine() {
        yield return new WaitForSeconds(FINAL_DEATH_DELAY);
        DontDestroyOnLoad(this);
        SceneManager.LoadScene("EndGame");
    }

    public IEnumerator RespawnCoroutine() {
        PlayerController dyingPlayer = dyingPlayers.Dequeue();
        dyingPlayer.gameObject.SetActive(false);
        int randomRespawn = UnityEngine.Random.Range(0, spawns.Length);
        Vector3 newPosition = dyingPlayer.transform.position;
        newPosition.y += 0.3f;
        Instantiate(dyingPlayer.effect, newPosition, Quaternion.identity);
        dyingPlayer.transform.position = spawns[randomRespawn].position;
        dyingPlayer.lives -= 1;
        yield return new WaitForSeconds(1f);
        dyingPlayer.gameObject.SetActive(true);
    }

    public string GetWinnerId() {
        // Assumes only one player left at this point
        for (int i = 0; i < players.Length; i++) {
            if (players[i].Alive) {
                return players[i].PlayerChoice.PlayerTag;
            }
        }
        // Shouldn't ever get here
        Debug.LogError("No alive players found at the end of the game!");
        return null;
    }
}
