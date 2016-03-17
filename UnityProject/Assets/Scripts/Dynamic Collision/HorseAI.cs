using UnityEngine;
using System.Collections;
using System;
//Implemented from: https://www.youtube.com/watch?v=drTcfhULpLA

/// <summary>
/// This class determines when the horse should run onto the path.
/// It is determined by HorseRange which is an empty object which detects when the player is in range
/// </summary>
public class HorseAI : MonoBehaviour {
    Transform tr_Player;
    Transform tr_Range;
    float f_RotSpeed = 3.0f, f_MoveSpeed = 5.0f;
    private bool collided = false;
    public Renderer rend;
  
    // Use this for initialization
    void Start()
    {
        tr_Player = GameObject.FindGameObjectWithTag("Player").transform;
        tr_Range = GameObject.FindGameObjectWithTag("HorseRange").transform;
        rend.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Checks if player is in range
        if ((tr_Player.transform.position - tr_Range.transform.position).magnitude < 10 && !collided)
        {
            //Make horse visible
            rend.enabled = true;

            /* Look at Player*/
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(tr_Player.position - transform.position), f_RotSpeed * Time.deltaTime);

            /* Move at Player*/
            transform.position += transform.forward * f_MoveSpeed * Time.deltaTime;
            //This stops the horse from moving when it reaches a certain position. I had it for when it is close to the players x before but sometimes that wasn't on the path. Can switch it back though
            if (transform.position.x >= -50)
            {
                rend.enabled = false;
                collided = true;
            }
        }

    }
}
