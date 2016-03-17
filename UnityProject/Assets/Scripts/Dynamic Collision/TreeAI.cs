using UnityEngine;
using System.Collections;

/// <summary>
/// This class determines when the dynamic tree falling should occur.
/// It uses an empty object called TreeRange to determine when the player is in the right place for the tree to fall,
/// and then rotates the tree to fall
/// </summary>

public class TreeAI : MonoBehaviour {
    Transform tr_Player;
    Transform tr_Range;
    float f_MoveSpeed = 5.0f;
    private bool treeFall = false;

    // Use this for initialization
    void Start () {
        tr_Player = GameObject.FindGameObjectWithTag("Player").transform;
        tr_Range = GameObject.FindGameObjectWithTag("TreeRange").transform;
    }
	
	// Update is called once per frame
	void Update () {
        //Checks if player is in range
        if ((tr_Player.transform.position - tr_Range.transform.position).magnitude < 10 && !treeFall)
        {

            /* Tree falls*/
            TreeFall();
            
        }
    }

    void TreeFall()
    {
        transform.Rotate(0, 0, -90);
        treeFall = true;
    }
}
