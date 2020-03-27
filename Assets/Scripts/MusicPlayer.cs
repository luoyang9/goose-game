using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour {
    private static MusicPlayer _instance;

    public static MusicPlayer Instance {  get { return _instance; } }

    // Music
    public AudioSource menuMusic;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void PlayMenuMusic() {
        if (!menuMusic.isPlaying) {
            menuMusic.Play();
        }
    }

    public void StopMenuMusic() {
        if (menuMusic.isPlaying) {
            menuMusic.Stop();
        }
    }
}
