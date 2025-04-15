using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class TrialManager : MonoBehaviour
{
    public enum LocomotionType
    {
        BalanceBoard_Horizontal =0,
        BalanceBoard_Vertical =1,
        SpringBoard_Horizontal =2,
        SpringBoard_Vertical =3,
        WalkInPlace = 4,
    }

    public LocomotionType locomotionMode;


    public enum TrialType
    {
        FreeMode = 0,
        StraightLine = 1,
        EvenZig = 2,
        LeftCurve = 3,
        RightCurve = 4,
        SinCurve = 5,

        FullCourse = 6,
        RaceTrack = 7,
        ScatteredMap = 8,

        Slope_None =9,
        Slope_LowAmp_LowFreq= 10,
        Slope_LowAmp_HighFreq = 11,
        Slope_HighAmp_LowFreq = 12,
        Slope_HighAmp_HighFreq = 13,
    }


    public SpeedMode speedMode;
    public enum SpeedMode
    {
        Walk,
        Jog,
        Sprint,
        Fastest
    }

    public TrialType trialMode;

    public delegate void TrialTypeDelegate(int count, float offset);

    TrialTypeDelegate trialTypeDelegate;

    public DataManager dataManager;


    public Transform player;
    public Transform board;

    public GameObject prefab;

    public GameObject trackContainer;
    public GameObject slopeContainer;
    public GameObject planeContainer;

    public List<GameObject> flags = new List<GameObject>();

    public int count_Flags = 10;
    public float offset_Flags = 2;

    public GameObject flag_Parent;

    public TMP_Text flagText;
    public TMP_Text movementText;
    public TMP_Text boardText;
    public TMP_Text toggleText;

    public UIManager uiManager;

    public bool trialRunning= false;

    public List<GameObject> orderCleared = new List<GameObject>();

    public bool toggleToMove = true;

    private float amplitude, frequency = 0f;
    private int numWaves  = 5;


    

    // Start is called before the first frame update
    void Start()
    {
        trackContainer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        flagText.text = trialMode.ToString();
        movementText.text = board.GetComponent<HoverBoard>().movementMode.ToString();
        boardText.text = Enum.GetName(typeof(ArduinoReader.BoardType), board.GetComponent<HoverBoard>().arduino.boardType);
        toggleText.text = "Toggle to move: " + toggleToMove.ToString();

    }

    private void FixedUpdate()
    {
        if (trialRunning)
        {
            dataManager.AddFrame();
            if (flags.Count == dataManager.flagsCollected && flags.Count > 0)
            {
                ClearTrial();

            }
        }
    }

    public void ClearTrial()
    {
        trialRunning = false;
        foreach (var item in flags)
        {
            Destroy(item.gameObject);
        }
        flags.Clear();
        orderCleared.Clear();

        trackContainer.SetActive(false);
        

        dataManager.Write();

        //Set player to origin and turnoff modes
        //ResetPosition();
    }

    public void SetLocomotionMode(int l_mode)
    {
        locomotionMode = (LocomotionType)l_mode;

  
        
    }

    public void SetTrialMode(int t_mode)
    {

        trialMode = (TrialType)t_mode;


    }

    IEnumerator ResetPosition()
    {
        //player.transform.position = Vector3.zero;
        board.GetComponent<Rigidbody>().velocity = Vector3.zero;
        board.GetComponent<Rigidbody>().isKinematic = true;
        board.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
        board.transform.position = new Vector3(0, board.transform.position.y , 0);
        board.transform.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(1);
        board.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        board.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void InitTrial()
    {
        StartCoroutine("ResetPosition");
        switch(locomotionMode)
        {
            case LocomotionType.BalanceBoard_Horizontal:
                Init_BalanceBoard_Horizontal();
                break;

            case LocomotionType.BalanceBoard_Vertical:
                Init_BalanceBoard_Vertical();
                break;

            case LocomotionType.SpringBoard_Horizontal:
                Init_SpringBoard_Horizontal();
                break;

            case LocomotionType.SpringBoard_Vertical:
                Init_SpringBoard_Vertical();
                break;

            case LocomotionType.WalkInPlace:
                Init_WalkInPlace();
                break;
        }

        trialRunning = true;
        slopeContainer.SetActive(false);

        SpawnTrial();

    }


    void SpawnTrial()
    {

        //uiManager.enableMove = true;

        switch (trialMode)
        {
            case TrialType.FreeMode:
                trialTypeDelegate = Freemode;
                break;
 
            case TrialType.StraightLine:
                trialTypeDelegate = Straight;
                break;

            case TrialType.EvenZig:
                trialTypeDelegate = EvenZig;
                break;

            case TrialType.RightCurve:
                trialTypeDelegate = RightCurve;
                break;

            case TrialType.LeftCurve:
                trialTypeDelegate = LeftCurve;
                break;

            case TrialType.SinCurve:
                trialTypeDelegate = SinCurve;
                break;

            case TrialType.FullCourse:
                trialTypeDelegate = FullCourse;
                break;

            case TrialType.RaceTrack:
                trialTypeDelegate = RaceTrack;
                break;

            case TrialType.Slope_None:
                trialTypeDelegate = Slopes;
                amplitude = 0;
                frequency = 0;
                numWaves = 5;
                break;

            case TrialType.Slope_LowAmp_LowFreq:
                trialTypeDelegate = Slopes;
                amplitude = 1;
                frequency = 0.1f;
                numWaves = 4;
                break;

            case TrialType.Slope_LowAmp_HighFreq:
                trialTypeDelegate = Slopes;
                amplitude = 1;
                frequency = 0.2f;
                numWaves = 8;
                break;

            case TrialType.Slope_HighAmp_LowFreq:
                trialTypeDelegate = Slopes;
                amplitude = 2;
                frequency = 0.1f;
                numWaves = 4;
                break;

            case TrialType.Slope_HighAmp_HighFreq:
                trialTypeDelegate = Slopes;
                amplitude = 2;
                frequency = 0.2f;
                numWaves = 8;
                break;
        }

        trialTypeDelegate(count_Flags,offset_Flags);
    }

    private void Freemode(int count,float offset=2)
    {

    }

    #region SpawnObstacles
    private void Straight(int count, float offset = 2)
    {
        Vector3 startPosition = Vector3.zero + new Vector3(0,0,offset);

        for (int i = 0; i < count; i++)
        {
            Vector3 position = startPosition + new Vector3(0, 0, offset * i);

            SpawnFlag(position);
        }
    }

    private void EvenZig(int count, float offset = 2)
    {
        Vector3 startPosition = Vector3.zero + new Vector3(0, 0, offset);


        for (int i = 0; i < count; i++)
        {
            Vector3 position;
            //if (i ==0)
            //{
            //    position = startPosition + new Vector3(Mathf.Sin(0), 0, offset * i);
            //}
            //else
            //{
            int neg = 1;
            if(i%2 ==0)
            {
                neg = -1;
            }
            
            print(Mathf.Sin(i / 2 *Mathf.PI));
            position = startPosition + new Vector3( offset * neg, 0, offset * i);
            //}

            SpawnFlag(position,1);
        }
    }

    private void SinCurve(int count, float offset = 2)
    {
        Vector3 startPosition = Vector3.zero + new Vector3(0, 0, offset);


        float maxPosX = count * offset;

        float offsetFraction = 1 / 2 * Mathf.PI;


        for (int i = 0; i < count; i++)
        {
            Vector3 position;
            //if (i ==0)
            //{
            //    position = startPosition + new Vector3(Mathf.Sin(0), 0, offset * i);
            //}
            //else
            //{

            print(Mathf.Sin(i / 2 * Mathf.PI));
            position = startPosition + new Vector3(offset * Mathf.Sin(i), 0,  offset * i);
            //}

            SpawnFlag(position);
        }
    }

    private void RightCurve(int count, float offset = 2)
    {
        Vector3 startPosition = Vector3.zero + new Vector3(0, 0, offset);

        float maxPosX = count * offset;

        

        Vector3 centerPoint = new Vector3(maxPosX / 2, 0, maxPosX / 2);

        for (int i = 0; i < count; i++)
        {
            float offsetNew = Mathf.Sqrt(100 * offset * offset / (i * i + 100));
            float lerp = (offset *i) / maxPosX;
            Vector3 position = startPosition + new Vector3(1 *offsetNew *i *lerp , 0, offsetNew * i); //Mathf.Pow(i, 2)

            SpawnFlag(position);
        }
    }

    private void LeftCurve(int count, float offset = 2)
    {
        Vector3 startPosition = Vector3.zero + new Vector3(0, 0, offset);

        float maxPosX = count * offset;

        Vector3 centerPoint = new Vector3(maxPosX / 2, 0, maxPosX / 2);

        for (int i = 0; i < count; i++)
        {
            float j = i;
            float lerp = (offset * i) / maxPosX;

            float offsetNew = Mathf.Sqrt(100 * offset * offset / (i * i + 100));
            if (i == 0)
                j = 0.1f;
            Vector3 position = startPosition + new Vector3(-1 *offsetNew * i * lerp, 0, offsetNew * i);

            SpawnFlag(position);
        }
    }

    

    void SpawnFlag(Vector3 position, int mo = 0)
    {
        GameObject point = GameObject.Instantiate(prefab, position, Quaternion.identity);
        point.name = flags.Count.ToString();
        if(mo == 0)
        {
            if (flags.Count > 0)
            {
                flags[flags.Count - 1].transform.LookAt(point.transform.position);
            }
            else
                point.transform.LookAt(Vector3.zero);

            if (flags.Count == count_Flags - 1)
            {
                
                point.transform.LookAt(flags[flags.Count - 1].transform.position);
            }

            
        }

        else
        {
            if (flags.Count > 0)
            {
                point.transform.LookAt(flags[flags.Count - 1].transform.position);
            }

           

            else
                point.transform.LookAt(Vector3.zero);
        }
        

        flags.Add(point);
        point.transform.parent = flag_Parent.transform;
    }

    #endregion

    #region fullcourse
    private Vector3 currentPosition = Vector3.zero;
    private Quaternion currentRotation = Quaternion.identity;

    private void Straight_Full(int count, float offset = 2)
    {
        for (int i = 0; i < count; i++)
        {
            currentPosition += currentRotation * new Vector3(0, 0, offset);
            SpawnFlag(currentPosition);
        }
    }

    private void Curved_Full(int count, float totalAngle, float offset = 2)
    {
        float angleIncrement = totalAngle / count;
        Quaternion rotationStep = Quaternion.Euler(0, angleIncrement, 0);

        for (int i = 0; i < count; i++)
        {
            currentRotation *= rotationStep;
            Vector3 direction = currentRotation * Vector3.forward;
            currentPosition += direction * offset;
            SpawnFlag(currentPosition);
        }
    }

    private void Sinusoidal_Full(int count, float offset = 2, float amplitude = 1, float frequency = 1)
    {
        for (int i = 0; i < count; i++)
        {
            float z = offset;
            float x = amplitude * Mathf.Sin(frequency * i);
            Vector3 localOffset = new Vector3(x, 0, z);
            currentPosition += currentRotation * localOffset;
            SpawnFlag(currentPosition);

            // Increase amplitude for next cycle
            amplitude += 0.1f;
        }
    }

    private void FullCourse(int count, float offset = 2)
    {
        currentPosition = Vector3.zero;
        currentRotation = Quaternion.identity;
        float spacing = 10;
        //currentDirection = Vector3.forward;
        //angleAccumulator = 0f;

        // Example sequence of sections
        Straight_Full(5,5);

        Curved_Full(10, -15, spacing);  // 15 degrees to the left
        Curved_Full(5, 15, spacing);   // 15 degrees to the right
        Curved_Full(5, 15, spacing);   // 15 degrees to the right
        Curved_Full(5, -15, spacing);  // 15 degrees to the left

        Curved_Full(5, -45, spacing);  // 15 degrees to the left
        Curved_Full(5, 45, spacing);   // 15 degrees to the right
        Curved_Full(5, 45, spacing);   // 15 degrees to the right
        Curved_Full(5, -45, spacing);  // 15 degrees to the left

        Straight_Full(5, spacing);
        //Sinusoidal_Full(10, spacing);
        Curved_Full(5, 45, spacing);   // 15 degrees to the right
        Curved_Full(5, -45, spacing);  // 15 degrees to the left
        Curved_Full(5, -45, spacing);  // 15 degrees to the left
        Curved_Full(5, 45, spacing);   // 15 degrees to the right
        //Curved_Full(10, -50);  // 15 degrees to the left
        //Curved_Full(10, 75);   // 15 degrees to the right
        //Curved_Full(10, 90);
        //Curved_Full(10, 45);


        //Sinusoidal_Full(10);
    }
    #endregion

    public void RaceTrack(int count, float offset=2)
    {

        trackContainer.SetActive(true);

        Vector3 startPosition = Vector3.zero + new Vector3(0, 0, offset);

        SpawnFlag(startPosition);

        Vector3 endPosition = Vector3.zero + new Vector3(0, 0, -offset);

        SpawnFlag(endPosition);
    }

    #region LocomotionTypes
    void Init_BalanceBoard_Horizontal()
    {
        //TODO
    }

    void Init_BalanceBoard_Vertical()
    {
        //TODO
    }

    void Init_SpringBoard_Horizontal()
    {
        //TODO
    }

    void Init_SpringBoard_Vertical()
    {
        //TODO
    }

    void Init_WalkInPlace()
    {
        //TODO
    }

    #endregion



    public void ToggleMove()
    {
        toggleToMove = !toggleToMove;
    }

    public void Slopes(int count, float offset=2)
    {
        slopeContainer.SetActive(true);
        planeContainer.SetActive(false);
        board.transform.rotation = Quaternion.Euler(0, 90, 0);
        Vector3 startPosition = Vector3.zero + new Vector3(0, 0, offset);

        SpawnFlag(startPosition);

        Vector3 finishPosition = Vector3.zero + new Vector3(0, 0, offset*70);

        SpawnFlag(finishPosition);

        slopeContainer.GetComponent<SinWaveTerrain>().frequency = frequency;
        slopeContainer.GetComponent<SinWaveTerrain>().amplitude = amplitude;
        slopeContainer.GetComponent<SinWaveTerrain>().numCompleteWaves = numWaves;
        slopeContainer.GetComponent<SinWaveTerrain>().Generate();
    }
}
