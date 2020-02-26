using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour {
    public enum AttackDirection { UP, DOWN, FORWARD }

    private struct AttackParams {
        public Vector2 position;
        public Vector2 hitboxSize;
        public Vector2 hitboxOffset;
        public Sprite sprite;
        public bool spriteFlipY;

        public AttackParams(
            Vector2 pos,
            Vector2 hbSize,
            Vector2 hbOffset,
            Sprite s,
            bool flipY
        ) {
            position = pos;
            hitboxSize = hbSize;
            hitboxOffset = hbOffset;
            sprite = s;
            spriteFlipY = flipY;
        }
    }

    public const float ATTACK_DURATION = 0.1f; // seconds

    private AttackParams UP_ATTACK;
    private AttackParams DOWN_ATTACK;
    private AttackParams FORWARD_ATTACK;

    public Sprite upAttackSprite;
    public Sprite forwardAttackSprite;

    public PlayerController player;
    public SpriteRenderer weaponSpriteRenderer;
    public BoxCollider2D weaponCollider; // disjoint hitbox
    private int playerLayer;

    private float attackTimer = 0;

    void Start() {
        playerLayer = LayerMask.NameToLayer("Player");

        UP_ATTACK = new AttackParams(
            new Vector2(0, 1.2f),
            new Vector2(0.7f, 1.1f),
            new Vector2(0, 0.4f),
            upAttackSprite,
            false
        );
        DOWN_ATTACK = new AttackParams(
            new Vector2(0, -1.2f),
            new Vector2(0.7f, 1.1f),
            new Vector2(0, -0.4f),
            upAttackSprite,
            true
        );
        FORWARD_ATTACK = new AttackParams(
            new Vector2(1.2f, 0),
            new Vector2(1.1f, 0.7f),
            new Vector2(0.4f, 0),
            forwardAttackSprite,
            false
        );
    }

    void Update() {
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
        AttackEnable();
    }

    private void ApplyAttackParams(AttackParams attack) {
        weaponSpriteRenderer.sprite = attack.sprite;
        weaponSpriteRenderer.flipY = attack.spriteFlipY;
        transform.localPosition = attack.position;
        weaponCollider.size = attack.hitboxSize;
        weaponCollider.offset = attack.hitboxOffset;
    }

    private void AttackReset() {
        attackTimer = ATTACK_DURATION;
    }

    private void AttackEnable() {
        weaponCollider.enabled = true;
        weaponSpriteRenderer.enabled = true;
    }

    private void AttackDisable() {
        if (!weaponCollider.enabled) return;
        weaponCollider.enabled = false;
        weaponSpriteRenderer.enabled = false;
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
