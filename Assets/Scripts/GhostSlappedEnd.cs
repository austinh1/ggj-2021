using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class GhostSlappedEnd : MonoBehaviour
{
    [UsedImplicitly]
    public void AnimationEnd()
    {
        var controller = GetComponentInParent<NetworkPlayer>();
        controller.MakeIntoHuman();
    }
}
