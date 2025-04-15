using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class HoverBoard : MonoBehaviour
{
    Rigidbody hb;
    public float mult;

    [SerializeField]
    private float moveForce;

    [SerializeField]
    private float turnTorque;

    public float verticle;
    public float horizontal;

    public Vector3 movementDir = Vector3.zero;


    [SerializeField]
    private float dampening;

    [SerializeField]
    private float strength;

    [SerializeField]
    private float length = 4f;

    [SerializeField]
    private float staticSpeed = 3;

    public bool boardEnabled = false;

    public ArduinoReader arduino;

    public TrialManager trialManager;

    public MovementMode movementMode = MovementMode.torque;
    public enum MovementMode
    {
        torque,
        directional,
        directionalNoAccel,
        torqueNoAccel,
        walkinplace,
        joyStick,
        chair,
        combined,
        oneDir,
        joystickDir
    }
    

    void Start()
    {
        trialManager = GameObject.FindObjectOfType<TrialManager>();
        hb = GetComponent<Rigidbody>();
        //hb.useGravity = false;
    }

    public Transform[] anchors = new Transform[4];
    public RaycastHit[] hits = new RaycastHit[4];

    float[] lastHitDist = new float[4];

    public float MoveForce { get => moveForce; set => moveForce = value; }
    public float TurnTorque { get => turnTorque; set => turnTorque = value; }
    public float Dampening { get => dampening; set => dampening = value; }
    public float Strength { get => strength; set => strength = value; }
    public float Length { get => length; set => length = value; }
    public float StaticSpeed { get => staticSpeed; set => staticSpeed = value; }

    public bool combinedTurning = false;

    void FixedUpdate()
    {
        //Debug.Log(string.Format("{0}, {2}", transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
        for (int i = 0; i < 4; i++)
            ApplyForce(anchors[i], hits[i],  i);

        if (arduino.sensorData.open)
        {
            //Debug.Log("Arduino");
            verticle = arduino.sensorData.vertical;
            horizontal = arduino.sensorData.horizontal;
            //Debug.Log(arduino.sensorData.vertical);

            //verticle = 1;
            //horizontal = 0;
        }

        else
        {
            //Debug.Log("Joystick");
            verticle = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
        }

        Vector3 forMove = Vector3.zero;
        Vector3 sideMove = Vector3.zero;

        switch (movementMode)
        {
            case MovementMode.torque:
                forMove = (verticle * MoveForce * transform.forward);
                sideMove = (horizontal * TurnTorque * transform.up);


                break;
            case MovementMode.directional:
                forMove = (verticle * MoveForce * transform.forward);
                sideMove = (horizontal * MoveForce * transform.right);
                break;

            

            case MovementMode.joyStick:
                Debug.Log("Joystick");
                verticle = Input.GetAxis("Vertical");
                horizontal = Input.GetAxis("Horizontal");
                forMove = (verticle * MoveForce * transform.forward);
                sideMove = (horizontal * TurnTorque * transform.up);
                break;

            case MovementMode.oneDir:
                forMove = Vector3.zero;
                sideMove = (horizontal * MoveForce * transform.right);
                break;
            case MovementMode.joystickDir:
                horizontal = -1 * Input.GetAxis("Vertical");
                verticle = Input.GetAxis("Horizontal");
                forMove = Vector3.zero;
                sideMove = (horizontal * MoveForce * transform.right);
                break;

        }
        movementDir = (forMove + sideMove).normalized;

        if (boardEnabled)
        {
            

            //Debug.Log(Input.GetAxis("Vertical"));

            //hb.AddForce(Input.GetAxis("Vertical") * moveForce * transform.forward);
            //hb.AddTorque(Input.GetAxis("Horizontal") * turnTorque * transform.up);

            
            
            switch(movementMode)
            {
                case MovementMode.torque:
                    hb.AddForce(verticle * MoveForce * transform.forward);
                    hb.AddTorque(horizontal * TurnTorque * transform.up);


                    break;
                case MovementMode.directional:
                    hb.AddForce(verticle * MoveForce * transform.forward);
                    hb.AddForce(horizontal * MoveForce * transform.right);
                    break;

                case MovementMode.oneDir:
                    //hb.AddForce(verticle * MoveForce * transform.forward);
                    hb.AddForce(horizontal * MoveForce * transform.right);
                    break;

                case MovementMode.joystickDir:
                    //hb.AddForce(verticle * MoveForce * transform.forward);
                    hb.AddForce(horizontal * MoveForce * transform.right);
                    break;

                case MovementMode.combined:
                    hb.AddForce(verticle * MoveForce * transform.forward);
                    if(combinedTurning)
                        hb.AddTorque(horizontal * TurnTorque * transform.up);
                    else
                        hb.AddForce(horizontal * MoveForce * transform.right);
                    break;


                case MovementMode.directionalNoAccel:
                    if(verticle>0)
                    {
                        verticle = 1;
                    }
                    if (verticle < 0)
                    {
                        verticle = -1;  
                    }

                    if (horizontal > 0)
                    {
                        horizontal = 1;
                    }
                    if (horizontal < 0)
                    {
                        horizontal = -1;
                    }


                    transform.localPosition += transform.forward * staticSpeed * verticle + transform.right* staticSpeed *horizontal;//new Vector3(horizontal, 0, verticle) * StaticSpeed;
                    break;

                case MovementMode.torqueNoAccel:

                    if (verticle > 0)
                    {
                        verticle = 1;
                    }
                    if (verticle < 0)
                    {
                        verticle = -1;
                    }

                    if (horizontal > 0)
                    {
                        horizontal = 1;
                    }
                    if (horizontal < 0)
                    {
                        horizontal = -1;
                    }
                    transform.localPosition  +=  verticle *transform.forward * StaticSpeed;
                    transform.Rotate(0, 1.25f * horizontal, 0);
                    //hb.AddTorque(horizontal * TurnTorque * transform.up);
                    break;

                case MovementMode.walkinplace:
                    break;

                case MovementMode.joyStick:
                    verticle = Input.GetAxis("Vertical");
                    horizontal = Input.GetAxis("Horizontal");
                    hb.AddForce(verticle * MoveForce * transform.forward);
                    hb.AddTorque(horizontal * TurnTorque * transform.up);
                    break;

            }

            //arduino.createTexture(horizontal, verticle);


        }
        

    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            //Jump();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Flag")
        {
            if(!other.GetComponent<Ring>().collected)
            {
                other.GetComponent<Ring>().SetCleared();

                trialManager.orderCleared.Add(other.transform.parent.gameObject);
                trialManager.dataManager.flagsCollected++;
            }
            

        }
    }

    float ApplyForce(Transform anchor, RaycastHit hit, int i)
    {
        if (Physics.Raycast(anchor.position, -anchor.up, out hit, Length))
        {
            float force = 0;
            //force = Mathf.Abs(1 / (hit.point.y - anchor.position.y));

            force = HooksLawDampen(hit.distance, i);

            //hb.AddForceAtPosition(transform.up * force * mult, anchor.position, ForceMode.Acceleration);
            hb.AddForceAtPosition(transform.up * force, anchor.position);
            return force;
        }
        else
        {
            lastHitDist[i] = Length * 1.1f;
        }
        return 0;
        
    }

    float HooksLawDampen(float distance, int i)
    {
        float force = Strength * (Length - distance) + (Dampening * (lastHitDist[i] - distance));
        force = Mathf.Max(0f, force);
        lastHitDist[i] = distance;
        
        return force;
    }

    float ApplyJump(Transform anchor, RaycastHit hit)
    {
        if (Physics.Raycast(anchor.position, -anchor.up, out hit))
        {
            float force = 0;
            force = Mathf.Abs(1 / (hit.point.y - anchor.position.y));
            hb.AddForceAtPosition(transform.up * force * mult *20, anchor.position, ForceMode.Impulse);
            return force;
        }
        return 0;

    }

    void Jump()
    {
        Debug.Log("Jump");
        for (int i = 0; i < 4; i++)
            ApplyJump(anchors[i], hits[i]);
    }

}