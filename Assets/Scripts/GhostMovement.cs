using System;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    private Rigidbody2D rigidbody2D;
    public Vector2 speed = new Vector2(10, 10);

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        rigidbody2D.velocity = new Vector2(speed.x * inputX, speed.y * inputY);
    }
}
