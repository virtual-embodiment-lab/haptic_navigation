using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;


public class PlayerController : MonoBehaviour
{
    public GameObject leftController, rightController;
    public HapticController hapticLeft, hapticRight;
    public CharacterController characterController;
    public float speed = 1f;
    public float turnSmoothTime = 1f;
    float turnSmoothVelocity;
    public float lastHorizontalInput;
    List<Vector2Int> path;
    public GameObject camera;
    public Vector3 cameraDir;

    void Start()
    {
        path = MazeGenerator.paths[MazeGenerator.choice].ToList();
        /*leftController = GameObject.FindWithTag("LeftController");
        rightController = GameObject.FindWithTag("RightController");*/
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        /*transform.forward = new Vector3(1f, transform.forward.y, 1f);*/
        transform.position = new Vector3(0.2f, -0.5f, 0f);
        /*transform.position = -transform.forward;
        transform.position = new Vector3(transform.position.x, -.5f, transform.position.z);*/
    }
    private void Update()
    {
        /*  How Movement Works: Players Presses WASD which collects "Horizontal" and "Vertical" axis information, so that player can rotate 
            Then Player presses space bar to actually move forward.
        */
        camera = GameObject.FindWithTag("MainCamera");
        leftController = GameObject.FindWithTag("LeftController");
        rightController = GameObject.FindWithTag("RightController");
        if (leftController != null && rightController != null)
        {
            hapticLeft = leftController.GetComponent<HapticController>();
            hapticRight = rightController.GetComponent<HapticController>();
        }
        /*cameraDir = camera.transform.position;*/
        


       /* float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f)
        {
            if (Mathf.Sign(horizontal) != Mathf.Sign(lastHorizontalInput)) // prevents the delay in the movement when abruptly turning
            {
                turnSmoothVelocity = 0;
            }

            // Sets the rotation of the character 
            float targetAngle = Mathf.Atan2(direction.x, direction.z) *Mathf.Rad2Deg + transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

        }

        lastHorizontalInput = horizontal;

        // Sets the movement of the character
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 moveDir = transform.forward;
            characterController.Move(moveDir.normalized * speed * Time.deltaTime);
        }*/
       
        CheckAndGuidePath();
    }


    void CheckAndGuidePath()
    {
        /* Compares the path the player is on to the actual optimal path and recommends adjustments to player direction.*/

        Vector2Int currentDir = new Vector2Int(Mathf.RoundToInt(transform.forward.x), Mathf.RoundToInt(transform.forward.z)); // discretizes the current player position
        Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)); // discretizes the current player position
        // whenever we turn that should be the current forward 
        Debug.Log("Current position: " + currentPos);
        Debug.Log("Current direction " + currentDir);

        /*if (!path.Contains(currentPos))
        {
            Vector2Int nearestPoint = FindNearestPathPoint(currentPos);
            Vector2Int directionVector = nearestPoint - currentPos;
            Debug.Log(directionVector);
            ProvideDirection(directionVector);
        }*/

        if (path.Contains(currentPos))
        {
            int currentPosIndex = path.IndexOf(currentPos);
            Vector2Int nextPos = path[currentPosIndex+1];
            Vector2Int differenceVector = nextPos - currentPos;
            Debug.Log("NextPos: " + nextPos);
            Debug.Log("differenceVector " + differenceVector);

            Vector2 a = new Vector2(currentDir.x, currentDir.y);
            Vector2 b = new Vector2(differenceVector.x, differenceVector.y);
            float currentAngle = Mathf.Atan2(a.y, a.x);
            float targetAngle = Mathf.Atan2(b.y, b.x);
            float angledif = targetAngle - currentAngle;
            angledif = Mathf.Atan2(Mathf.Sin(angledif), Mathf.Cos(angledif));
            Debug.Log("ANGLE:" + angledif);

            if(angledif > 0)
            {
                hapticLeft?.SendHaptics(); 
            }
            else if(angledif < 0)
            {
                hapticRight?.SendHaptics();
            }
            else if(angledif == 0)
            {
                hapticLeft?.SendHaptics();
                hapticRight?.SendHaptics();
            }

            
            
        }
    }

    /*Vector2Int FindNearestPathPoint(Vector2Int currentPos)
    {
        *//* Checks the set of points in `path` and finds the closest one to `currentPos`.*//*

        Vector2Int nearestPoint = new Vector2Int();
        float minDistance = float.MaxValue;

        foreach (Vector2Int point in path)
        {
            float distance = Vector2.Distance(currentPos, point);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = point;
            }
        }
        return nearestPoint;
    }*/

  /*  void ProvideDirection(Vector2Int directionVector)
    {
        *//*Depending on the `directionVector` of the player, this function gives Haptic Feedback to player
         * Remember when subtracting the player `currentPos` and the `nearestPoint` The player is the base.
         *//*


        if (Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y)) // if mag(x_coord) is > mag(y_coord) move horizontally
        {
            if (directionVector.x > 0)
            {
                //Debug.Log("Move right");
                *//*hapticRight?.SendHaptics();*//*

            }
            else
            {
              *//*  Debug.Log("Move left");
                hapticLeft?.SendHaptics();*//*
            }
        }
        else if (Mathf.Abs(directionVector.x) <= Mathf.Abs(directionVector.y))// if mag(x_coord) is < mag(y_coord) move vertically
        {
            if (directionVector.y > 0)
            {
                *//*Debug.Log("Move up");
                hapticRight?.SendHaptics();
                hapticLeft?.SendHaptics();*//*
            }
            else
            {
                *//*Debug.Log("Move down");
                hapticRight?.SendHaptics();
                hapticLeft?.SendHaptics();*//*
            }
        }
        else if (Mathf.Abs(directionVector.x) == 0  || Mathf.Abs(directionVector.y) == 0)
        {
            hapticRight?.SendHaptics();
            hapticLeft?.SendHaptics(); 
        }
    }*/
}



