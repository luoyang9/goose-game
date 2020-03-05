using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public const float DEATH_SHAKE_DURATION = 0.3f;
    public Camera camera;
    public Transform[] spawns;
    private PlayerController[] players;

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

    public void SpawnPlayers(List<PlayerMapping> mappings)
    {
        numPlayers = mappings.Count;
        Debug.Assert(numPlayers <= PlayerManager.MAX_PLAYERS, "unsupported number of players");

        players = new PlayerController[numPlayers];

        for (int i = 0; i < numPlayers; i++)
        {
            var mapping = mappings[i];
            var devices = mapping.Controller;
            var input = PlayerInput.Instantiate(mapping.Character, pairWithDevices: devices);
            input.transform.position = spawns[i].position;
            var player = input.GetComponent<PlayerController>();
            player.PlayerChoice = mapping;
            player.gameCamera = camera;
            players[i] = player;
        }
    }

    void OnPlayerDeath(string id) {
        numPlayers -= 1;
        initialCameraLocation = camera.transform.position;
        shakeDurationLeft = DEATH_SHAKE_DURATION;
        if (numPlayers <= 1)
        {
            SceneManager.LoadScene("EndGame");
        }
    }
}
