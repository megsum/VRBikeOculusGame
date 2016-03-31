﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class CourseAttemptStatsDisplay : MonoBehaviour
{
    [SerializeField] public GameObject ObstaclesHit;
    [SerializeField] public GameObject Time;

    // Use this for initialization
    private void Start()
    {
        var trial = GameObject.Find("TrialManager").GetComponent<TrialManager>();

        var timeElapsed = trial.TimeElapsed;

        Time.GetComponent<Text>().text = string.Format(@"{0}.{1:000} seconds",
            (int) Math.Floor(timeElapsed.TotalSeconds), timeElapsed.Milliseconds);
        ObstaclesHit.GetComponent<Text>().text = trial.ObstaclesHit.ToString();
    }
}