using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerManager : MonoBehaviour
{
    public const int MAX_PLAYERS = 4;

    public PlayerSelection[] playerSelections;
    public PlayerInputManager playerInputManager;

    private readonly PlayerMapping[] mappings;
    private int playerAddIdx;

    public PlayerManager() {
        mappings = new PlayerMapping[MAX_PLAYERS];
    }

    void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        SceneManager.activeSceneChanged += OnSceneChange;
        for (int i = 0; i < playerSelections.Length; i++)
        {
            playerSelections[i].OnSelectionStatusChange += OnSelectionStatusChange(i);
            playerSelections[i].OnCharacterChanged += OnCharacterChange(i);
        }
    }

    void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    private PlayerSelection.CharacterChangedAction OnCharacterChange(int selectionIdx)
    {
        return (character) => mappings[selectionIdx].Character = character;
    }

    private PlayerSelection.StatusChangedAction OnSelectionStatusChange(int selectionIdx)
    {
        return (status) => mappings[selectionIdx].Active = status;
    }

    public void OnPressBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnStartFight()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("Main");
    }

    private void OnSceneChange(Scene from, Scene to)
    {
        if (to.name == "PlayerSelection") return;

        if (to.name == "Main")
        {
            // some gameobjects are destroyed by now
            playerInputManager.onPlayerJoined -= OnPlayerJoined;

            var activeMappings = new List<PlayerMapping>(MAX_PLAYERS);
            foreach (var mapping in mappings)
            {
                if (mapping != null && mapping.Active)
                {
                    activeMappings.Add(mapping);
                }
            }

            var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameManager.SpawnPlayers(activeMappings);
        }
        Destroy(gameObject);
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        // update mapping
        var devices = new InputDevice[player.devices.Count];
        for (int i = 0; i < player.devices.Count; i++)
        {
            devices[i] = player.devices[i];
        }
        var mapping = new PlayerMapping(playerAddIdx, devices, null);
        mappings[playerAddIdx] = mapping;
        // update UI
        var selection = playerSelections[playerAddIdx];
        selection.gameObject.SetActive(true);
        selection.SetTagText($"P{playerAddIdx + 1}");
        selection.SetControllerText(player.currentControlScheme);
        // connect player to UI
        var evtSystem = player.GetComponent<MultiplayerEventSystem>();
        evtSystem.playerRoot = selection.gameObject;
        evtSystem.firstSelectedGameObject = selection.GetComponentInChildren<Button>().gameObject;

        playerAddIdx += 1;
    }
}
