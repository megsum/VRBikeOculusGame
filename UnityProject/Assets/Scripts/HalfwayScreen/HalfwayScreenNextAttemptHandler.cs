using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HalfwayScreenNextAttemptHandler : MonoBehaviour
{
    [SerializeField] public GameObject NextAttempt;

    // Use this for initialization
    private void Start()
    {
        var trial = GameObject.Find("TrialManager").GetComponent<Trial>();
        NextAttempt.GetComponent<Text>().text = trial.SecondAttempt.Type.ToString();
    }

    public void LoadScene(int level)
    {
        SceneManager.LoadScene(level);
    }
}