using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public const float FINAL_DEATH_DELAY = 2f;
    public const float DEATH_SHAKE_DURATION = 0.3f;
    public Camera camera;
    public Transform[] spawns;
    public GameObject banner;
    public Text countdown;

    private PlayerController[] players;
    private List<PlayerMapping> playerMappings;

    private float shakeDurationLeft = 0f;
    private Vector3 initialCameraLocation;

    private int numPlayers;

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

    private IEnumerator CountdownCoroutine()
    {
        banner.SetActive(true);
        // Initially disables all controllers
        for (int i = 0; i < numPlayers; i++)
        {
            players[i].DisableControls();
        }
        countdown.text = "3";
        yield return new WaitForSeconds(1);
        countdown.text = "2";
        yield return new WaitForSeconds(1);
        countdown.text = "1";
        yield return new WaitForSeconds(1);
        countdown.text = "FIGHT!";

        for (int i = 0; i < numPlayers; i++)
        {
            players[i].EnableControls();
        }

        yield return new WaitForSeconds(1);
        banner.SetActive(false);
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
            players[i] = player;
        }
        StartCoroutine(CountdownCoroutine());
    }

    void OnPlayerDeath(PlayerController player) {
        player.gameObject.SetActive(false);
        MakeBlood(player);
        if (player.lives > 0) {
            StartCoroutine(RespawnCoroutine(player));
        } else {
            numPlayers -= 1;
            if (numPlayers == 1) {
                StartCoroutine(EndGameCoroutine());
            }
        }
    }

    private void MakeBlood(PlayerController dyingPlayer) {
        Vector3 newPosition = dyingPlayer.transform.position;
        newPosition.y += 0.3f;
        Instantiate(dyingPlayer.effect, newPosition, Quaternion.identity);
    }

    private IEnumerator EndGameCoroutine() {
        yield return new WaitForSeconds(FINAL_DEATH_DELAY);
        DontDestroyOnLoad(this);
        SceneManager.LoadScene("EndGame");
    }

    public IEnumerator RespawnCoroutine(PlayerController dyingPlayer) {
        int randomRespawn = UnityEngine.Random.Range(0, spawns.Length);
        dyingPlayer.transform.position = spawns[randomRespawn].position;
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
