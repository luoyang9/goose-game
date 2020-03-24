using UnityEngine;
using TMPro;
using System.Collections;

public class LivesHUD : MonoBehaviour {
    private PlayerController player;
    public TMP_Text livesCount;
    
    private void OnEnable() {
        PlayerController.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable() {
        PlayerController.OnPlayerDeath -= OnPlayerDeath;
    }

    public void SetPlayer(PlayerController p) {
        player = p;
        UpdateHUD(p.Lives);
    }

    private void OnPlayerDeath(PlayerController p) {
        if (player != p) return;

        UpdateHUD(player.Lives);
    }

    private void UpdateHUD(int lives) {
        string[] hearts = new string[player.Lives];
        for (int i = 0; i < hearts.Length; i++) {
            hearts[i] = "<sprite=\"heart\" index=0>";
        }
        livesCount.text = player.PlayerChoice.PlayerTag + ": " + string.Join(" ", hearts);
    }
}
