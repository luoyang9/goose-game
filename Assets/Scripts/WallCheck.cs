using UnityEngine;
using System.Collections;

public class WallCheck : MonoBehaviour {
    public int dir;
    public bool Touching { get; private set; }

    private int wallLayer;

    void Awake() {
        Touching = false;
        wallLayer = LayerMask.NameToLayer("Wall");
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == wallLayer) {
            // wall
            Touching = true;
            Debug.Log("Touching");
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == wallLayer) {
            // wall
            Touching = false;
            Debug.Log("Not Touching");
        }
    }
}
