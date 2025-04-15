using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For UI button handling
using System.IO;

public class CSVMover : MonoBehaviour
{
    public string csvFileName = "elasticcombinedRaceTrack1734653344.csv"; // The file name in Resources folder
    public GameObject objectToMove; // Object to move along the path
    public Button startButton; // Button to start the movement

    private List<Vector3> positions = new List<Vector3>();
    private List<float> timestamps = new List<float>();
    private int startIndex = -1;

    public bool rotateTowardsDirection = false; // Toggle rotation behavior

    public float rotationSpeed = 180f; // Rotation speed in degrees per second


    void Start()
    {
        LoadCSV();

        if (startButton != null)
        {
            startButton.onClick.AddListener(StartMovement);
        }
    }

    void LoadCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++) // Start at 1 to skip the header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');

            float time = float.Parse(values[3]); // Assuming 'Time' is at index 3
            float x = float.Parse(values[4]);   // Assuming 'X' is at index 4
            float y = float.Parse(values[5]);   // Assuming 'Y' is at index 5
            float z = float.Parse(values[6]);   // Assuming 'Z' is at index 6

            // Safely parse isFiring
            bool isFiring;
            if (!bool.TryParse(values[8], out isFiring)) // Assuming 'isFiring' is at index 9
            {
                Debug.LogWarning($"Invalid boolean value in 'isFiring' column at line {i + 1}: {values[9]}");
                isFiring = false; // Default value if parsing fails
            }

            timestamps.Add(time);
            positions.Add(new Vector3(x, y, z));

            if (isFiring && startIndex == -1)
            {
                startIndex = i - 1; // Set the starting index when 'isFiring' is first true
            }
        }
    }


    void StartMovement()
    {
        if (positions.Count == 0)
        {
            Debug.LogError("No positions loaded from CSV.");
            return;
        }

        if (startIndex == -1)
        {
            Debug.LogError("Start index not found. Ensure 'isFiring' is true in at least one row of the CSV.");
            return;
        }

        if (startIndex < 0 || startIndex >= positions.Count)
        {
            Debug.LogError($"Invalid start index: {startIndex}. Ensure the CSV data is correctly formatted.");
            return;
        }

        StartCoroutine(MoveAlongPath(startIndex));
    }

    IEnumerator MoveAlongPath(int start)
    {
        // Add an initial delay before starting the movement
        float initialDelay = 3f; // Set the delay in seconds
        yield return new WaitForSeconds(initialDelay);

        if (positions.Count <= 1)
        {
            Debug.LogWarning("Not enough positions to move along a path.");
            yield break;
        }

        // Store the initial rotation if rotation is frozen
        Quaternion fixedRotation = objectToMove.transform.rotation;

        for (int i = start; i < positions.Count - 1; i++)
        {
            if (i + 1 >= timestamps.Count)
            {
                Debug.LogWarning("Mismatch between timestamps and positions.");
                yield break;
            }

            Vector3 startPosition = positions[i];
            Vector3 endPosition = positions[i + 1];
            float duration = timestamps[i + 1] - timestamps[i];

            if (duration <= 0)
            {
                Debug.LogWarning($"Invalid duration between points {i} and {i + 1}. Skipping.");
                continue;
            }

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                objectToMove.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);

                if (rotateTowardsDirection)
                {
                    // Rotate smoothly towards the movement direction
                    Vector3 direction = (endPosition - startPosition).normalized;
                    if (direction != Vector3.zero) // Prevent errors with zero-length direction
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        objectToMove.transform.rotation = Quaternion.RotateTowards(
                            objectToMove.transform.rotation,
                            targetRotation,
                            rotationSpeed * Time.deltaTime
                        );
                    }
                }
                else
                {
                    // Freeze rotation
                    objectToMove.transform.rotation = fixedRotation;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            objectToMove.transform.position = endPosition;

            if (!rotateTowardsDirection)
            {
                // Maintain fixed rotation
                objectToMove.transform.rotation = fixedRotation;
            }
        }
    }




}
