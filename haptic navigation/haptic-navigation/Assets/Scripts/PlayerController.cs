using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    List<Vector2Int> path;

    void Start()
    {
        path = MazeGenerator.paths[MazeGenerator.choice].ToList();
    }
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction .magnitude < speed)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) *Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            characterController.Move(direction * speed * Time.deltaTime);
  
        }
        CheckAndGuidePath();
    }


void CheckAndGuidePath()
{
        Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        if (!path.Contains(currentPos))
    {
        Vector2Int nearestPoint = FindNearestPathPoint(currentPos);
        Vector2Int directionVector = nearestPoint - currentPos;
        ProvideDirection(directionVector);
    }
}

Vector2Int FindNearestPathPoint(Vector2Int currentPos)
{
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

    void ProvideDirection(Vector2Int directionVector)
    {
        if (Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y))
        {
            if (directionVector.x > 0)
            {
                Debug.Log("Move right");
            }
            else
            {
                Debug.Log("Move left");
            }
        }
        else
        {
            if (directionVector.y > 0)
            {
                Debug.Log("Move up");
            }
            else
            {
                Debug.Log("Move down");
            }
        }
    }
}



