using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImagePool : MonoBehaviour
{
    public GameObject afterImagePrefab;

    public PlayerController attachedPlayer;

    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    private void Awake()
    {
    }


}
