using System.Linq;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T instance
    {
        get
        {
            if (!_instance)
            {
                var foundObjects = Resources.FindObjectsOfTypeAll<T>();
                if (foundObjects.Length > 0)
                {
                    _instance = foundObjects.FirstOrDefault(foundObject => foundObject.hideFlags == HideFlags.None);
                }
                else
                {
                    Debug.LogError($"Couldn't find singleton {typeof(T)}");
                }
            }

            return _instance;
        }
    }
}