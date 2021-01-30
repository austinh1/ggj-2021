using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private bool PickedUp = false;
    public LockedDoor LockedD1;

    // Start is called before the first frame update
    void Start()
    {
        GameObject test = GameObject.Find("LockedDoor");
        LockedDoor LockedDoor = test.GetComponent<LockedDoor>();
        LockedD1 = LockedDoor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Removes the key when the player picks it up
    void OnTriggerEnter2D(Collider2D Collider2D)
    {
        if (Collider2D.gameObject.tag == "Player")
        {
            PickedUp = true;
            LockedD1.NumRequiredKeys -= 1;
            Destroy(this.gameObject);
        }
    }
}