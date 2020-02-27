using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Camera camera;
    public Transform[] spawns;
    private PlayerController[] players;
    private int numPlayers;

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

    void OnPlayerDeath(string id)
    {
        numPlayers -= 1;

        if (numPlayers <= 1)
        {
            SceneManager.LoadScene("EndGame");
        }
    }
}
