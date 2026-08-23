using System.Xml.Serialization;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
