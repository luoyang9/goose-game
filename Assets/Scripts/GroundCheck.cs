using UnityEngine;
using System.Collections;

public class GroundCheck : MonoBehaviour {
    // Layer of the ground the player is on; -1 if not on the ground
    private int TouchingLayer { get; set; }
    private int noLayer, wallLayer, platformLayer;

    void Awake() {
        TouchingLayer = -1;
        noLayer = -1;
        wallLayer = LayerMask.NameToLayer("Wall");
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    void OnTriggerEnter2D(Collider2D collision) {
        int layer = collision.gameObject.layer;
        if (layer == wallLayer || layer == platformLayer) {
            TouchingLayer = layer;
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        int layer = collision.gameObject.layer;
        if (layer == wallLayer || layer == platformLayer) {
            TouchingLayer = noLayer;
        }
    }

    public bool isTouchingPlatform() {
        return TouchingLayer == platformLayer;
    }
}
