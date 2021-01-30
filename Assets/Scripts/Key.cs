using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public LockedDoor LockedD1;

    // Removes the key when the player picks it up
    void OnTriggerEnter2D(Collider2D Collider2D)
    {
        if (Collider2D.gameObject.tag == "Player")
        {
            LockedD1.NumRequiredKeys -= 1;
            Destroy(this.gameObject);
        }
    }
}