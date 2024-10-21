
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerPositionLogger : MonoBehaviour
{
    // The camera (headset) object in your XR Rig
    public Transform cameraTransform;

    // Path for the CSV file
    private string filePath;

    // How many times per second to record (usually 60 FPS)
    private float recordInterval = 1.0f / 60.0f;

    // To track time between frames
    private float timeSinceLastRecord = 0.0f;

    // The Solution Path to compare to player Position
    List<Vector2Int> path;


    // LOG FILE PATH: C:\Users\Cornellvel\AppData\LocalLow\DefaultCompany\haptic-navigation

    void Start()
    {
        
        path = MazeGenerator.paths[MazeGenerator.choice].ToList();
        // Set the file path to the persistent data path of the application

        // Create a unique filename with timestamp
        string timestamp = DateTime.Now.ToString("MM-dd-yy_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture);
        timestamp = timestamp.Replace("-", ""); // Remove dashes from the timestamp
        timestamp = timestamp.Replace(":", ""); // Remove colons from the timestamp
        filePath = Path.Combine(Application.persistentDataPath, "HapticNavTrackingData_" + timestamp + ".csv");

        // Initialize the CSV file with a header
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Timestamp,HeadsetPositionX,HeadsetPositionY,HeadsetPositionZ,HeadsetRotationPitch,HeadsetRotationYaw,HeadsetRotationRoll,IntPositionX,IntPositionY,OnPath\n");
        }

        // Ensure the cameraTransform is set, default to main camera if not provided
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;  // Assuming the main camera is the player's camera
        }
    }

    void Update()

    {
        // Track time between updates
        timeSinceLastRecord += Time.deltaTime;
        // Record position every frame at 60 FPS (or as close as possible)
        if (timeSinceLastRecord >= recordInterval)
        {
            LogPosition();
            timeSinceLastRecord = 0.0f;  // Reset the time tracker
        }
    }

    void LogPosition()
    {
        // Get the current position of the camera (player's head)
        Vector3 headsetPosition = transform.position;
        Vector2Int headsetIntPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)); // discretizes the current player position
        int onPath = path.Contains(headsetIntPos) ? 1 : 0; // determine if the player is on path 

        // Extract pitch, yaw, and roll from headset rotation
        float pitch = transform.rotation.eulerAngles.x;
        float yaw = transform.rotation.eulerAngles.y;
        float roll = transform.rotation.eulerAngles.z;
        


        // Get the current time
        string timestamp = DateTime.Now.ToString("MM-dd-yy_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture);

        string logData = $"{timestamp},{headsetPosition.x},{headsetPosition.y},{headsetPosition.z}," +
                         $"{pitch},{yaw},{roll},{headsetIntPos.x},{headsetIntPos.y},{onPath}" + $"\n";

        // Append the position data to the CSV file
        File.AppendAllText(filePath, logData);
    }
}