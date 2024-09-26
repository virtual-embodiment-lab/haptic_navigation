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
    List<Vector2Int> nonwalkables;
    public GameObject camera;
    public Vector3 cameraDir;
    public float frequency;
    public float duration;
    bool isPulsing;
    public MazeGenerator mazeGenerator;


    void Start()
    {
        path = MazeGenerator.paths[0].ToList();
        nonwalkables = MazeGenerator.nonwalkables[0].ToList();
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        transform.position = new Vector3(0.2f, -0.5f, 0f);
        transform.forward = new Vector3(1f, transform.forward.y, 1f);
        isPulsing = true;



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
           

            Vector2Int nearestPos = FindValidPathPoint(currentPos);
            Debug.Log(nearestPos);
            Vector2Int differenceVector = nearestPos - currentPos;
            Collider[] colliders = Physics.OverlapSphere(new Vector2(nearestPos.x, nearestPos.y), 0.1f);
            foreach(Collider collider in colliders)
            {
                GameObject obj = collider.gameObject;
                
            }
            

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
       /* bool blocked = IsPathBlocked(currentPos, nearestPoint);*/
      
        return nearestPoint;
    }


    Vector2Int FindValidPathPoint(Vector2Int currentPos)
    {
        List<Vector2Int> sortedPathPoints = path.OrderBy(point => Vector2.Distance(currentPos, point)).ToList();
        float minDistance = float.MaxValue;
        float tempdist;
        Vector2Int nearestPoint = new Vector2Int();
        

        foreach (Vector2Int pathPoint in path)
        {
            tempdist = IsPathBlocked(currentPos, pathPoint);
          
            if (tempdist != -1)
            {
                if (tempdist < minDistance)
                {
                    minDistance = tempdist;
                    nearestPoint = pathPoint;
                } 
            }
        }
      
        return nearestPoint;
        Debug.Log("FAILURE");
        return new Vector2Int(-1, -1);
        
    }

    int IsPathBlocked(Vector2Int startPos, Vector2Int endPos)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(startPos);
        int distance = 0;
        visited.Add(startPos);

        while(queue.Count > 0)
        {
            int levelSize = queue.Count;

            for (int i = 0; i <levelSize; i++)
            {
                Vector2Int currentPos = queue.Dequeue();
                if (currentPos == endPos)
                { return distance; }

                foreach (Vector2Int neighbor in GetNeighbors(currentPos))
                {
                    if (!IsInBounds(neighbor, 10, 10) || visited.Contains(neighbor)) { continue; }
                    if (nonwalkables.Contains(neighbor)) { continue; }
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
          
            distance++;
        }
        return -1;
    }


    bool IsInBounds(Vector2Int pos , int mazeWidth, int mazeHeight)
    {
        return pos.x >=0 && pos.x < mazeWidth && pos.y >=0 && pos.y < mazeHeight;
    }

    List<Vector2Int> GetNeighbors(Vector2Int currentPos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        neighbors.Add(new Vector2Int(currentPos.x + 1, currentPos.y));
        neighbors.Add(new Vector2Int(currentPos.x - 1, currentPos.y));
        neighbors.Add(new Vector2Int(currentPos.x , currentPos.y + 1));
        neighbors.Add(new Vector2Int(currentPos.x , currentPos.y + 1));
        return neighbors;
    }

    void ProvideDirection(float angleBetween, bool onTrack)
    {
        /*Sends different vibration specs depending on whether the player is on the right path or not*/

        float amplitude;

        amplitude = .3f;
        duration = .1f;
        frequency = .1f;

    if (angleBetween > 0)
        {
            // we hard coded this value because the left controller was feeling a bit weaker, so we gave it a higher amplitude
            hapticLeft?.SendHaptics(.5f * amplitude, duration, frequency);

        }
        else if (angleBetween < 0)
        {
            hapticRight?.SendHaptics(amplitude, duration, frequency);
        }
        else if (angleBetween == 0) //angleBetween >= -0.7853981f && angleBetween <= 0.7853981f
    {
            // Uncomment this code to activate CONSTANT FEEDBACK MECHANISM. When commented this GPS MECHANISM
            hapticLeft?.SendHaptics(amplitude, duration, frequency);
            hapticRight?.SendHaptics(amplitude*2f, duration, frequency);
        }

    }

    /*void ProvideDirection(float angleBetween, bool onTrack)
    {
        *//*Sends different vibration specs depending on whether the player is on the right path or not*//*

        float amplitude;
        amplitude = .3f;
   

        if (angleBetween > 0 && isPulsing == true)
        {
                hapticLeft?.SendHaptics(amplitude*2f, 1f, 1f);
                isPulsing = false; 
        }
         else if (angleBetween < 0 && isPulsing == true)
         {

            hapticRight?.SendHaptics(amplitude * 2f, 1f, 1f);
            isPulsing = false;
        }
         else if (angleBetween == 0 && isPulsing == false) *//*angleBetween >= -0.7853981f && angleBetween <= 0.7853981f*//*
                {
            // Uncomment this code to activate CONSTANT FEEDBACK MECHANISM. When commented this GPS MECHANISM
            hapticLeft?.SendHaptics(amplitude * 2f, 1f, 1f);
            hapticRight?.SendHaptics(amplitude * 2f, 1f, 1f);
            isPulsing = true;
        }

    }*/
}



/*List<Vector2Int> openList = new List<Vector2Int>();
       HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

       openList.Add(startPos);

       while (openList.Count > 0)
       {
           Vector2Int currentPos = openList[0];
           openList.RemoveAt(0);

           if(currentPos == endPos)
           {
               return false;
           }
           closedList.Add(currentPos);
           List<Vector2Int> neighbors = GetNeighbors(currentPos);

           foreach (Vector2Int neighbor in neighbors)
           {
               if(nonwalkables.Contains(neighbor) || closedList.Contains(neighbor))
               {
                   continue;
               }

               if (!openList.Contains(neighbor))
               {
                   openList.Add(neighbor);
               }
           }
           if (openList.Count == 0)
           {
               break;
           }
       }

       return true;*/