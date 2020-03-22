using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerManager : MonoBehaviour
{
    public const int MIN_PLAYERS = 2;
    public const int MAX_PLAYERS = 4;

    public PlayerSelectionUI[] playerSelectionUis;
    public PlayerInputManager playerInputManager;
    public GameObject startGamePrompt;
    public RectTransform backNavProgress;
    const float MAX_PROGRESS_WIDTH = 100f;

    private PlayerSelection[] playerSelections;
    private bool CanFight {
        get {
            var numPlayersReady = playerSelections.Count(
                selection => selection != null && selection.State == PlayerSelection.READY_STATE
            );
            var allReady = !playerSelections.Any(
                selection =>
                    selection != null
                    && selection.State == PlayerSelection.PENDING_STATE
            );
            return numPlayersReady >= MIN_PLAYERS && allReady;
        }
    }
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
    
    public void UpdateBackProgress(float pct) {
        // might cancel cancel action after cancel action performed
        if (backNavProgress == null) return;

        pct = Mathf.Clamp(pct, 0, 1);

        backNavProgress.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, 
            pct * MAX_PROGRESS_WIDTH
        );
    }

    public void NavigateBack() {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnStartFight() {
        if (CanFight) {
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
        selection.StateChanged += OnPlayerStateChange;
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

    private void OnPlayerStateChange(int s) {
        HandleFightPrompt();
    }

    private void HandleFightPrompt() {
        startGamePrompt.SetActive(CanFight);
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
