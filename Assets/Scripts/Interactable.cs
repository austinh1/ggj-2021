using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public AnimationCurve bounceCurve;
    public bool bounce = false;
    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered");
        if (other.tag == "Player")
        {
            bounce = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Something exited");
        if (other.tag == "Player")
        {
            bounce = false;
        }
    }

    void Update()
    {
        if (bounce || transform.position.y != startY)
        {
            transform.position = new Vector3(transform.position.x, bounceCurve.Evaluate((Time.time % bounceCurve.length)), transform.position.z);
        }
    }
}
