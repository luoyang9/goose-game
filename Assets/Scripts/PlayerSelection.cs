using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// todo: separate UI and logic
public class PlayerSelection: MonoBehaviour {
    public const int INACTIVE_STATE = 0;
    public const int PENDING_STATE = 1;
    public const int READY_STATE = 2;

    private int _state = PENDING_STATE;
    public int State {
        get { return _state; }
        private set {
            _state = value;
            StateChanged?.Invoke(_state);
        }
    }

    public delegate void StateChangedAction(int s);
    public event StateChangedAction StateChanged;
    public delegate void CharacterChangedAction(CharacterSelection character);
    public event CharacterChangedAction CharacterChanged;

    public PlayerManager Manager { get; set; }
    public InputDevice[] Controller { get; set; }

    public string PlayerName { get; set; }
    public CharacterSelection[] roster;

    private int selectedCharacter;
    private int SelectedCharacter {
        get { return selectedCharacter; }
        set {
            selectedCharacter = value;
            CharacterChanged?.Invoke(roster[selectedCharacter]);
        }
    }

    private void Start() {
        SelectedCharacter = 0;
    }

    public void OnSubmit() {
        switch (State) {
            case INACTIVE_STATE:
                Manager.joinSource.Play();
                State = PENDING_STATE;
                break;
            case PENDING_STATE:
                State = READY_STATE;
                break;
            case READY_STATE:
                Manager.OnStartFight();
                break;
            default:
                break;
        }
    }

    public void OnCancel() {
        switch (State) {
            case PENDING_STATE:
                Manager.leaveSource.Play();
                State = INACTIVE_STATE;
                break;
            case READY_STATE:
                State = PENDING_STATE;
                break;
            default:
                break;
        }
    }

    public void ChangeCharacter(int direction) {
        if (direction != -1 && direction != 1) return;

        SelectedCharacter = (SelectedCharacter + direction + roster.Length) % roster.Length;
    }

    public PlayerMapping GetMapping() {
        return new PlayerMapping(PlayerName, Controller, roster[SelectedCharacter].prefab);
    }
}
