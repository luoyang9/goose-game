using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour {
    public Text text;
    public void Start() {
        GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        text.text = manager.GetWinnerId() + " won";
        Destroy(manager.gameObject);
    }
    public void OpenMenu() {
        SceneManager.LoadScene("PlayerSelection");
    }
}
