using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    const int MAX_PLAYERS = 4;

    //public GameObject[] characters;
    public GameObject[] playerSelections;
    public PlayerInputManager playerInputManager;

    private List<PlayerMapping> mappings;

    public PlayerManager() {
        mappings = new List<PlayerMapping>(MAX_PLAYERS);
    }

    void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    int NumPlayers
    {
        get
        {
            return mappings.Count;
        }
    }

    public void OnStartFight()
    {
        // todo: loadsceneasync
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("Main");
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        var playerIdx = NumPlayers;
        var mapping = new PlayerMapping(playerIdx, player, null);
        mappings.Add(mapping);
        playerSelections[playerIdx].SetActive(true);
    }
}

public class PlayerMapping
{
    public int PlayerTag { get; private set; }
    public PlayerInput Controller { get; private set; }
    public GameObject Character { get; private set; }

    public PlayerMapping(int tag, PlayerInput controller, GameObject character)
    {
        PlayerTag = tag;
        Controller = controller;
        Character = character;
    }
}
