using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    //Width of the entire maze
    [SerializeField]
    [Range(1, 50)]
    private int width = 10;

    //Length of the entire maze
    [SerializeField]
    [Range(1, 50)]
    private int length = 10;

    //Single wall that will copied everywhere
    [SerializeField]
    private Transform wallPrefab = null;

    //Floor for the maze
    [SerializeField]
    private Transform floorPrefab = null;

    //Enter/Start for the maze
    [SerializeField]
    private Transform navigatorPrefab = null;

    //Exit/End for the maze
    [SerializeField]
    private Transform destinationPrefab = null;

    public Vector3 startPosition = new Vector3(0, 0, 0);

    [SerializeField]
    private GameObject player;

    //Cell size
    [SerializeField]
    private float size = 1f;

    //Seed to get a maze
    [SerializeField]
    [Range(1, 50)]
    private int seed = 42;

    // Start is called before the first frame update
    void Start()
    {
        var maze = MazeGenerator.GenerateMaze(width, length, seed);
        Draw(maze);
       
    }

    private void Draw(WallState[,] maze) 
    {
        var floor = Instantiate(floorPrefab, transform);
        floor.localScale = new Vector3(width, 1, length);
        for (int i= 0; i< width; ++i)
        {
            for (int j = 0; j < length; ++j)
            {
                var cell = maze[i,j];
                var position = new Vector3(-width / 2 + i, 0, -length/2 + j) ;

                if ((i == 0 && j == 0) || (i == width - 1 && j == length - 1))
                {
                    // Instantiate your start/end geometry at the opening position
                    if (i == 0)
                    {
                        var enter = Instantiate(navigatorPrefab, transform);
                        enter.position = position + new Vector3(-size / 2, 0, 0);
                        enter.rotation = Quaternion.Euler(0, 90, 0);
                        startPosition = enter.position;
                        player.transform.position = new Vector3(startPosition.x, startPosition.y + (float)0.50, startPosition.z);
                        player.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }

                    if (i == width - 1)
                    {
                        var exit = Instantiate(destinationPrefab, transform);
                        exit.position = position + new Vector3(size / 2, 0, 0);
                    }
                }

                if (cell.HasFlag(WallState.Front))
                {
                    var frontWall = Instantiate(wallPrefab, transform) as Transform;
                    frontWall.position = position + new Vector3(0, 0, size/2);
                    frontWall.localScale = new Vector3(size, frontWall.localScale.y, frontWall.localScale.z);
                }

                if (cell.HasFlag(WallState.Left))
                {
                    var leftWall = Instantiate(wallPrefab, transform) as Transform;
                    leftWall.position = position + new Vector3(-size / 2, 0, 0);
                    leftWall.localScale = new Vector3(size, leftWall.localScale.y, leftWall.localScale.z);
                    leftWall.eulerAngles = new Vector3(0, 90, 0);
                }
                if (i == width - 1)
                {
                    if (cell.HasFlag(WallState.Right))
                    {
                        var rightWall = Instantiate(wallPrefab, transform) as Transform;
                        rightWall.position = position + new Vector3(+size / 2, 0, 0);
                        rightWall.localScale = new Vector3(size, rightWall.localScale.y, rightWall.localScale.z);
                        rightWall.eulerAngles = new Vector3(0, 90, 0);
                    }
                }
                if (j == 0)
                {
                    if (cell.HasFlag(WallState.Rear))
                    {
                        var rearWall = Instantiate(wallPrefab, transform) as Transform;
                        rearWall.position = position + new Vector3(0, 0, -size / 2);
                        rearWall.localScale = new Vector3(size, rearWall.localScale.y, rearWall.localScale.z);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
