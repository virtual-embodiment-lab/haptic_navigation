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

    /* VR Controller Variables */
    public GameObject leftController, rightController;
    public HapticController hapticLeft, hapticRight;
    public float amplitude;
    public float frequency;
    public float duration;
    bool isPulsing;

    /* Player Direction/Position Variables*/
    public GameObject camera;
    public Vector3 cameraDir;

    /* MAZE Generation Variables */
    int choice;
    List<Vector2Int> path;
    List<Vector2Int> nonwalkables;
    public MazeGenerator mazeGenerator; // pulls from the MazeGenerator Script

    /* MAZE Highlight Variables */
    public bool isHighlight;
    public Material glowMaterial;
    public Material tiledefMaterial;
    public Material pathTileMaterial;
    List<GameObject> previousGlowingTiles = new List<GameObject>();


    void Start()
    {
        /* Gets the path and the walls (nonwalkables) from the MazeGenerator Script*/

        path = MazeGenerator.paths[MazeGenerator.choice].ToList();
        nonwalkables = MazeGenerator.nonwalkables[MazeGenerator.choice].ToList();

        /* Sets the Position and Direction of the VR headset */
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        transform.position = new Vector3(0.2f, -0.5f, 0f);
        transform.forward = new Vector3(1f, transform.forward.y, 0f);

        /* For GPS System */
        isPulsing = true;
    }


    /*  How Movement Works: Players Presses WASD which collects "Horizontal" and "Vertical" axis information, so that player can rotate 
        Then Player presses space bar to actually move forward.
    */
    private void Update()
    {
        Debug.Log(choice);
        /* Gets the VR headset position  */
        camera = GameObject.FindWithTag("MainCamera");
        cameraDir = camera.transform.position;

        /* Gets the Controller Attributes (ideally should be in Start() but doesn't work in there so I put it in Update() */
        leftController = GameObject.FindWithTag("LeftController");
        rightController = GameObject.FindWithTag("RightController");

        if (leftController != null && rightController != null)
        {
            hapticLeft = leftController.GetComponent<HapticController>();
            hapticRight = rightController.GetComponent<HapticController>();
        }

        FollowPath();
        HighlightPath();
    }

    /* 
     * Compares the path the player is on to the actual optimal path and recommends adjustments to player direction.
     * The player can either be on the:
     *         - `solutionPath`: which means if they follow the right directions they will solve the Maze
     *         OR 
     *         - OffPath: which means they are somehow lost and need help to get back on the `solutionPath` so they can solve the maze.
     * This function will determine what path the player is on and then will determine how to help them to get back on the solution path
     * so they can complete the maze.
     */
    void FollowPath()
    {

        /* Get the player direction and positions */
        Vector2Int currentDir = new Vector2Int(Mathf.RoundToInt(transform.forward.x), Mathf.RoundToInt(transform.forward.z)); // discretizes the current player position
        Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)); // discretizes the current player position

        List<Vector2Int> pathToFollow = new List<Vector2Int>();

        // I set tjh pathToFollow usiong ternary conditional operator
        pathToFollow = (!path.Contains(currentPos) ? IsPathBlocked(currentPos) : path);
        int currentPosIndex = pathToFollow.IndexOf(currentPos);
        Vector2Int nearestPos = pathToFollow[currentPosIndex + 1];
        Vector2Int differenceVector = nearestPos - currentPos;

        // convert from Vector2Int to Vector 2
        Vector2 a = new Vector2(currentDir.x, currentDir.y);
        Vector2 b = new Vector2(differenceVector.x, differenceVector.y);
        b.Normalize();

        // find angle in Betweeen two vectors
        float angleBetween = findAngle(a, b);
        ProvideDirection(angleBetween, false);
    }

    /* 
    This is just for visual purposes so the tester can see the player and track their movements from the solution path.
    It does the same thing as FollowPath except it doesn't guide the user it just makes certain tiles glow. 
    **ONLY FOR TESTER**
    */
    void HighlightPath()
    {
        if (isHighlight)
        {
            /* Compares the path the player is on to the actual optimal path and recommends adjustments to player direction.*/
            Vector2Int currentDir = new Vector2Int(Mathf.RoundToInt(transform.forward.x), Mathf.RoundToInt(transform.forward.z)); // discretizes the current player position
            Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)); // discretizes the current player position

            // The path to follow
            List<Vector2Int> pathToFollow = new List<Vector2Int>();

            // I set tjh pathToFollow usiong ternary conditional operator
            pathToFollow = (!path.Contains(currentPos) ? IsPathBlocked(currentPos) : path);

            foreach (GameObject tile in previousGlowingTiles)
            {
                Material tileMaterial = (tile.tag == "Floor") ? tiledefMaterial : pathTileMaterial;

                tile.GetComponent<Renderer>().material = tileMaterial; // Set to default material
            }
            // Clear the previous glowing tiles list
            previousGlowingTiles.Clear();

            GameObject[] floorTiles = GameObject.FindGameObjectsWithTag("Floor");

            foreach (Vector2Int pathTile in pathToFollow)
            {
                foreach (GameObject floorTile in floorTiles)
                {
                    Vector2Int floorTilePos = new Vector2Int(Mathf.RoundToInt(floorTile.transform.position.x), Mathf.RoundToInt(floorTile.transform.position.z));
                    if (floorTilePos == pathTile)
                    {
                        floorTile.GetComponent<Renderer>().material = glowMaterial;
                        previousGlowingTiles.Add(floorTile);
                        break;
                    }
                }
            }
        }
    }

    /* Essential for the Haptic Feedback as it finds the direction the player needs to turn, based on the direction they're facing.
     * Mainly used in FollowPath().
    */
    float findAngle(Vector2 currentVector, Vector2 targetVector)
    {



        float currentAngle = Mathf.Atan2(currentVector.y, currentVector.x);
        float targetAngle = Mathf.Atan2(targetVector.y, targetVector.x);
        float angledif = targetAngle - currentAngle;
        angledif = Mathf.Atan2(Mathf.Sin(angledif), Mathf.Cos(angledif));

        return angledif;
    }

    /* BFS Function that finds the nearest `solutionPath` tile to the player. This function is called when the player is lost 
     * and is off the `solutionTiles`.It will avoid walls and find the shortest path to the nearest tile. 
     * uses ReconstructPath() to find the actual path itself.
    */
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
                if (IsInBounds(neighbor, 10, 10) && !parent.ContainsKey(neighbor) && !nonwalkables.Contains(neighbor)) // might want to talk about nonwalkables
                {
                    queue.Enqueue(neighbor);
                    parent[neighbor] = current;
                }
            }
        }
        return null;
    }

    /* 
     * Used in IsPathBlocked to determine if ta tile is out of maze bounds
    */
    bool IsInBounds(Vector2Int pos, int mazeWidth, int mazeHeight)
    {
        return pos.x >= 0 && pos.x < mazeWidth && pos.y >= 0 && pos.y < mazeHeight;
    }


    /* 
     * Builds up the path from startPos to endPos.
     * Used in IsPathBlocked()
    */
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

    // HAND HOLDING CONDITION
    /*    void ProvideDirection(float angleBetween, bool onTrack)
        {
            *//*Sends different vibration specs depending on whether the player is on the right path or not*//*

            float amplitude;

            amplitude = 1f;
            duration = .1f;
            frequency = .1f;

            if (angleBetween > 0)
            {
                // we hard coded this value because the left controller was feeling a bit weaker, so we gave it a higher amplitude
                hapticLeft?.SendHaptics(amplitude, duration, frequency);
                Debug.Log("LEFT");

            }
            else if (angleBetween < 0)
            {
                hapticRight?.SendHaptics(amplitude, duration, frequency);
                Debug.Log("RIGHT");
            }
            else if (angleBetween == 0) //angleBetween >= -0.7853981f && angleBetween <= 0.7853981f
            {
                // Uncomment this code to activate CONSTANT FEEDBACK MECHANISM. When commented this GPS MECHANISM
                hapticLeft?.SendHaptics(amplitude, duration, frequency);
                hapticRight?.SendHaptics(amplitude, duration, frequency);
                Debug.Log("STRAIGHT");
            }

        }*/
    // GPS 
    void ProvideDirection(float angleBetween, bool onTrack)
    {
        

        float amplitude;
        amplitude = .3f;


        if (angleBetween > 0 && isPulsing == true)
        {
            hapticLeft?.SendHaptics(amplitude * 2f, 1f, 1f);
            isPulsing = false;
        }
        else if (angleBetween < 0 && isPulsing == true)
        {

            hapticRight?.SendHaptics(amplitude * 2f, 1f, 1f);
            isPulsing = false;
        }
        else if (angleBetween == 0 && isPulsing == false) 
                {
            // Uncomment this code to activate CONSTANT FEEDBACK MECHANISM. When commented this GPS MECHANISM
            hapticLeft?.SendHaptics(amplitude * 2f, 1f, 1f);
            hapticRight?.SendHaptics(amplitude * 2f, 1f, 1f);
            isPulsing = true;
        }

    }
}