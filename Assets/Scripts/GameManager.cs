using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public Transform[] spawns;

    public void SpawnPlayers(List<PlayerMapping> mappings)
    {
        Debug.Assert(mappings.Count <= PlayerManager.MAX_PLAYERS, "unsupported number of players");

        for (int i = 0; i < mappings.Count; i++)
        {
            var devices = mappings[i].Controller;
            var input = PlayerInput.Instantiate(mappings[i].Character, pairWithDevices: devices);
            input.transform.position = spawns[i].position;
        }
    }
}
