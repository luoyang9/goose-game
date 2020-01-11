using UnityEngine;
using System.Collections;

public class NewBehaviourScript1 : MonoBehaviour {
    public GameObject arrowPrefab;

    private void Awake() {
        InvokeRepeating("Shoot", 2, 2);
    }

    void Shoot() {
        Vector2 aimDirection = Vector2.down;
        var arrowStartPos = (Vector2)transform.position + aimDirection * 1f;
        Instantiate(arrowPrefab, arrowStartPos, Quaternion.identity);
    }
}
