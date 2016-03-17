using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HalfwayScreenDisplay : MonoBehaviour
{
    [SerializeField]
    public GameObject Time;
    [SerializeField]
    public GameObject ObstaclesHit;
    [SerializeField]
    public GameObject NextAttempt;

    // Use this for initialization
    void Start () {
        var trial = GameObject.Find("TrialManager").GetComponent<Trial>();

        var timeElapsed = trial.FirstAttempt.TimeElapsed;

        Time.GetComponent<Text>().text = TimeSpan.FromSeconds(timeElapsed).ToString();
        ObstaclesHit.GetComponent<Text>().text = trial.FirstAttempt.ObstaclesHit.ToString();
        NextAttempt.GetComponent<Text>().text = trial.SecondAttempt.Type.ToString();
    }
}
