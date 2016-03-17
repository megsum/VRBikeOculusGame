using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoalTrigger : MonoBehaviour
{
    private BikeController _bikeController;
    private Trial _trial;

    private void Awake()
    {
        _bikeController = FindObjectOfType<BikeController>();
        _trial = GameObject.Find("TrialManager").GetComponent<Trial>();
    }

    private void OnTriggerEnter()
    {
        var trialState = _trial.TrialState;
        var attempt = _trial.GetLatestAttempt();

        // Source: StandardUILoader.Update()
        var attemptTime = DateTime.Now - _bikeController.ReferenceTime;
        attempt.TimeElapsed = attemptTime;

        if (trialState == TrialState.OnFirstAttempt)
        {
            SceneManager.LoadScene(2); // Halfway screen
        }

        if (trialState == TrialState.OnSecondAttempt)
        {
            SceneManager.LoadScene(3); // End screen
        }
    }
}