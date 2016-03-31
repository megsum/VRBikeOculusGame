using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoalTrigger : MonoBehaviour
{
    private BikeController _bikeController;
    private TrialManager _trialManager;

    private void Awake()
    {
        _bikeController = FindObjectOfType<BikeController>();
        _trialManager = GameObject.Find("TrialManager").GetComponent<TrialManager>();
    }

    private void OnTriggerEnter()
    {
        // Source: StandardUILoader.Update()
        var attemptTime = DateTime.Now - _bikeController.ReferenceTime;
        _trialManager.OnTrialCompleted(attemptTime);
        SceneManager.LoadScene((int) Levels.EndScreen);
    }
}