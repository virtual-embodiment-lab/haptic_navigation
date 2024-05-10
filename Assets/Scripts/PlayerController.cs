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
    List<Vector2Int> path;
    public GameObject camera;
    public Vector3 cameraDir;


    void Start()
    {
        path = MazeGenerator.paths[MazeGenerator.choice].ToList();
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        transform.position = new Vector3(0.2f, -0.5f, 0f);
        transform.forward = new Vector3(1f, transform.forward.y, 1f);

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
        cameraDir = camera.transform.position;
        CheckAndGuidePath();
    }


    void CheckAndGuidePath()
    {
        /* Compares the path the player is on to the actual optimal path and recommends adjustments to player direction.*/
        //Vector2Int currentDir = new Vector2Int(Mathf.RoundToInt(cameraDir.x), Mathf.RoundToInt(cameraDir.z));
        Vector2Int currentDir = new Vector2Int(Mathf.RoundToInt(transform.forward.x), Mathf.RoundToInt(transform.forward.z)); // discretizes the current player position
        Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)); // discretizes the current player position

        // if they go off track
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
            ProvideDirection(angleBetween, false);
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

        /* Essential for the Haptic Feedback as it finds the direction the player needs to turn, based on the direction they're facing.
        */

        float currentAngle = Mathf.Atan2(currentVector.y, currentVector.x);
        float targetAngle = Mathf.Atan2(targetVector.y, targetVector.x);
        float angledif = targetAngle - currentAngle;
        angledif = Mathf.Atan2(Mathf.Sin(angledif), Mathf.Cos(angledif));
        Debug.Log("ANGLE:" + angledif);
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
        /*Sends different vibration specs depending on whether the player is on the right path or not*/

        float amplitude;
        float duration;
        float frequency;

       
        if(onTrack){
            amplitude = .2f;
            duration = .2f;
            frequency = .4f;
        }
        else
        {
            amplitude = .5f;
            duration = .5f;
            frequency = .9f;
        }
        /*0.7853981f*/
         
        if (angleBetween > 0)
        {
            // we hard coded this value because the left controller was feeling a bit weaker, so we gave it a higher amplitude
            hapticLeft?.SendHaptics(.4f, duration, frequency);
        }
        else if (angleBetween < 0)
        {
            hapticRight?.SendHaptics(amplitude, duration, frequency);
        }
        else if (angleBetween== 0) /*angleBetween >= -0.7853981f && angleBetween <= 0.7853981f*/
        {
            // Uncomment this code to activate CONSTANT FEEDBACK MECHANISM. When commented this GPS MECHANISM
           /* hapticLeft?.SendHaptics(.4f, duration, frequency);
            hapticRight?.SendHaptics(amplitude, duration, frequency);*/
        }

    }
}



