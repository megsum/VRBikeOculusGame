﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;

/// <summary>
/// Class that represent one participant's trial, which involves 2 course attempts: one on Oculus Rift, 
/// and one via the default projected view.
/// </summary>
public class Trial : MonoBehaviour
{
    public CourseAttemptType FirstAttemptType { get; set; }

    public CourseAttempt FirstAttempt { get; set; }
    public CourseAttempt SecondAttempt { get; set; }
    public TrialState TrialState { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        TrialState = TrialState.NotStarted;
    }

    // Update is called once per frame
    private void Update()
    {
        var attempt = GetLatestAttempt();
        if (attempt != null)
        {
            attempt.AddToTimer(Time.deltaTime);
        }
    }

    public void OnCollision()
    {
        var attempt = GetLatestAttempt();
        if (attempt != null)
        {
            attempt.IncrementObstaclesHit();
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (level == 1) // Trail 
        {
            switch (TrialState)
            {
                case TrialState.NotStarted:
                    var firstAttemptType = FirstAttemptType;
                    var secondAttemptType = firstAttemptType == CourseAttemptType.OculusRift3D
                        ? CourseAttemptType.Projected2D
                        : CourseAttemptType.OculusRift3D;

                    FirstAttempt = new CourseAttempt(firstAttemptType);
                    SecondAttempt = new CourseAttempt(secondAttemptType);

                    SetViewMode(firstAttemptType);

                    TrialState = TrialState.OnFirstAttempt;
                    break;
                case TrialState.DoneFirstAttempt:
                    SetViewMode(SecondAttempt.Type);

                    TrialState = TrialState.OnSecondAttempt;
                    break;
                default:
                    throw new InvalidOperationException("Trail scene loaded during invalid Trial state.");
            }
        }
        if (level == 2) // Halfway Screen
        {
            VRSettings.enabled = false;
            TrialState = TrialState.DoneFirstAttempt;
        }
        if (level == 3) // End Screen
        {
            VRSettings.enabled = false;
            TrialState = TrialState.Done;
        }
    }

    private void SetViewMode(CourseAttemptType attemptType)
    {
        var enableVR = attemptType == CourseAttemptType.OculusRift3D;

        Debug.Log(enableVR ? "Enabling VR." : "Disabling VR.");

        VRSettings.enabled = enableVR;
    }

    public CourseAttempt GetLatestAttempt()
    {
        switch (TrialState)
        {
            case TrialState.OnFirstAttempt:
            case TrialState.DoneFirstAttempt:
                return FirstAttempt;
            case TrialState.OnSecondAttempt:
            case TrialState.Done:
                return SecondAttempt;
            default:
                return null;
        }
    }
}

public enum TrialState
{
    NotStarted,
    OnFirstAttempt,
    DoneFirstAttempt,
    OnSecondAttempt,
    Done
}

public class CourseAttempt
{
    public CourseAttempt(CourseAttemptType type)
    {
        Type = type;
        TimeElapsed = 0f;
        ObstaclesHit = 0;
    }

    public CourseAttemptType Type { get; set; }

    public int ObstaclesHit { get; private set; }

    public float TimeElapsed { get; private set; }

    public void AddToTimer(float timeDelta)
    {
        TimeElapsed += timeDelta;
    }

    public void IncrementObstaclesHit()
    {
        ObstaclesHit++;
    }
}

public enum CourseAttemptType
{
    Projected2D,
    OculusRift3D
}