using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelection : MonoBehaviour
{
    public delegate void StatusChangedAction(bool active);
    public event StatusChangedAction OnSelectionStatusChange;
    public delegate void CharacterChangedAction(GameObject character);
    public event CharacterChangedAction OnCharacterChanged;

    public Button playerTag;
    public Button leftChange;
    public Button rightChange;
    public Image splash;
    public Image character;
    public GameObject foreground;
    public CharacterSelection[] roster;

    private int selectedCharacter;
    private int SelectedCharacter {
        get { return selectedCharacter; }
        set {
            selectedCharacter = value;
            OnCharacterChanged?.Invoke(roster[selectedCharacter].prefab);
            UpdateCharacterSprites(selectedCharacter);
        }
    }

    private bool selectionActive;

    private void Awake()
    {
        selectionActive = true;
        SelectedCharacter = 0;
    }

    public void OnEnable()
    {
        playerTag.onClick.AddListener(OnTagClicked);
        leftChange.onClick.AddListener(OnLeftChange);
        rightChange.onClick.AddListener(OnRightChange);
    }

    public void OnDisable()
    {
        playerTag.onClick.RemoveListener(OnTagClicked);
        leftChange.onClick.RemoveListener(OnLeftChange);
        rightChange.onClick.RemoveListener(OnRightChange);
    }

    private void OnTagClicked()
    {
        selectionActive = !selectionActive;

        foreground.SetActive(selectionActive);
        splash.enabled = selectionActive;
        OnSelectionStatusChange?.Invoke(selectionActive);
    }

    public void OnLeftChange()
    {
        SelectedCharacter = (SelectedCharacter - 1 + roster.Length) % roster.Length;
    }

    public void OnRightChange()
    {
        SelectedCharacter = (SelectedCharacter + 1) % roster.Length;
    }

    private void UpdateCharacterSprites(int i)
    {
        splash.sprite = roster[i].splash;
        character.sprite = roster[i].splash;
    }
}
