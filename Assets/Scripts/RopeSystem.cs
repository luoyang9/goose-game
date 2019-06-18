using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RopeSystem : MonoBehaviour {
    public DistanceJoint2D ropeJoint;
    private bool ropeAttached;
    public LineRenderer ropeRenderer;
    private float ropeMaxCastDistance = 20f;
    private List<Vector2> ropePositions = new List<Vector2>();
    public PlayerMovement playerMovement;
    public float climbSpeed = 10f;
    
    public Transform grapplingHookTransform;
    public GrapplingHook grapplingHookPrefab;

    void Awake() {
        ropeRenderer.positionCount = 2;
    }

    void Update() {
        UpdateRope();
        HandleInput();
        HandleRopeLength();

    }

    /**
     * returns unit vector of aimed direction from player
     */
    private Vector2 calculateAim() {
        var worldMousePosition =
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
        var aimDirection = (Vector2)(worldMousePosition - transform.position).normalized;
        
        return aimDirection;
    }

    private void HandleInput() {
        if (Input.GetButtonDown("Fire1")) {
            var aimDirection = calculateAim();
            ShootHook(aimDirection);
        }

        if (Input.GetButtonDown("Fire2")) {
            ResetRope();
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
        grapplingHookTransform = grapplingHookScript.transform;
        ropeRenderer.enabled = true;
    }

    public void LatchHook(Vector2 hookPoint) {
        ropeAttached = true;
        ropeJoint.distance = Vector2.Distance(transform.position, hookPoint);
        ropeJoint.connectedAnchor = hookPoint;
        ropeJoint.enabled = true;
        playerMovement.hookPosition = hookPoint;
        playerMovement.isSwinging = true;
    }

    private void ResetRope() {
        if (grapplingHookTransform == null) return;
        Destroy(grapplingHookTransform.gameObject);
        ropeJoint.enabled = false;
        ropeAttached = false;
        ropeRenderer.enabled = false;
        ropeRenderer.SetPositions(new Vector3[2]);
        playerMovement.isSwinging = false;
    }

    private void HandleRopeLength() {
        float verticalInput = Input.GetAxis("Vertical");
        if (verticalInput > 0 && ropeAttached) {
            float newRopeDistance = ropeJoint.distance - Time.deltaTime * climbSpeed;
            ropeJoint.distance = Mathf.Max(newRopeDistance, 0.5f);
        } else if (verticalInput < 0 && ropeAttached) {
            ropeJoint.distance += Time.deltaTime * climbSpeed;
        }
    }

}
