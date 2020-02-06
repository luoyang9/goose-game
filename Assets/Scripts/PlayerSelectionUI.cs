using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// todo: separate UI and logic
public class PlayerSelectionUI : MonoBehaviour {
    private Color TAG_ACTIVE = new Color(1f, 0.365f, 0.365f);
    private Color TAG_INACTIVE = new Color(0.755f, 0.755f, 0.755f);

    public Image playerTag;
    public Image splash;
    public Image characterFg;
    public GameObject foreground;
    public GameObject banner;
    public GameObject idle;
    public GameObject selectionUI;
    public Text controller;

    private PlayerSelection _selection;
    public PlayerSelection Selection {
        get { return _selection; }
        set {
            _selection = value;
            Selection.CharacterChanged += UpdateCharacterSprites;
            Selection.StateChanged += OnChangeState;
            SetTagText(Selection.PlayerName);

            // when a player joins, remove join prompt
            idle.SetActive(false);
            selectionUI.SetActive(true);
        }
    }

    public void OnChangeState(int state) {
        // when a player joins, remove join prompt
        idle.SetActive(false);
        selectionUI.SetActive(true);

        var selectionActive = state != PlayerSelection.INACTIVE_STATE;
        SetActivate(selectionActive);
        banner.SetActive(state == PlayerSelection.READY_STATE);
    }

    public void SetTagText(string text)
    {
        var tagText = playerTag.GetComponentInChildren<Text>();
        tagText.text = text;
    }

    public void SetControllerText(string text)
    {
        controller.text = text;
    }

    private void UpdateCharacterSprites(CharacterSelection selection)
    {
        splash.sprite = selection.splash;
        characterFg.sprite = selection.splash;
    }

    private void SetActivate(bool active) {
        var tagColor = active ? TAG_ACTIVE : TAG_INACTIVE;
        playerTag.color = tagColor;
        foreground.SetActive(active);
        splash.enabled = active;
    }
}
