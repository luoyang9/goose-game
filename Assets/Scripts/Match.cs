using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Match : MonoBehaviour {
    public void OnPlayerDeath() {
        GameObject[] remainingObjects = gameObject.scene.GetRootGameObjects();
        int numPlayersLeft = 0;
        foreach (GameObject gameObject in remainingObjects) {
            PlayerController player = gameObject.GetComponent<PlayerController>();
            if (player != null && player.alive) {
                numPlayersLeft += 1;
            }
        }
        if (numPlayersLeft == 1) {
            SceneManager.LoadScene("EndGame");
        }
     }
}
