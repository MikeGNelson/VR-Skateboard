using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowFollow : MonoBehaviour
{
    public Transform Board;
    public Vector3 moveDir;
    public Transform child;

    public HoverBoard hb;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = Board.transform.rotation;

        moveDir = hb.movementDir;
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            child.rotation = targetRotation;
        }
    }
}
