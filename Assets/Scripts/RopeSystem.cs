using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RopeSystem : MonoBehaviour {
    private StateMachine machine;
    private float nextFire = 0;
    private bool ropeAttached;

    public DistanceJoint2D ropeJoint;
    public LineRenderer ropeRenderer;
    public PlayerController playerController;
    public Transform grapplingHookTransform;
    public GrapplingHook grapplingHookPrefab;
    public InputActionMapper actions;
    public bool hitPlatform = false;

    public const float MIN_ROPE_LENGTH = 0.75f;
    public const float FIRE_DELAY = 0.3f;

    private void Start() {
        machine = gameObject.GetComponent<StateMachine>();
    }

    public float RopeDistance { get { return Vector2.Distance(grapplingHookTransform.position, transform.position); } }

    void Awake() {
        ropeRenderer.positionCount = 2;
    }

    void Update() {
        UpdateRope();
    }

    void FixedUpdate() {
        HandleRopeLength();
    }

    private void OnDestroy() {
        if (grapplingHookTransform != null) {
            Destroy(grapplingHookTransform.gameObject);
        }
    }

    public void AttemptShootHook() {
        if (Time.time <= nextFire) return;
        nextFire = Time.time + FIRE_DELAY;
        if (grapplingHookTransform != null) {
            if (!ropeAttached) return; // wait for hook to return
            ResetRope();
        }
        var aimDirection = actions.Aim;
        ShootHook(aimDirection);
    }

    private void UpdateRope() {
        if (grapplingHookTransform != null && ropeRenderer.enabled) {
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, grapplingHookTransform.position);
        }
    }

    private void ShootHook(Vector2 aimDirection) {
        if (grapplingHookTransform != null) return;
        playerController.hookShootAudioSource.Play();
        var hookRotation = Quaternion.AngleAxis(Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg, Vector3.forward);
        var grapplingHookScript = Instantiate(grapplingHookPrefab, transform.position, hookRotation);
        grapplingHookScript.direction = aimDirection;
        grapplingHookScript.ropeSystem = this;
        grapplingHookScript.playerTransform = transform;
        grapplingHookTransform = grapplingHookScript.transform;
        ropeRenderer.enabled = true;
    }

    public void LatchHook(Vector2 hookPoint) {
        playerController.hookLandAudioSource.Play();
        ropeAttached = true;
        ropeJoint.distance = Vector2.Distance(transform.position, hookPoint);
        ropeJoint.connectedAnchor = hookPoint;
        ropeJoint.enabled = true;
        playerController.hookPosition = hookPoint;
        machine.CurrentState = PlayerController.HOOK_PULL_STATE;
        playerController.hookPullAudioSource.Play();
    }

    public void ResetRope() {
        if (grapplingHookTransform == null) return;
        Destroy(grapplingHookTransform.gameObject);
        grapplingHookTransform = null;  // should eventually happen after Destroy, but need it done this call
        ropeJoint.enabled = false;
        ropeAttached = false;
        ropeRenderer.enabled = false;
        ropeRenderer.SetPositions(new Vector3[2]);
        machine.CurrentState = PlayerController.FALL_STATE;
    }

    private void HandleRopeLength() {
        if (ropeAttached) {
            float newRopeDistance = Mathf.Min(RopeDistance, ropeJoint.distance);
            ropeJoint.distance = Mathf.Max(newRopeDistance, MIN_ROPE_LENGTH);
        }
    }

}
