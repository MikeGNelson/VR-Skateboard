using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public Color cleared;
    public Color uncleared;

    public bool collected = false;
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Renderer>().material.color = uncleared;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCleared()
    {
        this.GetComponent<Renderer>().material.color = cleared;
        collected = true;
    }
}
