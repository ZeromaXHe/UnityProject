using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BallJump : MonoBehaviour
{
    private Rigidbody2D rb2D;

    public float jumpForce = 3.0f;
    public float jumpHoldForce = 0.1f;

    public float moveSpeed = 6;
    public LayerMask ground;

    private float xMovement;
    private float jumpTime = 0.0f;
    private bool isGround;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        xMovement = Input.GetAxis("Horizontal");
        rb2D.velocity = new Vector2(xMovement * moveSpeed, rb2D.velocity.y);
        isGround = rb2D.IsTouchingLayers(ground);

        if (isGround && Input.GetKeyDown(KeyCode.Space))
        {
            jumpTime = Time.time + 0.2f;
            rb2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }

        if (Input.GetKey(KeyCode.Space) && !isGround && Time.time < jumpTime)
        {
            rb2D.AddForce(new Vector2(0, jumpHoldForce), ForceMode2D.Impulse);
        }
    }
}