using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class RopeSystem : MonoBehaviour {
    public GameObject ropeHingeAnchor;
    public DistanceJoint2D ropeJoint;
    private bool ropeAttached;
    private Vector2 playerPosition;
    private Rigidbody2D ropeHingeAnchorRb;
    private SpriteRenderer ropeHingeAnchorSprite;
    public LineRenderer ropeRenderer;
    public LayerMask ropeLayerMask;
    private float ropeMaxCastDistance = 20f;
    private List<Vector2> ropePositions = new List<Vector2>();
    private bool distanceSet;
    public PlayerMovement playerMovement;
    public float climbSpeed = 30f;
    private bool isColliding;

    void Awake() {
        // 2
        playerPosition = transform.position;
        ropeHingeAnchorRb = ropeHingeAnchor.GetComponent<Rigidbody2D>();
        ropeHingeAnchorSprite = ropeHingeAnchor.GetComponent<SpriteRenderer>();
    }

    void Update() {
        // 3
        var worldMousePosition =
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
        var facingDirection = worldMousePosition - transform.position;
        var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
        if (aimAngle < 0f) {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        // 4
        var aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;
        // 5
        playerPosition = transform.position;

        // 6
        if (ropeAttached) {
            playerMovement.isSwinging = true;
            playerMovement.ropeHook = ropePositions.Last();
        } else {
            playerMovement.isSwinging = false;
        }
        HandleInput(aimDirection);
        UpdateRopePositions();
        HandleRopeLength();

    }

    private void HandleInput(Vector2 aimDirection) {
        if (Input.GetMouseButton(0)) {
            // 2
            if (ropeAttached) return;
            ropeRenderer.enabled = true;

            var hit = Physics2D.Raycast(playerPosition, aimDirection, Mathf.Infinity, ropeLayerMask);

            // 3
            if (hit.collider != null) {
                ropeAttached = true;
                if (!ropePositions.Contains(hit.point)) {
                    // 4
                    // Jump slightly to distance the player a little from the ground after grappling to something.
                    transform.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 2f), ForceMode2D.Impulse);
                    ropePositions.Add(hit.point);
                    ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);
                    ropeJoint.enabled = true;
                    ropeHingeAnchorSprite.enabled = true;
                }
            }
            // 5
            else {
                ropeRenderer.enabled = false;
                ropeAttached = false;
                ropeJoint.enabled = false;
            }
        }

        if (Input.GetMouseButton(1)) {
            ResetRope();
        }
    }

    // 6
    private void ResetRope() {
        ropeJoint.enabled = false;
        ropeAttached = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        ropeHingeAnchorSprite.enabled = false;
        playerMovement.isSwinging = false;
    }

    private void UpdateRopePositions() {
        // 1
        if (!ropeAttached) {
            return;
        }

        // 2
        ropeRenderer.positionCount = ropePositions.Count + 1;

        // 3
        for (var i = ropeRenderer.positionCount - 1; i >= 0; i--) {
            if (i != ropeRenderer.positionCount - 1) // if not the Last point of line renderer
            {
                ropeRenderer.SetPosition(i, ropePositions[i]);

                // 4
                if (i == ropePositions.Count - 1 || ropePositions.Count == 1) {
                    var ropePosition = ropePositions[ropePositions.Count - 1];
                    if (ropePositions.Count == 1) {
                        ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet) {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    } else {
                        ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet) {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                // 5
                else if (i - 1 == ropePositions.IndexOf(ropePositions.Last())) {
                    var ropePosition = ropePositions.Last();
                    ropeHingeAnchorRb.transform.position = ropePosition;
                    if (!distanceSet) {
                        ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                        distanceSet = true;
                    }
                }
            } else {
                // 6
                ropeRenderer.SetPosition(i, transform.position);
            }
        }
    }

    private void HandleRopeLength() {
        // 1
        float verticalInput = Input.GetAxis("Vertical");
        if (verticalInput > 0 && ropeAttached) {
            float newRopeDistance = ropeJoint.distance - Time.deltaTime * climbSpeed;
            ropeJoint.distance = Mathf.Max(newRopeDistance, 0.5f);
        } else if (verticalInput < 0 && ropeAttached) {
            ropeJoint.distance += Time.deltaTime * climbSpeed;
        }
    }

    void OnTriggerStay2D(Collider2D colliderStay) {
        isColliding = true;
    }

    private void OnTriggerExit2D(Collider2D colliderOnExit) {
        isColliding = false;
    }

}
