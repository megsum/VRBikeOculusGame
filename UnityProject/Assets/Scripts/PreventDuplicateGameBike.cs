using UnityEngine;

// Source: http://forum.unity3d.com/threads/dontdestroyonload-leading-to-double-objects.58609/ (2016-03-28)
public class PreventDuplicateGameBike : MonoBehaviour
{
    private static PreventDuplicateGameBike _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}