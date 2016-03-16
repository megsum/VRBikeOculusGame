using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A smaller helper object that loads the Standard UI from the Standard_UI Scene when loaded.
/// </summary>
public class StandardUILoader : MonoBehaviour
{
    void Awake()
    {
        SceneManager.LoadScene("Standard_UI", LoadSceneMode.Additive);
    }
}
