using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour {
    public enum AttackDirection { UP, DOWN, FORWARD }

    private struct AttackParams {
        public Vector2 position;
        public float rotation;

        public AttackParams(
            Vector2 pos,
            float rot
        ) {
            position = pos;
            rotation = rot;
        }
    }

    public const float ATTACK_DURATION = 0.1f; // seconds

    private AttackParams UP_ATTACK;
    private AttackParams DOWN_ATTACK;
    private AttackParams FORWARD_ATTACK;

    public Animator animator;

    public PlayerController player;
    public SpriteRenderer weaponSpriteRenderer;
    public BoxCollider2D weaponCollider; // disjoint hitbox
    private int playerLayer;

    private float attackTimer = 0;

    void Start() {
        playerLayer = LayerMask.NameToLayer("Player");

        UP_ATTACK = new AttackParams(
            new Vector2(0, 1.6f),
            90
        );
        DOWN_ATTACK = new AttackParams(
            new Vector2(0, -1.6f),
            -90
        );
        FORWARD_ATTACK = new AttackParams(
            new Vector2(1.6f, 0),
            0
        );
    }

    void FixedUpdate() {
        if (attackTimer > 0) {
            attackTimer -= Time.deltaTime;
        } else {
            AttackDisable();
        }
    }

    public void Attack(AttackDirection dir) {
        AttackReset();
        // change hitbox and sprites
        switch (dir) {
            case AttackDirection.UP:
                ApplyAttackParams(UP_ATTACK);
                break;
            case AttackDirection.DOWN:
                ApplyAttackParams(DOWN_ATTACK);
                break;
            case AttackDirection.FORWARD:
                ApplyAttackParams(FORWARD_ATTACK);
                break;
        }
        animator.SetTrigger("Attack");
        AttackEnable();
    }

    private void ApplyAttackParams(AttackParams attack) {
        transform.localPosition = attack.position;
        transform.localEulerAngles = new Vector3(0, 0, attack.rotation);
    }

    private void AttackReset() {
        attackTimer = ATTACK_DURATION;
    }

    private void AttackEnable() {
        weaponCollider.enabled = true;
    }

    private void AttackDisable() {
        if (!weaponCollider.enabled) return;
        weaponCollider.enabled = false;
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
