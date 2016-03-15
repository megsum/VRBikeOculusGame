using UnityEngine;
using System.Collections;

public class CollisionCounter : MonoBehaviour {
	public static int numCollisions = -1;

	// Use this for initialization
	void Start () {
		if (numCollisions == -1) {
			numCollisions = 0;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	//Increases collision counter upon entering a trigger
	void OnTriggerEnter(Collider other){
		numCollisions++;
		Debug.Log (numCollisions);
	}
}
