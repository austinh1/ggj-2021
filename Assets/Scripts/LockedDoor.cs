using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    public int NumRequiredKeys = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // After player obtains all keys, they can open the door
    void OnCollisionEnter2D(Collision2D Collision2D)
    {
        if (Collision2D.gameObject.tag == "Player" && NumRequiredKeys <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
