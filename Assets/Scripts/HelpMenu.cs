using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class HelpMenu : MonoBehaviour
{

    public int index =0;
    public List<string> prompts = new List<string>();
    public TMP_Text text;
    [SerializeField]
    public RawImage indicator;
    public Texture2D left;
    public Texture2D right;
    public Texture2D forward;
    public Texture2D backward;
    public Texture2D leftforward;
    public 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //text.text = prompts[index];
        //CheckImage();
    }

    public void Next()
    {
        if(index < prompts.Count)
        {
            index++;
            text.text = prompts[index];

        }
        else
        {
            index = 0;
            text.text = prompts[index];
            this.gameObject.SetActive(false);
        }
        CheckImage();
    }

    public void Previous()
    {
        if (index > 0)
        {
            index--;
            text.text = prompts[index];
            CheckImage();
        }
    }

    public void CheckImage()
    {
        if(index ==4)
        {
            indicator.texture = right;
        }
        else if(index ==5)
        {
            indicator.texture = left;
        }
        else if (index ==6)
        {
            indicator.texture = forward;
        }
        else if (index ==7)
        {
            indicator.texture  = backward;
        }
        else if (index ==8) 
        {
            indicator.texture = leftforward;
        }
        else
        {
            indicator.texture = null;
        }
    }
}
