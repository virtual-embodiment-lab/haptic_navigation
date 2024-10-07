using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Unity.VisualScripting;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public Material tiledefMaterial;
    public Material pathTileMaterial;
    public GameObject cubePrefab;
    public GameObject boundPrefab;
    private bool[,] gridOccupied = new bool[10, 10]; // Track occupied positions
    [SerializeField]
    public int choice;


    /* Currently there are two mazes represented by the:
        paths[] list: path[0] gives solution path to maze0 and path[1] gives solution path to maze1 ... 
        nonwalkables[] list: nonwalkables[0] gives the wall blocks to maze0 and nonwalkablesh[1] gives the wall blocks to maze1 ... */

    public static Vector2Int[][] paths = new Vector2Int[][]
    {
        new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1),
            new Vector2Int(2, 2), new Vector2Int(3, 2), new Vector2Int(3, 3), new Vector2Int(3, 4),
            new Vector2Int(3, 5), new Vector2Int(4, 5), new Vector2Int(4, 6), new Vector2Int(5, 6),
            new Vector2Int(6, 6), new Vector2Int(6, 7), new Vector2Int(6, 8), new Vector2Int(6, 9),
            new Vector2Int(7, 9), new Vector2Int(8, 9), new Vector2Int(9, 9)
        },
        new Vector2Int[]
        {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2),
            new Vector2Int(1, 3), new Vector2Int(1, 4), new Vector2Int(1, 5), new Vector2Int(1, 6),
            new Vector2Int(0, 6), new Vector2Int(0, 7), new Vector2Int(0, 8), new Vector2Int(0, 9),
            new Vector2Int(1, 9), new Vector2Int(2, 9), new Vector2Int(3, 9), new Vector2Int(3, 8),
            new Vector2Int(4, 8), new Vector2Int(4, 7), new Vector2Int(4, 6), new Vector2Int(4, 5),
            new Vector2Int(3, 5), new Vector2Int(3, 4), new Vector2Int(3, 3), new Vector2Int(3, 2),
            new Vector2Int(4, 2), new Vector2Int(4, 1), new Vector2Int(5, 1), new Vector2Int(6, 1),
            new Vector2Int(7, 1), new Vector2Int(8, 1), new Vector2Int(9, 1), new Vector2Int(9, 2),
            new Vector2Int(9, 3), new Vector2Int(9, 4), new Vector2Int(8, 4), new Vector2Int(7, 4),
            new Vector2Int(7, 5), new Vector2Int(7, 6), new Vector2Int(7, 7), new Vector2Int(8, 7),
            new Vector2Int(9, 7), new Vector2Int(9, 8), new Vector2Int(9, 9)
        },
        new Vector2Int[]
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1),
            new Vector2Int(3, 1), new Vector2Int(4, 1), new Vector2Int(5, 1), new Vector2Int(6, 1),
            new Vector2Int(6, 0), new Vector2Int(7, 0), new Vector2Int(8, 0), new Vector2Int(9, 0),
            new Vector2Int(9, 1), new Vector2Int(9, 2), new Vector2Int(9, 3), new Vector2Int(8, 3),
            new Vector2Int(8, 4), new Vector2Int(7, 4), new Vector2Int(6, 4), new Vector2Int(5, 4),
            new Vector2Int(5, 3), new Vector2Int(4, 3), new Vector2Int(3, 3), new Vector2Int(2, 3),
            new Vector2Int(2, 4), new Vector2Int(1, 4), new Vector2Int(1, 5), new Vector2Int(1, 6),
            new Vector2Int(1, 7), new Vector2Int(1, 8), new Vector2Int(1, 9), new Vector2Int(2, 9),
            new Vector2Int(3, 9), new Vector2Int(4, 9), new Vector2Int(4, 8), new Vector2Int(4, 7),
            new Vector2Int(5, 7), new Vector2Int(6, 7), new Vector2Int(7, 7), new Vector2Int(7, 8),
            new Vector2Int(7, 9), new Vector2Int(8, 9), new Vector2Int(9, 9)
        },
        new Vector2Int[]
        {
            new Vector2Int(0, 9), new Vector2Int(1, 9), new Vector2Int(2, 9), new Vector2Int(2, 8),
            new Vector2Int(3, 8), new Vector2Int(4, 8), new Vector2Int(5, 8), new Vector2Int(6, 8),
            new Vector2Int(6, 9), new Vector2Int(7, 9), new Vector2Int(8, 9), new Vector2Int(9, 9),
            new Vector2Int(9, 8), new Vector2Int(9, 7), new Vector2Int(9, 6), new Vector2Int(8, 6),
            new Vector2Int(8, 5), new Vector2Int(7, 5), new Vector2Int(6, 5), new Vector2Int(5, 5),
            new Vector2Int(5, 6), new Vector2Int(4, 6), new Vector2Int(3, 6), new Vector2Int(2, 6),
            new Vector2Int(2, 5), new Vector2Int(1, 5), new Vector2Int(1, 4), new Vector2Int(1, 3),
            new Vector2Int(1, 2), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(2, 0),
            new Vector2Int(3, 0), new Vector2Int(4, 0), new Vector2Int(4, 1), new Vector2Int(4, 2),
            new Vector2Int(5, 2), new Vector2Int(6, 2), new Vector2Int(7, 2), new Vector2Int(7, 1),
            new Vector2Int(7, 0), new Vector2Int(8, 0), new Vector2Int(9, 0)
        }

    }; // List of solution paths
    public static Vector2Int[][] nonwalkables = new Vector2Int[][]
    {
        new Vector2Int[] {
            new Vector2Int(5, 0), new Vector2Int(9, 0), new Vector2Int(1, 1),/* new Vector2Int(3, 1),*/
            new Vector2Int(4, 1), new Vector2Int(5, 1), new Vector2Int(7, 1), /*new Vector2Int(4, 2),*/
            new Vector2Int(7, 2), new Vector2Int(9, 2), new Vector2Int(1, 3),/* new Vector2Int(2, 3),*/
            new Vector2Int(6, 3), new Vector2Int(7, 3), new Vector2Int(9, 3), /*new Vector2Int(1, 4),*/
            new Vector2Int(2, 4), new Vector2Int(4, 4), new Vector2Int(5, 4), /*new Vector2Int(1, 5),*/
            new Vector2Int(5, 5), new Vector2Int(7, 5), new Vector2Int(9, 6),
            new Vector2Int(1, 7), new Vector2Int(5, 7), new Vector2Int(7, 7), new Vector2Int(0, 8),
            new Vector2Int(3, 8), new Vector2Int(5, 8), new Vector2Int(7, 8), new Vector2Int(8, 8),
            new Vector2Int(0, 9), new Vector2Int(3, 9), new Vector2Int(5, 9),
            /*new Vector2Int(1,7), new Vector2Int(2, 7), new Vector2Int(3, 7), new Vector2Int(4, 7), new Vector2Int(5, 7)*/


        },
        new Vector2Int[] {},
        new Vector2Int[] {},
        new Vector2Int[] {},
    }; 

    

    public enum MazeType
    {
        Maze0, Maze1
    }
    public MazeType mazeType;


    // I called this Awake function so the PlayerControllerScript can collect the solution path it needs before startup.
    private void Awake()
    {
        /*choice = (mazeType == MazeType.Maze0) ? 0 : 1;*/
        choice = 3;
    }

    void Start()
    {

        DrawGrid();
        GenerateBounds();

    }
    void DrawGrid()
    {
        /*Depending on the `choice` it draws the specified grid*/

        foreach (var point in nonwalkables[choice])
        {
            GameObject wallCube = Instantiate(cubePrefab, new Vector3(point.x, 0, point.y), Quaternion.identity);
            wallCube.tag = "Wall";
        }
    }

    void GenerateBounds()
    {
        /* Very Long and Convoluted function that essentially just draws the borders and ground of the maze. 
         * The most important take away is that TO LIGHT UP THE SOLUTION PATH TO THE MAZE uncomment the block that says " GLOW" */


        List<Vector2Int> path = paths[choice].ToList();
        Renderer prefabRenderer = cubePrefab.GetComponent<Renderer>();
        float offset = (prefabRenderer.bounds.size.x) / 2;
        Vector2Int bounds = new Vector2Int(10, 10);

        for (int x = 0; x < bounds.x; x++)
        {
            for (int y = 0; y < bounds.y; y++)
            {
                // Instantiate floor at every grid position
                GameObject tile = Instantiate(boundPrefab, new Vector3(x, -offset, y), Quaternion.identity);
                // Name the tile based on its grid coordinates
               
                tile.name = $"Tile_{x}_{y}";
                

                // GLOW - UNCOMMENT TO LIGHT UP SOLUTION PATH
                if (path.Contains(new Vector2Int(x, y)))
                {
                    tile.tag = "PathTile";
                    tile.GetComponent<Renderer>().material = pathTileMaterial;

                }
                else
                {
                    tile.tag = "Floor";
                    tile.GetComponent<Renderer>().material = tiledefMaterial;
                }
                // GLOW 

                // Check if the position is on the boundary of the grid
                if (x == 0 || x == bounds.x - 1 || y == 0 || y == bounds.y - 1)
                {
                    // Instantiate walls at the boundary positions
                    float rotationAngle = 90;
                    if (x == 0 || x == bounds.x - 1)
                    {
                        float x_offset = (x == 0) ? -offset : bounds.x - offset;
                        Instantiate(boundPrefab, new Vector3(x_offset, 0, y), Quaternion.Euler(new Vector3(0, 0, rotationAngle)));
                    }
                    else if (y == 0 || y == bounds.y - 1)
                    {
                        float y_offset = (y == 0) ? -offset : bounds.y - offset;
                        Instantiate(boundPrefab, new Vector3(x, 0, y_offset), Quaternion.Euler(new Vector3(rotationAngle, 0, 0)));
                    }
                }
            }
        }
    }

    

}













