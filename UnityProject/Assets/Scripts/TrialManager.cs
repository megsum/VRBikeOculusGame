using System;
using UnityEngine;
using UnityEngine.VR;

/// <summary>
/// Class that represent one participant's trial, which involves 2 course attempts: one on Oculus Rift, 
/// and one via the default projected view.
/// </summary>
public class TrialManager : MonoBehaviour
{
    public CourseAttemptType Type { get; private set; }
    public TimeSpan TimeElapsed { get; private set; }
    public int ObstaclesHit { get; private set; }

    public TrialState TrialState { get; private set; }

    public void SetCourseAttemptType(CourseAttemptType type)
    {
        if (TrialState == TrialState.WaitingForCourseAttemptType)
        {
            Type = type;
            TrialState = TrialState.NotStarted;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        TrialState = TrialState.WaitingForCourseAttemptType;
    }

    public void OnCollision()
    {
        if (TrialState == TrialState.OnCourseAttempt)
        {
            ObstaclesHit++;
        }
    }

    public void OnTrialCompleted(TimeSpan timeElapsed)
    {
        if (TrialState == TrialState.OnCourseAttempt)
        {
            TimeElapsed = timeElapsed;
            TrialState = TrialState.Done;
			Debug.Log("Time: " + TimeElapsed);
            Debug.Log("Collisions: " + ObstaclesHit);
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if ((Levels) level == Levels.Trail && TrialState == TrialState.NotStarted)
        {
            SetViewMode(Type);

            TrialState = TrialState.OnCourseAttempt;
        }
    }

    private void SetViewMode(CourseAttemptType attemptType)
    {
        var enableVR = attemptType == CourseAttemptType.OculusRift3D;

        Debug.Log(enableVR ? "Enabling VR." : "Disabling VR.");

        VRSettings.enabled = enableVR;
    }
}

public enum TrialState
{
    WaitingForCourseAttemptType,
    NotStarted,
    OnCourseAttempt,
    Done
}

public enum CourseAttemptType
{
    Projected2D,
    OculusRift3D
}