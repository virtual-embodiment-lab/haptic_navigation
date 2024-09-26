using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class NavigationController : MonoBehaviour
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
        transform.GetComponent<NavMeshAgent>().destination = Destination.position;

        /*NavMesh.CalculatePath(transform.position, transform.GetComponent<NavMeshAgent>().destination, NavMesh.AllAreas, navPath);*/

        for (int i = 0; i < navPath.corners.Length - 1; i++)
            Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.yellow);
    }


}
