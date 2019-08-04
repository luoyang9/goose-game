using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour {
    public const float ATTACK_DURATION = 0.2f; // seconds
    private const float ATTACK_ANGLE = 120f; // degrees
    private const float START_ROTATION = 0; // degrees measured from straight up

    public PlayerController player;
    public SpriteRenderer weaponSprite;
    public Collider2D weaponCollider;
    private int playerLayer;

    private float attackTimer = 0;
    private float attackRotateRate = 0;

    void Start() {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    void Update() {
        if (attackTimer <= 0) {
            AttackDisable();
        }
    }

    void FixedUpdate() {
        if (attackTimer > 0) {
            transform.RotateAround(transform.parent.transform.position, Vector3.back, attackRotateRate * Time.deltaTime);
            attackTimer -= Time.deltaTime;
        }
    }

    public void Attack() {
        AttackReset();
        AttackEnable();
    }

    private void AttackReset() {
        attackTimer = ATTACK_DURATION;
        var rotateSpeed = ATTACK_ANGLE / ATTACK_DURATION;
        attackRotateRate = (player.Facing > 0) ? rotateSpeed : -rotateSpeed;
        weaponSprite.flipX = player.Facing < 0;
        var rot = Quaternion.Euler(new Vector3(0, 0, START_ROTATION));
        var pos = transform.parent.position + Vector3.up;
        transform.SetPositionAndRotation(pos, rot);
    }

    private void AttackEnable() {
        weaponCollider.enabled = true;
        weaponSprite.enabled = true;
    }

    private void AttackDisable() {
        if (!weaponCollider.enabled) return;
        weaponCollider.enabled = false;
        weaponSprite.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.layer == playerLayer) {
            var enemy = collider.GetComponent<PlayerController>();
            if (enemy != null && player != enemy) {
                enemy.Kill();
            }
        }
    }
}
