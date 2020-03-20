using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundCheck : MonoBehaviour {
    private int platformLayer;
    private ISet<int> touchingLayers;

    public bool TouchingGround { get { return touchingLayers.Count > 0; } }

    void Awake() {
        platformLayer = LayerMask.NameToLayer("Platform");

        touchingLayers = new HashSet<int>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        touchingLayers.Add(collision.gameObject.layer);
    }

    void OnTriggerExit2D(Collider2D collision) {
        touchingLayers.Remove(collision.gameObject.layer);
    }

    public bool isTouchingPlatform() {
        return touchingLayers.Contains(platformLayer);
    }
}
