using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    private int arrowLayer;
    private InputActionMapper input;

    private void Awake() {
        arrowLayer = LayerMask.NameToLayer("Arrow");
        input = GetComponentInParent<InputActionMapper>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == arrowLayer) {
            var arrow = collision.GetComponent<Arrow>();
            var direction = input.Aim;
            var arrowStartPos = (Vector2)transform.position + direction * PlayerController.ARROW_START_DIST;
            arrow.transform.position = arrowStartPos;
            arrow.direction = direction;
            arrow.rbody.velocity = direction * Arrow.ARROW_SPEED;
        }
    }
}
