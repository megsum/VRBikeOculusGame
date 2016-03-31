using UnityEngine;
using UnityEngine.VR;

/// <summary>
/// Disables VR in the scene in which its attached GameObject appears.
/// </summary>
public class DisableVR : MonoBehaviour
{
    private void Awake()
    {
        VRSettings.enabled = false;
    }
}