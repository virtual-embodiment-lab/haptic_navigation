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
 

    void Start()
    {
        path = MazeGenerator.paths[MazeGenerator.choice].ToList();
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        transform.position = new Vector3(0.2f, -0.5f, 0f);
     
    }
    private void Update()
    {
        /*  How Movement Works: Players Presses WASD which collects "Horizontal" and "Vertical" axis information, so that player can rotate 
            Then Player presses space bar to actually move forward.
        */
        leftController = GameObject.FindWithTag("LeftController");
        rightController = GameObject.FindWithTag("RightController");
        if (leftController != null && rightController != null)
        {
            hapticLeft = leftController.GetComponent<HapticController>();
            hapticRight = rightController.GetComponent<HapticController>();
        }
  
       

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
        /*// whenever we turn that should be the current forward 
        Debug.Log("Current position: " + currentPos);
        Debug.Log("Current direction " + currentDir);*/

        // if they go off track ( beep, beep, beep)
        if (!path.Contains(currentPos))
        {
            Vector2Int nearestPos = FindNearestPathPoint(currentPos);
            Vector2Int differenceVector = nearestPos - currentPos;
            // convert from Vector2Int to Vector 2
            Vector2 a = new Vector2(currentDir.x, currentDir.y);
            Vector2 b = new Vector2(differenceVector.x, differenceVector.y);
            b.Normalize();

            // find angle in Betweeen two vectors
            float angleBetween = findAngle(a, b);
            ProvideDirection(angleBetween,false);
        }

        // if player is on track
        if (path.Contains(currentPos))
        {
            int currentPosIndex = path.IndexOf(currentPos);
            Vector2Int nearestPos = path[currentPosIndex + 1];
            Vector2Int differenceVector = nearestPos - currentPos;

            // convert from Vector2Int to Vector 2
            Vector2 a = new Vector2(currentDir.x, currentDir.y);
            Vector2 b = new Vector2(differenceVector.x, differenceVector.y);

            // find angle in Betweeen two vectors
            float angleBetween = findAngle(a, b);
            ProvideDirection(angleBetween,true);
        }
    }

    float findAngle(Vector2 currentVector, Vector2 targetVector) {
        float currentAngle = Mathf.Atan2(currentVector.y, currentVector.x);
        float targetAngle = Mathf.Atan2(targetVector.y, targetVector.x);
        float angledif = targetAngle - currentAngle;
        angledif = Mathf.Atan2(Mathf.Sin(angledif), Mathf.Cos(angledif));
        return angledif;
    }

    Vector2Int FindNearestPathPoint(Vector2Int currentPos)
    {
       /* Checks the set of points in `path` and finds the closest one to `currentPos`.*/

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
    }

    void ProvideDirection(float angleBetween, bool onTrack)
    {
        float amplitude;
        float duration;
        float frequency;

        // Sends different vibration specs depending on whether the player is on the right path or not
        if(onTrack){
            amplitude = .5f;
            duration = .5f;
            frequency = .5f;
        }
        else
        {
            amplitude = .2f;
            duration = .2f;
            frequency = .2f;
        }
         
        if (angleBetween > 0)
        {
            hapticLeft?.SendHaptics(amplitude, duration, frequency);
        }
        else if (angleBetween < 0)
        {
            hapticRight?.SendHaptics(amplitude, duration, frequency);
        }
        else if (angleBetween == 0)
        {
            hapticLeft?.SendHaptics(amplitude, duration, frequency);
            hapticRight?.SendHaptics(amplitude, duration, frequency);
        }

    }
}



