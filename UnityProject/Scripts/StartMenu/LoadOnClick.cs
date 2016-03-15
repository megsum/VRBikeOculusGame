using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.StartMenu
{
    public class LoadOnClick : MonoBehaviour
    {
        public void LoadScene()
        {
            SceneManager.LoadScene("");
        }
    }
}