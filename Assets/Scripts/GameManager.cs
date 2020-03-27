using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public const float FINAL_DEATH_DELAY = 2f;
    public const float DEATH_SHAKE_DURATION = 0.2f;
    public const float FINAL_DEATH_TIMESCALE = 0.5F;
    public Camera camera;
    public Transform[] spawns;
    public GameObject banner;
    public Text countdown;
    public LivesHUD[] livesUIs;
    public AudioSource startAudio;

    private PlayerController[] players;
    private List<PlayerMapping> playerMappings;

    private float shakeDurationLeft = 0f;
    private Vector3 initialCameraLocation;

    private int numPlayers;

    private void Start() {
        initialCameraLocation = camera.transform.position;
        startAudio.Play();
    }

    private void Update() {
        if (shakeDurationLeft > 0) {
            shakeDurationLeft -= Time.deltaTime;
            if (shakeDurationLeft <= 0) {
                shakeDurationLeft = 0;
                camera.transform.position = initialCameraLocation;
            } else {
                camera.transform.localPosition = initialCameraLocation + UnityEngine.Random.insideUnitSphere;
            }
        }
    }

    protected void OnEnable()
    {
        PlayerController.OnPlayerDeath += OnPlayerDeath;
    }

    protected void OnDisable()
    {
        PlayerController.OnPlayerDeath -= OnPlayerDeath;
    }

    private IEnumerator CountdownCoroutine()
    {
        banner.SetActive(true);
        // Initially disables all controllers
        for (int i = 0; i < numPlayers; i++)
        {
            players[i].DisableControls();
        }
        countdown.text = "3";
        yield return new WaitForSeconds(1);
        countdown.text = "2";
        yield return new WaitForSeconds(1);
        countdown.text = "1";
        yield return new WaitForSeconds(1);
        countdown.text = "FIGHT!";

        for (int i = 0; i < numPlayers; i++)
        {
            players[i].EnableControls();
        }

        yield return new WaitForSeconds(1);
        banner.SetActive(false);
    }

    public void StartGame(List<PlayerMapping> mappings) {
        SpawnPlayers(mappings);
        // hook up players to HUD
        for (int i = 0; i < numPlayers; i++) {
            livesUIs[i].SetPlayer(players[i]);
        }
        StartCoroutine(CountdownCoroutine());
    }

    public void SpawnPlayers(List<PlayerMapping> mappings)
    {
        numPlayers = mappings.Count;
        playerMappings = mappings;
        Debug.Assert(numPlayers <= PlayerManager.MAX_PLAYERS, "unsupported number of players");

        players = new PlayerController[numPlayers];

        for (int i = 0; i < numPlayers; i++)
        {
            var mapping = mappings[i];
            var devices = mapping.Controller;
            var input = PlayerInput.Instantiate(mapping.Character, pairWithDevices: devices);
            var player = input.GetComponent<PlayerController>();
            player.playerIndex = i;
            player.PlayerChoice = mapping;
            player.Spawn(spawns[i].position);
            players[i] = player;
        }
    }

    void OnPlayerDeath(PlayerController player) {
        player.gameObject.SetActive(false);
        MakeBlood(player);
        shakeDurationLeft = DEATH_SHAKE_DURATION;
        if (player.Lives > 0) {
            StartCoroutine(RespawnCoroutine(player));
        } else {
            numPlayers -= 1;
            if (numPlayers == 1) {
                StartCoroutine(EndGameCoroutine());
            }
        }
    }

    private void MakeBlood(PlayerController dyingPlayer) {
        Vector3 newPosition = dyingPlayer.transform.position;
        newPosition.y += 0.3f;
        Instantiate(dyingPlayer.effect, newPosition, Quaternion.identity);
    }

    private IEnumerator EndGameCoroutine() {
        Time.timeScale = FINAL_DEATH_TIMESCALE;
        yield return new WaitForSeconds(FINAL_DEATH_DELAY);
        Time.timeScale = 1;
        DontDestroyOnLoad(this);
        MusicPlayer.Instance.StopMenuMusic();
        SceneManager.LoadScene("EndGame");
    }

    public IEnumerator RespawnCoroutine(PlayerController dyingPlayer) {
        yield return new WaitForSeconds(1f);
        int randomRespawn = UnityEngine.Random.Range(0, spawns.Length);
        dyingPlayer.Spawn(spawns[randomRespawn].position);
        dyingPlayer.gameObject.SetActive(true);
    }

    public string GetWinnerId() {
        // Assumes only one player left at this point
        for (int i = 0; i < players.Length; i++) {
            if (players[i].Alive) {
                return players[i].PlayerChoice.PlayerTag;
            }
        }
        // Shouldn't ever get here
        Debug.LogError("No alive players found at the end of the game!");
        return null;
    }
}
