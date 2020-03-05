using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour {
    public TMPro.TextMeshProUGUI text;
    public void Start() {
        GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        text.SetText(manager.GetWinnerId() + " won");
        Destroy(manager.gameObject);
    }
    public void OpenMenu() {
        SceneManager.LoadScene("PlayerSelection");
    }
}
