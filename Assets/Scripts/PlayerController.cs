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
    public Material glowMaterial;
    public Material tiledefMaterial;
    List<GameObject> previousGlowingTiles = new List<GameObject>();


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


    Vector2Int FindValidPathPoint(Vector2Int currentPos)
    {
        List<Vector2Int> path = new List<Vector2Int>();
       
        foreach (GameObject tile in previousGlowingTiles)
        {
            tile.GetComponent<Renderer>().material = tiledefMaterial; // Set to default material
        }
        // Clear the previous glowing tiles list
        previousGlowingTiles.Clear();
        path = IsPathBlocked(currentPos);
        Vector2Int lastElement = path[path.Count - 1];
        Debug.Log("Last element of path: " + lastElement);
        path.RemoveAt(path.Count - 1);

        Debug.Log("START of PATH ");
        foreach (Vector2Int pathTile in path)
        {
            GameObject[] tiles = GameObject.FindGameObjectsWithTag("Floor");

            foreach(GameObject tile in tiles)
            {
                Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.z));
                if( tilePos.x == pathTile.x && tilePos.y == pathTile.y)
                {
                    Debug.Log(tile);
                    tile.GetComponent<Renderer>().material = glowMaterial;
                    previousGlowingTiles.Add(tile);
                }
            }

       
        }
        Debug.Log("END OF PATH");
        return lastElement;


    }

    List<Vector2Int> IsPathBlocked(Vector2Int startPos)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        queue.Enqueue(startPos);
        parent[startPos] = startPos; // Root has itself as its parent

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (path.Contains(current))
            {
                return ReconstructPath(parent, startPos, current);
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (IsInBounds(neighbor,10,10) && !parent.ContainsKey(neighbor) && !nonwalkables.Contains(neighbor)) // might want to talk about nonwalkables
                {
                    queue.Enqueue(neighbor);
                    parent[neighbor] = current;
                }
            }
        }
        return null;
    }


    bool IsInBounds(Vector2Int pos , int mazeWidth, int mazeHeight)
    {
        return pos.x >=0 && pos.x < mazeWidth && pos.y >=0 && pos.y < mazeHeight;
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> parent, Vector2Int startPos, Vector2Int endPos)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = endPos;
        while (current != startPos) 
        { 
            path.Add(current);
            current = parent[current];
        }
        path.Add(startPos); // Add the start position at the end of the path list

        path.Reverse(); // Reverse to get the path from start to end
        return path;
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