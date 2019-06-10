using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public const float GRAVITY_ACCELERATION = 3;
    public const float JUMP_VELOCITY = 40;
    public const float MOVEMENT_ACCELERATION = 5;
    public const float MAX_MOVEMENT_SPEED = 20;
    public const float MAX_FALL = 40;

    public Rigidbody2D rb;

    private bool onGround = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0)
        {
            onGround = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        onGround = false;
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
        if (Input.GetKey(KeyCode.S))
        {
            acceleration += Vector2.down * MOVEMENT_ACCELERATION;
        }
        Vector2 velocity = rb.velocity + acceleration;
        velocity.x = Mathf.Min(Mathf.Max(velocity.x, -MAX_MOVEMENT_SPEED), MAX_MOVEMENT_SPEED);
        velocity.y = Mathf.Max(velocity.y, -MAX_FALL);
        if (Input.GetKey(KeyCode.W) && onGround) // Jump
        {
            velocity.y = JUMP_VELOCITY;
        }
        rb.velocity = velocity;
    }
}
