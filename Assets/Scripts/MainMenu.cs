using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() {
        SceneManager.LoadScene("PlayerSelection");
    }

    public void StartTutorial() {
        SceneManager.LoadScene("Tutorial");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
