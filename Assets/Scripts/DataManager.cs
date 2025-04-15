using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;
using Unity.VisualScripting;

public class DataManager : MonoBehaviour
{

    [SerializeField]
    public List<Frame> frames = new List<Frame>();
    public int flagsCollected = 0;
    public int fireCount = 0;
    public Transform player;
    public Transform head;
    public Spline spline;
    public TrialManager trialManager;
    public HoverBoard hoverBoard;
    public UIManager uiManager;
    public ArduinoReader arduinoReader;
    long unixTime;

    [Serializable]
    public struct Frame{
        public float time;
        public Vector3 position;
        public Vector3 headPosition;
        public Vector3 swayVelocity;
        public float swayVelocityMagnitude;
        public HoverBoard.MovementMode movementMode;
        public TrialManager.TrialType trialType;
        public int flagsCollected;
        public bool isFiring;
        public int fireCount;
        public float deviation;
        public ArduinoReader.SensorData sensorData;
    }

    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    public int lengthOfLineRenderer = 20;

    LineRenderer lineRenderer;
    private List<Vector3> linePositions = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {

        DateTime currentTime = DateTime.UtcNow;
        unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.positionCount = 0;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddFrame()
    {
        var frame = new Frame();
        Vector3 prevHeadPos = Vector3.zero;
        float prevTime = 0;

        // Compare head position to previous. If first, set to current
        if(head != null)
        {
            if(frames.Count == 0) 
            { 
                prevHeadPos = head.position;
                frame.swayVelocity = Vector3.zero;
                frame.swayVelocityMagnitude = 0;

            }
            else
            {
                prevHeadPos = frames[frames.Count - 1].headPosition;
                prevTime = frames[frames.Count - 1].time;
                frame.swayVelocity = (head.position - prevHeadPos) / (Time.time - prevTime);
                frame.swayVelocityMagnitude = frame.swayVelocity.magnitude;
            }
        }
        

        frame.time = Time.time;
        frame.position = player.position;
        if (head != null)
            frame.headPosition = head.position;

        

        frame.trialType = trialManager.trialMode;
        frame.movementMode = hoverBoard.movementMode;
        frame.flagsCollected = flagsCollected;
        frame.isFiring = uiManager.enableMove;
        frame.fireCount = fireCount;

        frame.sensorData = arduinoReader.sensorData;
        //if (trialManager.trialMode == TrialManager.TrialType.RaceTrack)
        //{
        //    Vector3 nearestPoint = CurveUtility.GetNearestPoint(spline, (float3)player.position, out float t);
        //}

        //if(linePositions.Count >0)
        //{
        //    Vector3 offset = linePositions[linePositions.Count - 1] - frame.position;
        //    float sqrLen = offset.magnitude;
        //    if (sqrLen < 2.5f)
        //    {
        //        linePositions.Add(frame.position);

        //        // Update LineRenderer positions if necessary
        //        if (linePositions.Count > 1)
        //        {
        //            lineRenderer.positionCount = linePositions.Count;
        //            //lineRenderer.SetPositions(linePositions.ToArray());
        //        }
        //    }
        //}
        //else
        //{
        //    linePositions.Add(frame.position);

        //    // Update LineRenderer positions if necessary
        //    if (linePositions.Count > 1)
        //    {
        //        lineRenderer.positionCount = linePositions.Count;
        //        //lineRenderer.SetPositions(linePositions.ToArray());
        //    }
        //}



        frames.Add(frame);

        
    }

    private void FixedUpdate()
    {
        if(uiManager.hb.boardEnabled)
        {
            //lineRenderer.SetPositions(linePositions.ToArray());
        }
    }

    public static float CalculateRMSE(List<float> differences)
    {
        //if (differences == null || differences.Count == 0)
        //{
        //    throw new ArgumentException("The list of differences cannot be null or empty.");
        //}

        float sumOfSquares = 0f;

        // Step 1: Square each difference and sum them
        foreach (float diff in differences)
        {
            sumOfSquares += diff * diff;
        }

        // Step 2: Calculate the mean of the squared differences
        float meanSquare = sumOfSquares / differences.Count;

        // Step 3: Take the square root of the mean to get RMSE
        float rmse = Mathf.Sqrt(meanSquare);

        return rmse;
    }

    float GetMin(List<float> values)
    {
        float min = float.PositiveInfinity;
        foreach (float value in values)
        {
            if (value < min)
                min = value;
        }
        return min;
    }

    float GetMax(List<float> values)
    {
        float max = float.NegativeInfinity;
        foreach (float value in values)
        {
            if (value > max)
                max = value;
        }
        return max;
    }

    float GetMean(List<float> values)
    {
        float sum = 0f;
        foreach (float value in values)
        {
            sum += value;
        }
        return sum / values.Count;
    }

    float GetStandardDeviation(List<float> values, float mean)
    {
        float sumOfSquares = 0f;
        foreach (float value in values)
        {
            float diff = value - mean;
            sumOfSquares += diff * diff;
        }
        return Mathf.Sqrt(sumOfSquares / values.Count);
    }

    float SampleSpline(SplineContainer splineContainer, Vector3 playerPos, int samplesPerSpline=1000)
    {
        if (splineContainer != null && playerPos != null)
        {
            float minDistanceSqr = float.PositiveInfinity; // Initialize to a large value
            float3 nearestPoint = float3.zero;

            foreach (var spline in splineContainer.Splines)
            {
                for (int i = 0; i <= samplesPerSpline; i++)
                {
                    // Calculate the t value for this sample
                    float t = i / (float)samplesPerSpline;

                    // Sample the spline at the t value
                    float3 splinePoint = spline.EvaluatePosition(t);

                    // Calculate the squared distance from the player to this point
                    float distanceSqr = math.distancesq(playerPos, splinePoint);

                    // If this point is closer, update the nearest point and minimum distance
                    if (distanceSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distanceSqr;
                        nearestPoint = splinePoint;
                    }
                }
            }

            // Use the nearest point information
            //Debug.DrawLine(player.position, nearestPoint, Color.red);
            //Debug.Log($"Nearest point on spline: {nearestPoint}, Distance: {math.sqrt(minDistanceSqr)}");
            return math.sqrt(minDistanceSqr);
        }
        else
            return -1;
    }

    public void Write()
    {
        uiManager.SetPlay(false);
        lineRenderer.positionCount = 0;
        string boardType = Enum.GetName(typeof(ArduinoReader.BoardType), arduinoReader.boardType);

        if (frames.Count == 0)
        {
            return;
        }
        var path = "Assets/Results/" + boardType + frames[0].movementMode.ToString() + frames[0].trialType.ToString() + unixTime.ToString() + ".csv";
        var path1 = "Assets/Results/" + boardType + frames[0].movementMode.ToString() + frames[0].trialType.ToString() + unixTime.ToString() + "_Summary.csv";
        if (System.IO.File.Exists(path))
        {
            path = "Assets/Results/" + boardType + frames[0].movementMode.ToString() + frames[0].trialType.ToString() + unixTime.ToString() + "B.csv";
            path1 = "Assets/Results/" + boardType + frames[0].movementMode.ToString() + frames[0].trialType.ToString() + unixTime.ToString() + "B_Summary.csv";
        }

        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("Index, Movement Mode, Trial Type, Time, X, Y, Z, Flags Collected, isFiring, Fire Count, RMSE Distance, Sway V X, Sway V Y, Sway V Z, Sway V Magnitude, stg0,stg1,stg2,stg3,horizontal,vertical");

   
    int i = 0;

        float sumDistance = 0;
        
        float endTime = frames[frames.Count-1].time;
        float totalTime = 0;
        float startTime = frames[0].time;
        float averageSpeed = 0;
        int flagsCollected = frames[frames.Count - 1].flagsCollected;
        int fireCount = frames[frames.Count - 1].fireCount;
        float timeFiring = 0;
        string movementMode = frames[0].movementMode.ToString();
        string trialType = frames[0].trialType.ToString();
        List<float> rmseDistances = new List<float>();
        List<float> swayMags = new List<float>();

        float rmseDistance = 0;

        int tIndex = 0;
        int maxTIndex = frames.Count - 1;

        foreach (var frame in frames)
        {

            if (tIndex != maxTIndex)
            {
                sumDistance += Vector3.Distance(frame.position, frames[tIndex + 1].position);

                if(frame.isFiring)
                {
                    timeFiring += frames[tIndex + 1].time - frame.time;
                }
                
            }
            else
            {
                endTime = frame.time;
                //flagsCollected = frame.flagsCollected;
                //fireCount = frame.fireCount;
                //movementMode = frame.movementMode.ToString();
                //trialType = frame.trialType.ToString();
            }

            // Only perform calculations on tracks with splines
            if(trialManager.trialMode == TrialManager.TrialType.RaceTrack)
            {
                Debug.Log("RaceTrack add RSME");
                rmseDistance = SampleSpline(trialManager.trackContainer.GetComponent<SplineContainer>(), frame.position);
                rmseDistances.Add(rmseDistance);
            }
            
            swayMags.Add(frame.swayVelocityMagnitude);

            //TODO
            writer.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}"
                , i, 
                frame.movementMode.ToString(), 
                frame.trialType.ToString(), 
                frame.time,
                frame.position.x, 
                frame.position.y, 
                frame.position.z,
                frame.flagsCollected,
                frame.isFiring, 
                frame.fireCount,
                rmseDistance,
                frame.swayVelocity.x,
                frame.swayVelocity.y,
                frame.swayVelocity.z,
                frame.swayVelocityMagnitude,
                frame.sensorData.stg0,
                frame.sensorData.stg1,
                frame.sensorData.stg2,
                frame.sensorData.stg3,
                frame.sensorData.horizontal,
                frame.sensorData.vertical
                ));

            i++;
            tIndex++;
        }

        writer.Close();

        totalTime = endTime - startTime;
        averageSpeed = sumDistance / totalTime;
        float RSME = CalculateRMSE(rmseDistances);

        float sm_min = GetMin(swayMags);
        float sm_max = GetMax(swayMags);
        float sm_mean = GetMean(swayMags);
        float sm_stdDev = GetStandardDeviation(swayMags, sm_mean);

        StreamWriter sw1 = new StreamWriter(path1);

        sw1.WriteLine("Movement Mode, Trial Type, Total Distance, Total Time, Average Speed, Flags Collected, Fire Count, Time Firing, RSME, Sway Min, Sway Max, Sway Mean, Sway SD ");
        sw1.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}",
            movementMode,
            trialType,
            sumDistance.ToString(),
            totalTime.ToString(),
            averageSpeed.ToString(),
            flagsCollected.ToString(),
            fireCount.ToString(),
            timeFiring.ToString(),
            RSME.ToString(),
            sm_min.ToString(),
            sm_max.ToString(),
            sm_mean.ToString(),
            sm_stdDev.ToString()
            ));

        sw1.Close();

        ResetData();

    }

    public void ResetData()
    {
        frames.Clear();
        flagsCollected = 0;
        fireCount = 0;
    }
}
