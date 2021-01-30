using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PossessionManager : MonoBehaviourSingleton<PossessionManager>
{
    public List<PossessionObject> PossessionObjects;
    public GameObject PossessionObjectParent;

    public void Start()
    {
        PossessionObjects = PossessionObjectParent.GetComponentsInChildren<PossessionObject>().ToList();
    }
}
