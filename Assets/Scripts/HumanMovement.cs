using System;
using UnityEngine;

public class HumanMovement : MonoBehaviour, IPlayerMovement
{
    private Rigidbody2D rigidbody2D;

    [Range(0.0f, 10.0f)]
    public float speed = 7f;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        rigidbody2D.velocity = new Vector2(speed * inputX, speed * inputY);
    }

    public void SetEnabled(bool value)
    {
        enabled = value;
    }
}