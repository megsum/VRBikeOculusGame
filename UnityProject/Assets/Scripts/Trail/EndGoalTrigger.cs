using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndGoalTrigger : MonoBehaviour {
    private Trial _trial;

    void Awake()
    {
        _trial = GameObject.Find("TrialManager").GetComponent<Trial>();
    }

    void OnTriggerEnter()
    {
        var trialState = _trial.TrialState;

        if (trialState == TrialState.OnFirstAttempt)
        {
            SceneManager.LoadScene(2);
        }
        if (trialState == TrialState.OnSecondAttempt)
        {
            SceneManager.LoadScene(3);
        }
	}
}
