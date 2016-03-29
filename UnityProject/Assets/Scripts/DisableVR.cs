using UnityEngine;
using UnityEngine.VR;

public class DisableVR : MonoBehaviour
{
    private void Awake()
    {
        VRSettings.enabled = false;
    }
}