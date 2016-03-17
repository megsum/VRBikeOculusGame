using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndGoalTrigger : MonoBehaviour {
    private Trial _trial;
    private BikeController _bikeController;

    void Awake()
    {
        _bikeController = FindObjectOfType<BikeController>();
        _trial = GameObject.Find("TrialManager").GetComponent<Trial>();
    }

    void OnTriggerEnter()
    {
        var trialState = _trial.TrialState;
        var attempt = _trial.GetLatestAttempt();

        var attemptTime = DateTime.Now - _bikeController.ReferenceTime;
        attempt.TimeElapsed = attemptTime;

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
