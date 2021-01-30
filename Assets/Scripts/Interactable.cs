using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public AnimationCurve bounceCurve;
    public float bounceSpeed = 5.0f;
    public float bounceDistance = 0.3f;
    
    private bool bounce = false;
    private float bouncePos = 0f;
    private float prevBouncePos = 0f;
    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            bounce = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            bounce = false;
        }
    }

    void Update()
    {
        // Finish the bounce animation if it's already going
        if (bounce || bouncePos != 0)
        {
            float diff = Time.deltaTime * bounceSpeed;
            bouncePos = (bouncePos + diff) % bounceCurve.length;

            // Stop the bounce if it would ping-pong at the end of the curve and the trigger is not active
            if (!bounce && diff >= prevBouncePos)
            {
                bouncePos = 0;
            }

            transform.position = new Vector3(transform.position.x, startY + bounceCurve.Evaluate(bouncePos) * bounceDistance, transform.position.z);

            prevBouncePos = bouncePos;
        }
    }
}
