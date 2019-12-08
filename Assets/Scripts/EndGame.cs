using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    // TODO: Identify the winner once we decide on how we're identifying them
    // Eg. by name or player number
    public void OpenMenu() {
        SceneManager.LoadScene("PlayerSelection");
    }
}
