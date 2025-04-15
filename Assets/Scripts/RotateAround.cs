using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    // Start is called before the first frame update

    public float speed =10f;

    public GameObject Obj;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Obj != null)
        {
            transform.position = Obj.transform.position;
            transform.Rotate(0, speed * Time.deltaTime,0);
        }
    }
}
