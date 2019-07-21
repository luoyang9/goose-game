using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RopeSystem : MonoBehaviour {
    private StateMachine machine;
    public DistanceJoint2D ropeJoint;
    private bool ropeAttached;
    public LineRenderer ropeRenderer;
    private List<Vector2> ropePositions = new List<Vector2>();
    public PlayerController playerController;
    public const float MIN_ROPE_LENGTH = 0.75f;
    
    public Transform grapplingHookTransform;
    public GrapplingHook grapplingHookPrefab;

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

    /**
     * returns unit vector of aimed direction from player
     */
    private Vector2 CalculateAim() {
        var worldMousePosition =
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
        var aimDirection = ((Vector2)worldMousePosition - (Vector2)transform.position).normalized;
        return aimDirection;
    }

    private void HandleInput() {
        if (Input.GetButtonDown("Fire1")) {
            if (grapplingHookTransform != null) {
                if (ropeAttached) ResetRope();
            } else {
                var aimDirection = CalculateAim();
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
        machine.SwitchState<HookPullState>();
    }

    public void ResetRope() {
        if (grapplingHookTransform == null) return;
        Destroy(grapplingHookTransform.gameObject);
        ropeJoint.enabled = false;
        ropeAttached = false;
        ropeRenderer.enabled = false;
        ropeRenderer.SetPositions(new Vector3[2]);
        machine.SwitchState<FallState>();
    }

    private void HandleRopeLength() {
        if (ropeAttached) {
            float newRopeDistance = Mathf.Min(RopeDistance, ropeJoint.distance);
            ropeJoint.distance = Mathf.Max(newRopeDistance, MIN_ROPE_LENGTH);
        }
    }

}
