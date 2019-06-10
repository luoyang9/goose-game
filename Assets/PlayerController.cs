using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public const float GRAVITY_ACCELERATION = 3;
    public const float MOVEMENT_ACCELERATION = 5;
    public const float MAX_MOVEMENT_SPEED = 10;
    public const float MAX_FALL = 15;

    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

    }

    void FixedUpdate()
    {
        // Default acceleration (with gravity)
        Vector2 acceleration = new Vector2(0, -GRAVITY_ACCELERATION);
        if (Input.GetKey(KeyCode.A))
        {
            acceleration += Vector2.left * MOVEMENT_ACCELERATION;
        }
        if (Input.GetKey(KeyCode.D))
        {
            acceleration += Vector2.right * MOVEMENT_ACCELERATION;
        }
        if (Input.GetKey(KeyCode.W))
        {
            acceleration += Vector2.up * MOVEMENT_ACCELERATION;
        }
        if (Input.GetKey(KeyCode.S))
        {
            acceleration += Vector2.down * MOVEMENT_ACCELERATION;
        }
        Vector2 velocity = rb.velocity + acceleration;
        velocity.x = Mathf.Min(Mathf.Max(velocity.x, -MAX_MOVEMENT_SPEED), MAX_MOVEMENT_SPEED);
        velocity.y = Mathf.Min(Mathf.Max(velocity.y, -MAX_MOVEMENT_SPEED), MAX_FALL);
        rb.velocity = velocity;
    }
}
