using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[ExecuteInEditMode]

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;



    Vector3[] vertices;

    int[] triangles;

    Vector2[] uvs; 



    public int xSize = 20;

    public int zSize = 20;



    public float strength = 0.3f;
    public float stepSize = 0;
    public float caveFreq = 0.05f;
    public int seed;
    public float heightMod = 1f;
    public float height = 1f;

    public int octaves = 5;
    public float frequency = 1f;
    public float amplitude = 12f;
    public float persistence = 0.5f;
    public float lacunarity =2f;
    public float scale = 50f;
    public float exponential = 3.5f;


    public bool FBM = false;


    public Texture2D noiseTexture;




    void Start()

    {
        Random.InitState(seed);
        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = mesh;

        GenerateNoiseTexture();


        CreateShape();

        UpdateMesh();

        //this.transform.position = new Vector3(-(float)xSize / 2, 0, -(float)zSize / 2);

    }

    private void OnValidate()
    {
        Rerun();
        
    }

    public void Rerun()
    {
        Random.InitState(seed);
        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = mesh;

        GenerateNoiseTexture();


        CreateShape();

        UpdateMesh();
        
    }



    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)

            {
                //float y = 
                float y = noiseTexture.GetPixel(x, z).r * heightMod;
                //Debug.Log(y);
                //float y = 0;
                vertices[i] = new Vector3(x * stepSize, y, z * stepSize);

                i++;

            }

        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;

        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {

            for (int x = 0; x < xSize; x++)

            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
                vert++;

                tris += 6;
            }

            vert++;
        }

        uvs = new Vector2[vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)

            {
                uvs[i] = new Vector2((float)x/xSize, (float)z /zSize);
                i++;

            }



        }
    }



        void UpdateMesh()

    {

        mesh.Clear();
        this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.uv = uvs;


        mesh.RecalculateNormals();

        this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;


        //this.transform.position = new Vector3(-(float)xSize / 2, 0, -(float)zSize / 2);
        //Color c = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        //Color c2 = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        
        //this.GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", c);
        //this.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", c2);


    }


    public void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(xSize, zSize);

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < zSize; y++)
            {
                float value = 0;
                if(FBM)
                {
                    value = ComputeFBM(x, y);
                }

                else
                {
                    value = Mathf.PerlinNoise((x + transform.position.x + seed) * frequency,
                                        (y + transform.position.y + seed) * frequency);
                }
                //for(int q = 0; q < octaves; q++)
                //{

                //    value += Mathf.PerlinNoise((x *frequency + transform.position.x + seed) , (y * frequency + transform.position.y + seed) ) * amplitude;
                //    amplitude *= 0.5f;
                //    frequency *= 2;
                //}

                //float v = Mathf.PerlinNoise((x + transform.position.x + seed) * caveFreq, (y + transform.position.y + seed) * caveFreq);
                //noiseTexture.SetPixel(x, y, new Color(v, v, v));

                noiseTexture.SetPixel(x, y, new Color(value, value, value));


            }
        }

        noiseTexture.Apply();


    }

    float ComputeFBM(int x, int y)
    {
        float xs = x / scale ;
        float ys = y / scale ;
        float G = Mathf.Pow(2, -persistence);
        float normalization = 0;
        float total = 0;

        float _frequency = frequency;
        float _amplitude = amplitude;

        for (int q = 0; q < octaves; q++)
        {
            //float value = Mathf.PerlinNoise(xs * _frequency, ys * _frequency) *0.5f +0.5f;

            float value = Mathf.PerlinNoise((xs * _frequency  +seed), (ys * _frequency + seed)) * 0.5f + 0.5f;
            total += value * _amplitude;
            normalization += _amplitude;
            _amplitude *= G;
            _frequency *=lacunarity;

        }
        total /= normalization;
        //Debug.Log(total);
        //return total;
        return Mathf.Pow(total, exponential * height);

    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices == null)
    //        return;

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], 0.1f);
    //    }
    //}
}
