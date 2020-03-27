using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start() {
        MusicPlayer.Instance.PlayMenuMusic();
    }

    public void PlayGame() {
        SceneManager.LoadScene("PlayerSelection");
    }

    public void StartTutorial() {
        MusicPlayer.Instance.StopMenuMusic();
        SceneManager.LoadScene("Tutorial");
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void Credits() {
        SceneManager.LoadScene("Credits");
    }
}
