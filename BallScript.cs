using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour {

    public GameObject player1_hold_RH; //starting guy
    public GameObject player2_hold_RH; //second guy's right hand
    public GameObject player2_hold_LH; //second guy's left hand
    public GameObject self_hold_RH; //third guy
    public GameObject player1_ground; //ground next to first
    public GameObject player2_ground; //ground next to second
    public GameObject self_ground; //ground next to third
    public Rigidbody rigid;
    //public Vector3 currBallPosition;
    public int switchInt;  //Indicates where the current ball is
    public int counter;


    // var lastPos:Vector3;
    // var curVel:Vector3;


    // Use this for initialization
    void Start () {
        transform.parent = player1_hold_RH.transform;
        transform.position = player1_hold_RH.transform.position;
        rigid.useGravity = false;
        Vector3 currBallPosition = player1_hold_RH.transform.position;
    }

    // Update is called once per frame
    void Update () {

    }

    public void pickUpBall()
    {
        CancelInvoke();
        if (counter == 3)
        {
            //transform.parent = self_hold_RH.transform;
            //transform.position = self_hold_RH.transform.position;
            transform.parent = player1_hold_RH.transform;
            transform.position = player1_hold_RH.transform.position;
            Debug.Log("in Three");
        }
        if (counter == 5)
        {
            transform.parent = self_hold_RH.transform;
            transform.position = self_hold_RH.transform.position;
            Debug.Log("In Five");
            //transform.parent = player1_hold_RH.transform;
            //transform.position = player1_hold_RH.transform.position;
        }
    }

    public void ReleaseMe()
    {
        CancelInvoke();
        counter = counter + 1; //increments for each trial, on the ball throw (purpose: when ball falls)
        transform.parent = null;
        Debug.Log(counter);
        switch (counter)
        {
            case 1:
                switchInt = 1;
                break;
            case 2:
                switchInt = 2;
                break;
            case 3:
                switchInt = 6;
                break;
            //first pickup should happen here
            case 4:
                switchInt = 1;
                break;
            case 5:
                switchInt = 5;
                break;
            //second pickup should happen here
            case 6:
                switchInt = 3;
                break;
            case 7:
                switchInt = 7;
                break;
            default:
                Debug.Log("Error");
                break;
        }


        InvokeRepeating("MoveToOther", 0f, .005f);
        //2nd Parameter: delay to call (don't want delay, so 0 seconds), 3rd parameter: how frequently move to other is called(every .005 seconds)

    }

    public void MoveToOther()
    {
        GameObject desttarg = getDestTarg();
        transform.position = Vector3.MoveTowards(transform.position, desttarg.transform.position, .06f);
        //currBallPosition = desttarg.transform.position;
    }

    public GameObject getDestTarg()
    {
        switch (switchInt)
        {
            case 1:
                return player2_hold_RH;
            case 2:
                return self_hold_RH;
            case 3:
                return player1_hold_RH;
            case 4:
                return player2_ground;
            case 5:
                return self_ground;
            case 6:
                return player1_ground;
            case 7:
                return player2_hold_LH;
            default:
                Debug.Log("Error");
                return player1_hold_RH;
        }
    }

    public int getLocation()
    {
        return switchInt;
    }

    //use the function 'coroutine' for delays

}