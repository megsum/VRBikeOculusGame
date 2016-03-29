using System;
using UnityEngine;
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
        DontDestroyOnLoad(gameObject);
        TrialState = TrialState.NotStarted;
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
        var levelID = (Levels) level;

        if (levelID == Levels.Trail)
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

        if (levelID == Levels.HalfwayScreen)
        {
            TrialState = TrialState.DoneFirstAttempt;
        }

        if (levelID == Levels.EndScreen)
        {
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
        TimeElapsed = TimeSpan.Zero;
        ObstaclesHit = 0;
    }

    public CourseAttemptType Type { get; set; }
    public TimeSpan TimeElapsed { get; set; }
    public int ObstaclesHit { get; private set; }

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