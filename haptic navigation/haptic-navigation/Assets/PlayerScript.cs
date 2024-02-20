using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour

{
    public MazeRenderer mazeRenderer;

    private Vector3 position = new Vector3(0, 0, 0);
    // Start is called before the first frame update
    void Start()
    {
        Vector3 temp = mazeRenderer.startPosition;
        Debug.Log(temp);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
