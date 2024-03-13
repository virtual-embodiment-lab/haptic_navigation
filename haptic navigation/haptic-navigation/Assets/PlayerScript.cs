using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerScript : MonoBehaviour

{
    public float moveSpeed = 5f;
    private NavMeshPath navPath;
    public Transform Destination;

    // Start is called before the first frame update
    void Start()
    {
        navPath = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, moveSpeed * Time.deltaTime);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Ayyyee");

            NavMesh.CalculatePath(transform.position, Destination.position, NavMesh.AllAreas, navPath);
            for (int i = 0; i < navPath.corners.Length - 1; i++)
            {
                Debug.Log("Draw");
                Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.white);
            }
            Vector3 firstSegmentDirection = (navPath.corners[1] - navPath.corners[0]).normalized;
            Debug.DrawRay(navPath.corners[0], firstSegmentDirection, Color.red, 5);
        }
     

    }

 }

