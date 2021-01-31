using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [UsedImplicitly]
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
