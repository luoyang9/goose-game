using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RopeSystem : MonoBehaviour {
    private StateMachine machine;
    private float nextFire = 0;
    private bool ropeAttached;
    private List<Vector2> ropePositions = new List<Vector2>();

    public DistanceJoint2D ropeJoint;
    public LineRenderer ropeRenderer;
    public PlayerController playerController;
    public Transform grapplingHookTransform;
    public GrapplingHook grapplingHookPrefab;
    public InputActionMapper actions;
    public bool hitPlatform = false;

    public const float MIN_ROPE_LENGTH = 0.75f;
    public const float FIRE_RATE = 0.15f;

    private void Start() {
        machine = gameObject.GetComponent<StateMachine>();
    }

    public float RopeDistance { get { return Vector2.Distance(grapplingHookTransform.position, transform.position); } }

    void Awake() {
        ropeRenderer.positionCount = 2;
    }

    void Update() {
        UpdateRope();
        HandleInput();
    }

    void FixedUpdate() {
        HandleRopeLength();
    }

    private void HandleInput() {
        if (actions.HookShootPressed() && Time.time > nextFire) {
            nextFire = Time.time + FIRE_RATE;
            if (grapplingHookTransform != null) {
                if (ropeAttached) ResetRope();
            } else {
                var aimDirection = actions.CalculateAim();
                ShootHook(aimDirection);
            }
        }
    }

    private void UpdateRope() {
        if (grapplingHookTransform != null && ropeRenderer.enabled) {
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, grapplingHookTransform.position);
        }
    }

    private void ShootHook(Vector2 aimDirection) {
        if (grapplingHookTransform != null) return;
        // TODO: rotate according to angle
        var hookRotation = Quaternion.identity;
        var grapplingHookScript = Instantiate(grapplingHookPrefab, transform.position, hookRotation);
        grapplingHookScript.direction = aimDirection;
        grapplingHookScript.ropeSystem = this;
        grapplingHookScript.playerTransform = transform;
        grapplingHookTransform = grapplingHookScript.transform;
        ropeRenderer.enabled = true;
    }

    public void LatchHook(Vector2 hookPoint) {
        ropeAttached = true;
        ropeJoint.distance = Vector2.Distance(transform.position, hookPoint);
        ropeJoint.connectedAnchor = hookPoint;
        ropeJoint.enabled = true;
        playerController.hookPosition = hookPoint;
        machine.CurrentState = PlayerController.HookPullState;
    }

    public void ResetRope() {
        if (grapplingHookTransform == null) return;
        Destroy(grapplingHookTransform.gameObject);
        ropeJoint.enabled = false;
        ropeAttached = false;
        ropeRenderer.enabled = false;
        ropeRenderer.SetPositions(new Vector3[2]);
        machine.CurrentState = PlayerController.FallState;
    }

    private void HandleRopeLength() {
        if (ropeAttached) {
            float newRopeDistance = Mathf.Min(RopeDistance, ropeJoint.distance);
            ropeJoint.distance = Mathf.Max(newRopeDistance, MIN_ROPE_LENGTH);
        }
    }

}
