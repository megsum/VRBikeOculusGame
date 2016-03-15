using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class that represent one participant's trial, which involves 2 course attempts: one on Oculus Rift, 
/// and one via the default projected view.
/// </summary>
public class Trial : MonoBehaviour
{
    private CourseAttempt FirstAttempt { get; set; }
    private CourseAttempt SecondAttempt { get; set; }
    private TrialState TrialState { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        TrialState = TrialState.NotStarted;
    }

    // Update is called once per frame
    private void Update()
    {
        switch (TrialState)
        {
            case TrialState.OnFirstAttempt:
                FirstAttempt.AddToTimer(Time.deltaTime);
                break;
            case TrialState.OnSecondAttempt:
                SecondAttempt.AddToTimer(Time.deltaTime);
                break;
            // do nothing on default
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (level == 1) // Trail 
        {
            switch (TrialState)
            {
                case TrialState.NotStarted:
                    var startMenu = GetComponentInParent<StartMenu>();
                    var firstAttemptType = startMenu.GetSelectedCourseAttemptType();
                    var secondAttemptType = FirstAttempt.Type == CourseAttemptType.OculusRift3D
                        ? CourseAttemptType.Projected2D
                        : CourseAttemptType.OculusRift3D;

                    FirstAttempt = new CourseAttempt(firstAttemptType);
                    SecondAttempt = new CourseAttempt(secondAttemptType);
                    TrialState = TrialState.OnFirstAttempt;
                    break;
                case TrialState.DoneFirstAttempt:
                    TrialState = TrialState.OnSecondAttempt;
                    break;
                default:
                    throw new InvalidOperationException("Trail scene loaded during invalid Trial state.");
            }
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
}

public enum CourseAttemptType
{
    Projected2D,
    OculusRift3D
}