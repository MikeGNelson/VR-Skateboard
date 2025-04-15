using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class UIManager : MonoBehaviour
{
    public HoverBoard hb;
    public GameObject root;
    public GameObject canvas;
    public GameObject placeMenu;
    public GameObject mountMenu;
    public GameObject helpMenu;
    public GameObject controls;
    public GameObject hide;
    public GameObject show;
    public Transform left;
    public Transform right;

    public bool positioning = false;
    public bool mounting = false;
    public bool mounted = false;
    public bool enableMove = false;

    public float toggleTimer = 0.1f;
    public float toggleTimerValue = 0;

    public List<GameObject> toDisable = new List<GameObject>();
    [SerializeField]
    public List<InputDevice> inputDevices = new List<InputDevice>();

    public TrialManager trialManager;



    // Start is called before the first frame update
    void Start()
    {
        foreach( var d in toDisable )
        {
            if(d != null)
                d.SetActive( false );
        }
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Left, inputDevices);

        InitInputReader();
        //Debug.Log(inputDevices.Count);
        //foreach (var input in inputDevices)
        //{
        //    input.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
        //    //Debug.Log(input.name + " " + input.characteristics);
        //    Debug.Log(input.name + " " + triggerValue);
        //}
    }

    void InitInputReader()
    {
        InputDevices.GetDevices(inputDevices);
        //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Left, inputDevices);

        //Debug.Log(inputDevices.Count);
        foreach (var input in inputDevices)
        {
            input.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
            //Debug.Log(input.name + " " + input.characteristics);
            //Debug.Log(input.name +  " " + triggerValue);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //foreach (var input in inputDevices)
        //{
        //    input.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
        //    //Debug.Log(input.name + " " + triggerValue);
        //}
        if (inputDevices.Count <= 2) 
        {
            InitInputReader();
            
        }
        canvas.transform.LookAt(2 * canvas.transform.position - Camera.main.transform.position);
        if(positioning)
        {
            PositionBoard();
            //If triggers up
            // Mount();
        }

        if (mounting)
        {
            MountBoard();
        }

        if (mounted)
        {
            if(trialManager.toggleToMove)
                CheckCanMove();
            else
            {
                if(toggleTimerValue >= toggleTimer)
                {
                    toggleTimerValue = 0;
                    CheckCanMove() ;
                }
            }
            if(hb.movementMode == HoverBoard.MovementMode.combined)
            {
                CheckCanTurn();
            }
            toggleTimerValue += Time.deltaTime;
        }
    }

    void CheckCanTurn()
    {
        bool enableTurn = false;
        if (!trialManager.trialRunning)
        {
            
            return;
        }

        foreach (var input in inputDevices)
        {
            if (input.TryGetFeatureValue(CommonUsages.grip, out float triggerValue))
            {
                //Debug.Log($"{input.name} Trigger Value: {triggerValue}");

                if (triggerValue > 0.9f)
                {
                    enableTurn = true;
                    SetTurn(enableTurn);
                    return; // Exit early since movement is allowed
                }

            }
        }
        SetTurn(enableTurn);
    }
    void CheckCanMove()
    {
        enableMove = false;

        if (!trialManager.trialRunning)
        {
            SetPlay(false);
            return;
        }

        foreach (var input in inputDevices)
        {
            if (input.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                //Debug.Log($"{input.name} Trigger Value: {triggerValue}");

                if (triggerValue > 0.9f)
                {
                    enableMove = !hb.boardEnabled;
                    SetPlay(enableMove);
                    return; // Exit early since movement is allowed
                }
                
            }
        }
        if(trialManager.toggleToMove)
            SetPlay(enableMove);
        //else
        //    SetPlay(true);
    }
    public void SetPosition(bool mode)
    {
        positioning = mode;
        
    }

    public void PositionBoard()
    {
        Dismount();
        Debug.Log("Position Board");
        Vector3 position = (left.position - right.position) / 2 + right.position;
        root.transform.position = position;
        root.transform.LookAt(right, transform.up);
        root.GetComponent<Rigidbody>().isKinematic = true;

        float triggerTotal = 0;

        foreach (var input in inputDevices)
        {
            input.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
            //Debug.Log(input.name + " " + triggerValue);
            triggerTotal += triggerValue;
        }

        if(triggerTotal > 1.2f)
        {
            positioning = false;
            StartCoroutine("HoldBoard");
        }

    }

    IEnumerator HoldBoard()
    {
        ToggleMenu(placeMenu);
        ToggleMenu(mountMenu);

        yield return new WaitForSeconds(1);
        root.GetComponent<Rigidbody>().isKinematic = false;
        root.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
        StartCoroutine("FreezeMovement");
    }

    IEnumerator FreezeMovement()
    {
        
        yield return new WaitForSeconds(2);
        root.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        mounting = true;
    }

    



    public void SetMovementMode(int mode)
    {
        hb.movementMode = (HoverBoard.MovementMode)mode;
    }


    void MountBoard()
    {
        float triggerTotal = 0;

        foreach (var input in inputDevices)
        {
            input.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
            //Debug.Log(input.name + " " + triggerValue);
            triggerTotal += triggerValue;
        }

        if (triggerTotal > 1.2f)
        {
            mounting = false;
            Mount();
            ToggleMenu(mountMenu);
            ToggleMenu(helpMenu);
            ToggleMenu(controls);
            ToggleMenu(hide);
            ToggleMenu(show);
        }
    }

    // 
    public void Mount()
    {
        positioning = false;
        mounted = true;
        this.transform.parent = root.transform;
    }

    public void Dismount()
    {
        mounted = false;
        this.transform.parent = null;
    }

    public void SetPlay(bool mode)
    {
        hb.boardEnabled = mode;
        //Debug.Log("move board");
    }

    public void SetTurn(bool turn)
    {
        hb.combinedTurning = turn;
    }


    public void ToggleMenu(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }
    
    
}
