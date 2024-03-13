using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationPath : MonoBehaviour
{
    public Transform Destination;
    private NavMeshPath navPath;
    // Start is called before the first frame update
    void Start()
    {
        navPath = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {
        NavMesh.CalculatePath(transform.position, Destination.position, NavMesh.AllAreas, navPath);
        for (int i = 0; i < navPath.corners.Length - 1; i++)
        {
            Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.white);
            
        }
    }
}
