using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public Texture2D texture2D;
    public float top, bottom, left, right;

    public HoverBoard hoverBoard;

    Color topC = new Color(1, 0, 0, 1);
    Color bottomC = new Color(0, 1, 0, 1);
    Color leftC = new Color(0, 0, 1, 1);
    Color rightC = new Color(0.5f, 0, 1f, 1);
    // Start is called before the first frame update
    void Start()
    {
        texture2D = CreateTexture();
        this.GetComponent<Renderer>().material.mainTexture = texture2D;
        //hoverBoard = this.GetComponentInParent<HoverBoard>();
        top = 0;
        bottom = 0;
        left = 0;
        right = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        texture2D = CreateTexture();
    }

    public Texture2D CreateTexture()
    {
        int width = 100;
        int height = 100;

        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float horizontal = hoverBoard.horizontal;
                float verticle = hoverBoard.verticle;

                Color color = new Color(0,0,0,1);

                Color t = (float)i / (float)width * topC ;

                float b1 = 1 - ((float)i / (float)width );
                Color b = b1 * bottomC;

                Color l = (float)j / (float)height * leftC;

                float r1 = 1 - (float)j / (float)height;
                Color r = r1 * rightC;


                color = t * b *(height-j) + l * r *(width - i);


                //if (i < width / 2)
                //{
                //    if (j < height / 2)
                //    {

                //        color = topC;
                //    }
                //    else if (j > height / 2)
                //    {
                //        color = leftC;
                //    }
                //    else
                //    {
                //        color = Color.black;
                //    }

                //}
                //else if (i > width / 2)
                //{
                //    if (j < height / 2)
                //    {

                //        color = rightC;
                //    }
                //    else if (j > height / 2)
                //    {
                //        color = bottomC;
                //    }
                //    else
                //    {
                //        color = Color.black;
                //    }
                //}
                
                texture.SetPixel(j, height - 1 - i, color);
            }
        }
        texture.Apply();
        return texture;
    }

}
