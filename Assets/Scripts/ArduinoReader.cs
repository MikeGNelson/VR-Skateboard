using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using TMPro;
using System.Threading.Tasks;
using System;
//using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Linq;
using System.Drawing;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;
using UnityEngine.UI;
//using ExtensionMethods.SerialPort;

//namespace ExtensionMethods.SerialPort
//{
//    public static class SerialPortExtensions
//    {
//        public async static Task ReadAsync(this System.IO.Ports.SerialPort serialPort, byte[] buffer, int offset, int count)
//        {
//            var bytesToRead = count;
//            var temp = new byte[count];

//            while (bytesToRead > 0)
//            {
//                var readBytes = await serialPort.BaseStream.ReadAsync(temp, 0, bytesToRead);
//                Array.Copy(temp, 0, buffer, offset + count - bytesToRead, readBytes);
//                bytesToRead -= readBytes;
//            }
//        }

//        public async static Task<byte[]> ReadAsync(this System.IO.Ports.SerialPort serialPort, int count)
//        {
//            var buffer = new byte[count];
//            await serialPort.ReadAsync(buffer, 0, count);
//            return buffer;
//        }
//    }
//}

public class ArduinoReader : MonoBehaviour
{
    public string portString = "COM8";
    public int baudRate = 115200;
    public SerialPort sp;
    public string recievedString;
    public string[] data;

    public BoardType boardType = BoardType.joystick;
    public enum BoardType
    {
        isometric,
        elastic,
        chair,
        headDirected,
        joystick
    }

    public TMP_Text text;

    byte input;

    public float bounding = 0.1f;
    public float Horizontalbounding = 0.1f;

    public float upperBound = -100;

    public SensorData sensorData = new SensorData();
    private string receivedString;


    public TMP_Text nw;
    public TMP_Text ne;
    public TMP_Text sw;
    public TMP_Text se;

    public GameObject image;
    public GameObject imageFloating;
    public RawImage uiImage;
    public RawImage uiProctorImage;

    public float horTest, vertTest = 0;
    public bool testMode = false;

    public TMP_Text Horizontal;
    public TMP_Text Vertical;

    public Texture2D noiseTexture;

    const float scaleBottomRange = 0.1f / 0.15f;
    const float scaleTopRange = 1.0f / 0.45f;

    public float horizontalMin = 0.2f;
    public float horizontalMax = 0.55f;

    public float verticalMin = 0.2f;
    public float verticalMax = 0.55f;



    public struct SensorData
    {
        public float mode;

        public float stg0;
        public float stg1;
        public float stg2;
        public float stg3;

        public float tof0;
        public float tof1;
        public float tof2;
        public float tof3;
        public float tof4;
        public float tof5;
        public float tof6;
        public float tof7;

        public float horizontal;
        public float vertical;

        public bool open;
    };

    Thread myThread;

    // Start is called before the first frame update
    void Start()
    {


        InitSerialPort();


        sensorData = new SensorData();
        text = GetComponent<TMP_Text>();


        //sp.DataReceived += Sp_DataReceived;
        //sp.ReadTimeout = 100;

        //InvokeRepeating("GetArduino", 0f, 0.01f);
        //myThread = new Thread(new ThreadStart(GetArduino));
        //myThread.Start();
    }

    public void InitSerialPort()
    {

        string[] ports = SerialPort.GetPortNames();
        for (int i = 0; i < ports.Length; i++)
        {
            Debug.Log(ports[i]);
        }
        //sp = new SerialPort(portString, baudRate);

        if(ports.Length > 0)
        {
            sp = new SerialPort(ports[0], 115200) { RtsEnable = true, DtrEnable = true };

            //sp.RtsEnable = true;
            //sp.DtrEnable = true;

            //sp.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            if (sp.IsOpen)
            {
                //Debug.Log("Close");
                sp.Close();
            }
            sp.Open();
        }
            
    }

    //private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    //{
    //    try
    //    {
    //        receivedString = sp.ReadLine();
    //        lock (lockObject)
    //        {
    //            SetData(receivedString);
    //        }
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError(ex.Message);
    //    }
    //}

    private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        
        
            recievedString = sp.ReadLine();
        //Debug.Log(recievedString);
            //Debug.Log(recievedString);
            // When left button is pushed
            //if (sp.ReadByte() == 1)
            //{
            //    print(sp.ReadByte());
            //    transform.Translate(Vector3.left * Time.deltaTime * 5);
            //}
            //// When right button is pushed
            //if (sp.ReadByte() == 2)
            //{
            //    print(sp.ReadByte());
            //    transform.Translate(Vector3.right * Time.deltaTime * 5);
            //}
            //text.text = "Values : " + recievedString;
            //SetData(recievedString);

        
    }

    private void GetArduino()
    {
        Debug.Log("Get");
        while (true)
        {
            Debug.Log("Stuff");
            recievedString = sp.ReadLine();
            //input = (byte)sp.ReadByte();
            Debug.Log(input);
            //SetData(recievedString);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Work();

        if(sp != null)
        {
            if (sp.IsOpen)
            {
                try
                {
                    if (sp.BytesToRead > 0)
                    {
                        recievedString = sp.ReadLine();

                        SetData(recievedString);
                    }
                        

                }
                catch (System.Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            else
            {
                sensorData.open = false;
                if (testMode)
                {
                    CreateTexture(horTest, vertTest);
                }

            }
        }
        else
        {
            sensorData.open = false;
            if (testMode)
            {
                CreateTexture(horTest, vertTest);
            }

        }



        //sp.DiscardInBuffer();
    }

    void SetData(string text)
    {
        string[] strValues = text.Split(',');

        if (strValues.Length < 5)
            return;

        float.TryParse(strValues[0], out sensorData.mode);
        float.TryParse(strValues[1], out sensorData.stg0);
        float.TryParse(strValues[2], out sensorData.stg1);
        float.TryParse(strValues[3], out sensorData.stg2);
        float.TryParse(strValues[4], out sensorData.stg3);

        switch (sensorData.mode)
        {
            case 0:
                boardType = BoardType.isometric; break;
            case 1:
                boardType= BoardType.elastic; break;
            case 2:
                boardType= BoardType.chair; break;
            case 3:
                boardType = BoardType.headDirected; break;
            case 4:
                boardType= BoardType.joystick; break;
            default:
                boardType = BoardType.joystick; break;
        }

        if (sensorData.mode == 1 && strValues.Length >= 12)
        {
            float.TryParse(strValues[5], out sensorData.tof0);
            float.TryParse(strValues[6], out sensorData.tof1);
            float.TryParse(strValues[7], out sensorData.tof2);
            float.TryParse(strValues[8], out sensorData.tof3);
            float.TryParse(strValues[9], out sensorData.tof4);
            float.TryParse(strValues[10], out sensorData.tof5);
            float.TryParse(strValues[11], out sensorData.tof6);
        }

        sensorData.open = true;

        UpdateUI();
        CalculateAndSetOrientation();
    }

    void UpdateUI()
    {
        ne.text = sensorData.stg0.ToString("F2");
        se.text = Mathf.Round(sensorData.stg1).ToString("F2");
        sw.text = Mathf.Round(sensorData.stg2).ToString("F2");
        nw.text = Mathf.Round(sensorData.stg3).ToString("F2");
    }

    void CalculateAndSetOrientation()
    {
        var result = MapPressure(Normalize(sensorData.stg0), Normalize(sensorData.stg1), Normalize(sensorData.stg2), Normalize(sensorData.stg3), horizontalMin, horizontalMax, verticalMin, verticalMax);

        if (Mathf.Abs(result.horizontal) <= Horizontalbounding)
        {
            result.horizontal = 0;
        }

        if (Mathf.Abs(result.vertical) <= bounding)
        {
            result.vertical = 0;
        }

        sensorData.horizontal = result.horizontal;
        sensorData.vertical = result.vertical;

        Horizontal.text = result.horizontal.ToString("F2");
        Vertical.text = result.vertical.ToString("F2");

        CreateTexture(result.horizontal, result.vertical);
    }

 



    //void Serial_Read()
    //{
    //    if (sp.IsOpen)
    //    {
    //        //try
    //        {
    //            recievedString = sp.ReadLine();
    //            //sp.ReadTimeout = 10;

    //            UnityEngine.Debug.Log(recievedString);
    //            // When left button is pushed
    //            //if (sp.ReadByte() == 1)
    //            //{
    //            //    print(sp.ReadByte());
    //            //    transform.Translate(Vector3.left * Time.deltaTime * 5);
    //            //}
    //            //// When right button is pushed
    //            //if (sp.ReadByte() == 2)
    //            //{
    //            //    print(sp.ReadByte());
    //            //    transform.Translate(Vector3.right * Time.deltaTime * 5);
    //            //}
    //            //text.text = "Values : " + recievedString;
    //            //SetData(recievedString);

    //        }
    //        //catch (System.Exception e)
    //        //{
    //        //    UnityEngine.Debug.Log(e.Message);
    //        //}

    //    }
    //}

    //void SetData(string text)
    //{
    //    char[] separators = { ',', ';', '|' };
    //    string[] strValues = text.Split(',');

    //    //List<float> values = new List<float>();

    //    //UnityEngine.Debug.Log(strValues[0]);

    //    sensorData.mode = Convert.ToInt32(strValues[0]);    // Mode

    //    //sensorData.stg0 = float.Parse(strValues[1]);    // NW    BLue _Frotn lef
    //    //sensorData.stg1 = float.Parse(strValues[2]);    // NE    Yellow front right
    //    //sensorData.stg2 = float.Parse(strValues[3]);   // SW     Red       back left
    //    //sensorData.stg3 = float.Parse(strValues[4]);   // SE     Green      back right

    //    sensorData.stg0 = float.Parse(strValues[1]);    // SW    BLue _Frotn lef
    //    sensorData.stg1 = float.Parse(strValues[2]);    // SE    Red front right
    //    sensorData.stg2 = float.Parse(strValues[3]);   // NE     Yellow       back left
    //    sensorData.stg3 = float.Parse(strValues[4]);   // NW     Green      back right

    //    if (sensorData.mode ==1)
    //    {
    //        sensorData.tof0 = float.Parse(strValues[5]);    // SW
    //        sensorData.tof1 = float.Parse(strValues[6]);    // SE
    //        sensorData.tof2 = float.Parse(strValues[7]);    // NE
    //        sensorData.tof3 = float.Parse(strValues[8]);    // NW
    //        sensorData.tof4 = float.Parse(strValues[9]);    // X
    //        sensorData.tof5 = float.Parse(strValues[10]);    // Y
    //        sensorData.tof6 = float.Parse(strValues[11]);    // Z

    //    }

    //    sensorData.open = true;


    //    ne.text = sensorData.stg0.ToString("F2");
    //    se.text = Mathf.Round(sensorData.stg1).ToString("F2");
    //    sw.text = Mathf.Round(sensorData.stg2).ToString("F2");
    //    nw.text = Mathf.Round(sensorData.stg3).ToString("F2");



    //    //float hor = -1 * Mathf.Abs(sensorData.stg0) + Mathf.Abs(sensorData.stg2);
    //    //float vert = -1 * Mathf.Abs(sensorData.stg1) + Mathf.Abs(sensorData.stg3);

    //    //if (sensorData.stg0 >= upperBound) { sensorData.stg0 = 0; }
    //    //if(sensorData.stg1 >= upperBound) { sensorData.stg1 = 0; }
    //    //if(sensorData.stg2 >= upperBound) { sensorData.stg2 = 0; }
    //    //if(sensorData.stg3 >= upperBound) { sensorData.stg3 = 0; }


    //    float[] data = new[] { sensorData.stg0, sensorData.stg1, sensorData.stg2, sensorData.stg3 };
    //    //float max = Math.Min(-3000, data.Min());

    //    //sensorData.stg0 = sensorData.stg0 / max;
    //    //sensorData.stg1 = sensorData.stg1 / max;
    //    //sensorData.stg2 = sensorData.stg2 / max;
    //    //sensorData.stg3 = sensorData.stg3 / max;


    //    //float horPlus = Mathf.Abs(sensorData.stg0) + Mathf.Abs(sensorData.stg1);
    //    //float horMin = Mathf.Abs(sensorData.stg2) + Mathf.Abs(sensorData.stg3);

    //    //float vertPlus = Mathf.Abs(sensorData.stg1) + Mathf.Abs(sensorData.stg2);
    //    //float vertMin = Mathf.Abs(sensorData.stg0) + Mathf.Abs(sensorData.stg3);

    //    //float hor = horPlus - horMin;
    //    //float vert = vertPlus - vertMin;

    //    // Normalize and convert the sensor values
    //    //float stg0 = ConvertToRange(Normalize(sensorData.stg0));
    //    //float stg1 = ConvertToRange(Normalize(sensorData.stg1));
    //    //float stg2 = ConvertToRange(Normalize(sensorData.stg2));
    //    //float stg3 = ConvertToRange(Normalize(sensorData.stg3));

    //    float stg0 = Normalize(sensorData.stg0);// ConvertToRange(Normalize(sensorData.stg0));
    //    float stg1 = Normalize(sensorData.stg1);//ConvertToRange(Normalize(sensorData.stg1));
    //    float stg2 = Normalize(sensorData.stg2);//ConvertToRange(Normalize(sensorData.stg2));
    //    float stg3 = Normalize(sensorData.stg3);//ConvertToRange(Normalize(sensorData.stg3));

    //    //// Calculate horizontal and vertical values
    //    //float horPlus = (Mathf.Abs(stg0) + Mathf.Abs(stg1))/2f;
    //    //float horMin = (Mathf.Abs(stg2) + Mathf.Abs(stg3))/2f;

    //    //float vertPlus = (Mathf.Abs(stg0) + Mathf.Abs(stg3))/2f;
    //    //float vertMin = (Mathf.Abs(stg1) + Mathf.Abs(stg2))/2f;

    //    //float hor = horPlus - horMin;
    //    //float vert = vertPlus - vertMin;

    //    //float threshold = 0f; // 20% threshold
    //    //float outputHor = 0f;
    //    //float outputVert = 0f;


    //    //if (Mathf.Abs(hor) > threshold)
    //    //{
    //    //    if (hor > 0)
    //    //    {
    //    //        outputHor = Mathf.Lerp(0.1f, 1f, (hor - threshold) / (100f - threshold));
    //    //    }
    //    //    else
    //    //    {
    //    //        outputHor = Mathf.Lerp(-0.1f, -1f, (-hor - threshold) / (100f - threshold));
    //    //    }
    //    //}

    //    //if (Mathf.Abs(vert) > threshold)
    //    //{
    //    //    if (vert > 0)
    //    //    {
    //    //        outputVert = Mathf.Lerp(0.1f, 1f, (vert - threshold) / (100f - threshold));
    //    //    }
    //    //    else
    //    //    {
    //    //        outputVert = Mathf.Lerp(-0.1f, -1f, (-vert - threshold) / (100f - threshold));
    //    //    }
    //    //}

    //    // Clamp the hor and vert values to be within -1 to 1 range
    //    //hor = Mathf.Clamp(hor, -1f, 1f);
    //    //vert = Mathf.Clamp(vert, -1f, 1f);

    //    //hor = -hor;
    //    //vert = -vert;

    //    //if (Mathf.Abs(hor) <= Horizontalbounding)
    //    //{
    //    //    hor = 0;
    //    //}

    //    //if (Mathf.Abs(vert) <= bounding)
    //    //{
    //    //    vert = 0;
    //    //}

    //    var result = MapPressure(stg0, stg1, stg2, stg3);

    //    if (Mathf.Abs(result.horizontal) <= Horizontalbounding)
    //    {
    //        result.horizontal = 0;
    //    }

    //    if (Mathf.Abs(result.vertical) <= bounding)
    //    {
    //        result.vertical = 0;
    //    }

    //    sensorData.horizontal = result.horizontal;
    //    sensorData.vertical = result.vertical;

    //    Horizontal.text = result.horizontal.ToString("F2");
    //    Vertical.text = result.vertical.ToString("F2");

    //    createTexture(result.horizontal, result.vertical);


    //    //Debug.Log(hor);

    //}

    public static (float horizontal, float vertical) MapPressure(float p1, float p2, float p3, float p4, float horMin, float horMax, float vertMin, float vertMax)
    {
        // Calculate average pressures for edges
        float topEdge = (p1 + p2) / 2.0f;
        float bottomEdge = (p3 + p4) / 2.0f;
        float leftEdge = (p1 + p4) / 2.0f;
        float rightEdge = (p2 + p3) / 2.0f;


        //float horPlus = (Mathf.Abs(stg0) + Mathf.Abs(stg1))/2f;
        //float horMin = (Mathf.Abs(stg2) + Mathf.Abs(stg3))/2f;

        //float vertPlus = (Mathf.Abs(stg0) + Mathf.Abs(stg3))/2f;
        //float vertMin = (Mathf.Abs(stg1) + Mathf.Abs(stg2))/2f;

        //float hor = horPlus - horMin;
        //float vert = vertPlus - vertMin;


        // Calculate differences
        float horizontalDifference = topEdge - bottomEdge; 
        float verticalDifference = leftEdge - rightEdge;


 
        // Scale differences
        float horizontal = ScaleDifference(horizontalDifference, min: horMin, max: horMax);
        float vertical = ScaleDifference(verticalDifference, min: vertMin, max: vertMax);

        return (horizontal, vertical);
    }

    private static float ScaleDifference(float difference, float min, float max)
    {
        // Scale difference based on provided ranges
        if (Math.Abs(difference) <= min)
        {
            return difference * 0;
        }
        else if (Math.Abs(difference) <= max && Math.Abs(difference) > min)
        {
            return difference * scaleTopRange;
        }
        else // +- 1
        {
            // Handle differences beyond 30%
            return Math.Sign(difference);
        }
    }

    //public void createTexture(float hor, float vert)
    //{

    //    //Debug.Log("Set Mat");

    //    //sensorData.stg0    // NW    BLue     Frotn left
    //    //sensorData.stg1     // NE    Yellow front right
    //    //sensorData.stg2    // SW     Red       back left
    //    //sensorData.stg3    // SE     Green      back right
    //    int dim = 20;
    //    noiseTexture = new Texture2D(dim, dim);
    //    //Debug.Log("create");

    //    Vector3 color = new Vector3(0,0,0);

    //    float r = 0f;
    //    float g = 0f;
    //    float b = 0f;



    //    for (int x = 0; x < dim; x++)
    //    {
    //        for (int y = 0; y < dim; y++)
    //        {
    //            noiseTexture.SetPixel(x, y, new UnityEngine.Color(0, 0, 0));
    //            r = 0f;
    //            g = 0f;
    //            b = 0f;
    //            //Right
    //            if (x >= dim/2)
    //            {
    //                if(hor> Horizontalbounding)
    //                {
    //                    g +=  Mathf.Abs(hor);
    //                }


    //            }
    //            // Left
    //            else
    //            {
    //                if(hor< Horizontalbounding)
    //                {
    //                    g += Mathf.Abs(hor);
    //                }

    //            }

    //            // Top
    //            if (y >= dim / 2)
    //            {
    //                if(vert> bounding)
    //                {
    //                    g  += vert;
    //                }

    //            }
    //            // Bottom
    //            else
    //            {
    //                if(vert< bounding)
    //                {
    //                    g += Mathf.Abs(vert);
    //                }
    //            }

    //            if(g ==0 && b == 0)
    //            {
    //                r = 0;
    //                g = 0;
    //                b = 0;
    //            }
    //            else
    //            {
    //                r = 0;
    //            }

    //            if(g > 1)
    //            {
    //                g = 1;

    //            }


    //            noiseTexture.SetPixel(x, y, new UnityEngine.Color(r, g, b));

    //        }
    //    }

    //    noiseTexture.Apply();


    //    image.GetComponent<Renderer>().material.SetTexture("_BaseMap", noiseTexture);
    //    uiImage.texture = noiseTexture;
    //    uiProctorImage.texture = noiseTexture;
    //}

    public void CreateTexture(float hor, float vert)
    {
        int dim = 20;
        noiseTexture = new Texture2D(dim, dim);

        float halfDim = dim / 2f;
        float horAbs = Mathf.Abs(hor);
        float vertAbs = Mathf.Abs(vert);
        bool horGreaterThanBound = hor > Horizontalbounding;
        bool vertGreaterThanBound = vert > bounding;

        for (int x = 0; x < dim; x++)
        {
            for (int y = 0; y < dim; y++)
            {
                float r = 0f, g = 0f, b = 0f;

                // Right or Left
                if ((x >= halfDim && horGreaterThanBound) || (x < halfDim && hor < Horizontalbounding))
                {
                    g += horAbs;
                }

                // Top or Bottom
                if ((y >= halfDim && vertGreaterThanBound) || (y < halfDim && vert < bounding))
                {
                    g += vertAbs;
                }

                g = Mathf.Clamp(g, 0, 1); // Ensure g is within the range [0, 1]

                noiseTexture.SetPixel(x, y, new UnityEngine.Color(r, g, b));
            }
        }

        noiseTexture.Apply();

        var renderer = image.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.SetTexture("_BaseMap", noiseTexture);
        }

        var rend  = imageFloating.GetComponent<Renderer>(); 
        if (rend != null)
        {
            rend.material.SetTexture("_BaseMap",noiseTexture);
        }
        uiImage.texture = noiseTexture;
        uiProctorImage.texture = noiseTexture;
    }


    private void OnApplicationQuit()
    {
        sp.Close();
    }

    // Function to normalize a value from 0-100 to 0-1
    float Normalize(float value)
    {
        return value / 100f;
    }

    // Function to convert a normalized value from 0-1 to -1 to 1
    float ConvertToRange(float value)
    {
        return (value * 2f) - 1f;
    }
}
