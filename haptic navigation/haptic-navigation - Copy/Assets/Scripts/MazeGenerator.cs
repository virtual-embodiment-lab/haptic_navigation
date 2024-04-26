using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Security.Cryptography;

[Flags]
public enum WallState
{
    // Bit representation of the walls
    // 0000 -> No Walls
    // 1111 -> Walls: Left, Right, Front, and Back
    Left = 1, // 0001
    Right = 2, // 0010
    Front = 4, // 0100
    Rear = 8, // 1000

    //Recursive backtracking
    //To keep track of which cells have been visited
    Visited = 128, //1000 000
}

//To keep track where we are in the maze, we need Position
public struct Position
{
    public int X;
    public int Y;
}

//To keep track of the walls been broken, we need WallBreak
public struct Neighbor
{
    //Position of Neighbor
    public Position Position;
    // Wall shared with the neighbor
    public WallState SharedWall;
}

public static class MazeGenerator
{
    //A function that returns the opposite side of the SharedWall
    private static WallState GetOppositeWall(WallState wall)
    {
        switch (wall)
        {
            case WallState.Left: return WallState.Right;
            case WallState.Right: return WallState.Left;
            case WallState.Front: return WallState.Rear;
            case WallState.Rear: return WallState.Front;
            default: return WallState.Left;
        }
    }

    //Function to iterate over maze cells and remove walls
    private static WallState[,] RecursiveBacktracker(WallState[,] maze, int width, int length, int seed)
    {
        //1: Choose a random position for initiating
        var ran = new System.Random(seed);

        //1.1: Storing the random visited positions in a stack
        var VisitedPosStack = new Stack<Position>();

        //1.2: Acquiring the random position
        var position = new Position {X = ran.Next(0, width), Y = ran.Next(0, length)};

        //2: Marking the start random position as visited
        maze[position.X, position.Y] |= WallState.Visited;

        //2.2: Storing the visited position in the stack
        VisitedPosStack.Push(position);

        //3: Iterate over the position stack till it becomes empty
        while (VisitedPosStack.Count > 0)
        {
            //Acquire the current position
            var current = VisitedPosStack.Pop();

            //Get the neighbors of the current positions
            var neighbors = GetUnvisitedNeighbors(current, maze, width, length);

            //If the current position has unvisited neighbors
            if (neighbors.Count > 0)
            {
                //Then visit them and store them in position stack
                VisitedPosStack.Push(current);

                //Then find random neighbor of current

                //ranIndex gives a random index between 0 to number of neighbors
                var ranIndex = ran.Next(0, neighbors.Count);
                //Extracting the neighbor at ranIndex
                var randomNeighbor = neighbors[ranIndex];
                
                //Extract the random neighbor's position
                var neighborPos = randomNeighbor.Position;
                //Remove wall at the current position in the maze
                maze[current.X, current.Y] &= ~randomNeighbor.SharedWall;
                //Remove wall at neighbor's position
                maze[neighborPos.X, neighborPos.Y] &= ~GetOppositeWall(randomNeighbor.SharedWall);
                //Mark the neighbor's position as visited
                maze[neighborPos.X, neighborPos.Y] |= WallState.Visited;

                //Push the visited neighbor in the position 
                VisitedPosStack.Push(neighborPos);
            }
        }
        return maze;
    }

    //Storing a list of unvisited neighbors
    //Iterate over all positions
    //Check if each location has the flag Visited, if not then add to the list and return the list of Unvisited Neighbors
    private static List<Neighbor> GetUnvisitedNeighbors (Position p, WallState[,] maze, int width, int length)
    {
        var UnvisitedNeighbors = new List<Neighbor> ();
        if(p.X > 0) //left
        {
            if (!maze[p.X - 1, p.Y].HasFlag(WallState.Visited))
            {
                UnvisitedNeighbors.Add(new Neighbor
                {
                    Position = new Position
                    {
                        X = p.X - 1,
                        Y = p.Y
                    },
                    SharedWall = WallState.Left
                });
            }
        }

        if (p.Y > 0) // rear
        {
            if (!maze[p.X, p.Y - 1].HasFlag(WallState.Visited))
            {
                UnvisitedNeighbors.Add(new Neighbor
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y - 1
                    },
                    SharedWall = WallState.Rear
                });
            }
        }

        if (p.Y < length - 1) // front
        {
            if (!maze[p.X, p.Y + 1].HasFlag(WallState.Visited))
            {
                UnvisitedNeighbors.Add(new Neighbor
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y + 1
                    },
                    SharedWall = WallState.Front
                });
            }
        }

        if (p.X < width - 1) // right
        {
            if (!maze[p.X + 1, p.Y].HasFlag(WallState.Visited))
            {
                UnvisitedNeighbors.Add(new Neighbor
                {
                    Position = new Position
                    {
                        X = p.X + 1,
                        Y = p.Y
                    },
                    SharedWall = WallState.Right
                });
            }
        }

        return UnvisitedNeighbors;
    }

    // A function that generates mazes.
    public static WallState[,] GenerateMaze(int width, int length, int seed)
    {
        //Initiating a new empty Wallstate 2D array called maze.
        WallState[,] maze = new WallState[width, length];

        //Declaring the initial state of the maze. All walls will be up initially, i.e., WallState = 1111.
        WallState initial = WallState.Left | WallState.Right | WallState.Front | WallState.Rear;

        //Creating cells for the maze.
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < length; ++j)
            {
                maze[i, j] = initial;
            }
        }

        // Opening up the starting wall 
        maze[0, 0] &= ~WallState.Left; // Opening the left wall at the start

        // Opening up the ending wall 
        maze[width - 1, length - 1] &= ~WallState.Right; // Opening the right wall at the end

        return RecursiveBacktracker(maze, width, length, seed);
    }

}
