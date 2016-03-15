using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour {

    [SerializeField]
    Toggle Projected2DFirstToggle;

    [SerializeField]
    Toggle OculusRift3DFirstToggle;

    public void LoadScene(int level)
    {
        SceneManager.LoadScene(level);
    }

    public CourseAttemptType GetSelectedCourseAttemptType()
    {
        // The above toggles are in a toggle group and one must be selected, so this works
        return Projected2DFirstToggle.isOn 
            ? CourseAttemptType.Projected2D 
            : CourseAttemptType.OculusRift3D;
    }
}
