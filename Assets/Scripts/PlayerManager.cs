using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerManager : MonoBehaviour
{
    public const int MIN_PLAYERS = 1;
    public const int MAX_PLAYERS = 4;

    public PlayerSelectionUI[] playerSelectionUis;
    public PlayerInputManager playerInputManager;

    private PlayerSelection[] playerSelections;
    private int playerAddIdx;

    private List<PlayerMapping> activeMappings;

    void Awake() {
        playerSelections = new PlayerSelection[MAX_PLAYERS];
    }

    void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
        SceneManager.activeSceneChanged -= OnSceneChange;
    }
        
    public void OnPressBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnStartFight() {
        var numPlayersReady = playerSelections.Count(
            selection => selection != null && selection.State == PlayerSelection.READY_STATE
        );
        var allReady = !playerSelections.Any(
            selection => 
                selection != null
                && selection.State == PlayerSelection.PENDING_STATE
        );
        if (numPlayersReady >= MIN_PLAYERS && allReady) {
            // good
            playerInputManager.onPlayerJoined -= OnPlayerJoined;
            activeMappings = GetPlayerMappings();
            DontDestroyOnLoad(gameObject);
            SceneManager.LoadScene("Main");
        } else {
            Debug.Log("can't start game with fewer than 2 players");
        }
    }

    private void OnSceneChange(Scene from, Scene to)
    {
        if (to.name == "PlayerSelection") return;

        if (to.name == "Main")
        {
            // some gameobjects are destroyed by now
            var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameManager.SpawnPlayers(activeMappings);
        }
        Destroy(gameObject);
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        // update selection logic
        var devices = new InputDevice[player.devices.Count];
        for (int i = 0; i < player.devices.Count; i++)
        {
            devices[i] = player.devices[i];
        }
        var selection = player.GetComponent<PlayerSelection>();
        selection.Manager = this;
        selection.PlayerName = $"P{playerAddIdx + 1}";
        selection.Controller = devices;
        playerSelections[playerAddIdx] = selection;
        // update UI
        var selectionUI = playerSelectionUis[playerAddIdx];
        selectionUI.Selection = selection;
        selectionUI.SetControllerText(player.currentControlScheme);
        // hook up input
        var lobbyPlayer = player.GetComponent<GameLobbyPlayer>();
        lobbyPlayer.Selection = selection;

        playerAddIdx += 1;
    }

    private List<PlayerMapping> GetPlayerMappings() {
        var activeMappings = new List<PlayerMapping>(MAX_PLAYERS);
        foreach (var selection in playerSelections) {
            if (selection != null && selection.State == PlayerSelection.READY_STATE) {
                var mapping = selection.GetMapping();
                activeMappings.Add(mapping);
            }
        }
        return activeMappings;
    }
}
