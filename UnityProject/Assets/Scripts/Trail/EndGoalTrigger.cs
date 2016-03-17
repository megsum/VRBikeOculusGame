using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndGoalTrigger : MonoBehaviour {
	void OnTriggerEnter() {
	    SceneManager.LoadScene(2);
	}
}
