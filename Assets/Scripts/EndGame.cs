using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour {
    public void Start() {
        GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        TMPro.TextMeshProUGUI text = GameObject.Find("Winner").GetComponent<TMPro.TextMeshProUGUI>();
        text.SetText("Player " + (manager.GetWinnerId() + 1) + " won");
    }
    public void OpenMenu() {
        SceneManager.LoadScene("PlayerSelection");
    }
}
