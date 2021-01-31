using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSlappedEnd : MonoBehaviour
{
    public void AnimationEnd()
    {
        var controller = GetComponentInParent<PlayerController>();
        controller.MakeIntoHuman();
    }
}
