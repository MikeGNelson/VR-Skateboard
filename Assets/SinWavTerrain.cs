using System.Collections.Generic;
using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class SinWaveTerrain : MonoBehaviour
{
    public enum WaveDirection { Up, Down, Both }  // Enum for wave direction
    public WaveDirection waveDirection = WaveDirection.Both;  // Selected wave direction

    public int resolutionX = 1000;    // Resolution along the x-axis
    public int resolutionZ = 500;     // Resolution along the z-axis
    public float frequency = 1f;      // Controls the frequency of the sine wave
    public float amplitude = 1f;      // Controls the amplitude of the sine wave
    public float flatPeriod = 2f;     // Distance before the wave pattern starts
    public bool waveAlongX = true;    // Set to true for sine wave along x-axis, false for z-axis
    public int numCompleteWaves = 3;  // Number of complete sine waves before stopping

    private Mesh mesh;
    private Vector3[] vertices;
    private MeshCollider meshCollider;
    private Material material;

    private int previousResolutionX;
    private int previousResolutionZ;

    void Start()
    {
        meshCollider = GetComponent<MeshCollider>();
        material = GetComponent<MeshRenderer>().material;
        GenerateMesh();
        UpdateTerrain();
        UpdateMaterialTiling();
        //FindMatchingCombinations(5, 0.5f);
    }

    void OnValidate()
    {
        Generate();
    }

    public void Generate()
    {
        if (resolutionX != previousResolutionX || resolutionZ != previousResolutionZ)
        {
            GenerateMesh();
            UpdateMaterialTiling();
            previousResolutionX = resolutionX;
            previousResolutionZ = resolutionZ;
        }

        UpdateTerrain();
        //FindMatchingCombinations(125);
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "Generated Sine Wave Mesh";
        GetComponent<MeshFilter>().mesh = mesh;

        float offsetX = waveAlongX ? -flatPeriod / 2f : -resolutionX / 2f;
        float offsetZ = waveAlongX ? -resolutionZ / 2f : -flatPeriod / 2f;

        vertices = new Vector3[resolutionX * resolutionZ];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int z = 0; z < resolutionZ; z++)
        {
            for (int x = 0; x < resolutionX; x++)
            {
                int index = z * resolutionX + x;

                vertices[index] = new Vector3(x + offsetX, 0, z + offsetZ);
                uvs[index] = new Vector2((float)x / resolutionX, (float)z / resolutionZ);
            }
        }

        int[] triangles = new int[(resolutionX - 1) * (resolutionZ - 1) * 6];
        int triangleIndex = 0;
        for (int z = 0; z < resolutionZ - 1; z++)
        {
            for (int x = 0; x < resolutionX - 1; x++)
            {
                int vertexIndex = z * resolutionX + x;
                triangles[triangleIndex++] = vertexIndex;
                triangles[triangleIndex++] = vertexIndex + resolutionX;
                triangles[triangleIndex++] = vertexIndex + 1;
                triangles[triangleIndex++] = vertexIndex + 1;
                triangles[triangleIndex++] = vertexIndex + resolutionX;
                triangles[triangleIndex++] = vertexIndex + resolutionX + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    void UpdateTerrain()
    {
        float maxWaveLength = numCompleteWaves * (2 * Mathf.PI / frequency);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            float wavePosition = waveAlongX ? Mathf.Abs(vertex.x) : Mathf.Abs(vertex.z);

            bool inInitialFlat = (waveAlongX && vertex.x < flatPeriod / 2f && vertex.x > -flatPeriod / 2f) ||
                                 (!waveAlongX && vertex.z < flatPeriod / 2f && vertex.z > -flatPeriod / 2f);

            bool inWaveRegion = wavePosition >= flatPeriod / 2f && wavePosition < flatPeriod / 2f + maxWaveLength;
            bool inEndFlat = wavePosition >= flatPeriod / 2f + maxWaveLength;

            if (inInitialFlat || inEndFlat)
            {
                vertex.y = 0;
            }
            else if (inWaveRegion)
            {
                float waveOffset = wavePosition - flatPeriod / 2f;
                float sineValue = Mathf.Sin(waveOffset * frequency) * amplitude;

                // Adjust sine value based on the selected wave direction
                switch (waveDirection)
                {
                    case WaveDirection.Up:
                        vertex.y = Mathf.Max(0, sineValue);  // Only positive values
                        break;
                    case WaveDirection.Down:
                        vertex.y = Mathf.Min(0, sineValue);  // Only negative values
                        break;
                    case WaveDirection.Both:
                        vertex.y = sineValue;                // Both positive and negative values
                        break;
                }
            }

            vertices[i] = vertex;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    void UpdateMaterialTiling()
    {
        if (material != null)
        {
            material.mainTextureScale = new Vector2(resolutionX, resolutionZ);
        }
    }

    public float GetMeshLength()
    {
        // Calculate the length of the mesh in the direction of the wave (X or Z)
        float maxWaveLength = numCompleteWaves * (2 * Mathf.PI / frequency);

        if (waveAlongX)
        {
            return resolutionX; // The mesh length in the X direction
        }
        else
        {
            return resolutionZ; // The mesh length in the Z direction
        }
    }

    public float CalculateFlatRegionStartDistance()
    {
        float totalLength = 0f;
        Vector3 previousVertex = vertices[0];

        // Calculate the distance where the flat end region begins
        float maxWaveLength = numCompleteWaves * (2 * Mathf.PI / frequency);
        float flatEndPosition = flatPeriod / 2f + maxWaveLength;

        for (int i = 1; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            float wavePosition = waveAlongX ? Mathf.Abs(vertex.x) : Mathf.Abs(vertex.z);

            // Check if the vertex is in the region where the flat section begins
            bool inEndFlat = waveAlongX ? vertex.x >= flatEndPosition : vertex.z >= flatEndPosition;

            if (inEndFlat)
            {
                // Calculate the distance between the start of the mesh and the start of the flat end region
                totalLength = Vector3.Distance(vertices[0], vertex);
                break;  // Stop once we reach the start of the flat end region
            }
        }

        return totalLength;
    }

    public float CalculateMeshLengthForCombination(float frequency, float numCompleteWaves)
    {
        // Calculate the wavelength based on frequency
        float wavelength = 2f * Mathf.PI / frequency;

        // Calculate the total length based on the number of complete waves
        return wavelength * numCompleteWaves;
    }

    public void FindMatchingCombinations(float numCompleteWaves, float tolerance)
    {
        // Set of frequencies and amplitudes you want to check
        float[] amplitudes = { .5f, 1f, 1.5f, 2f,2.5f, 3f }; // Low and High Amplitude
        float[] frequencies = { 0.1f, 0.25f, 0.2f, 0.15f }; // Include some values in between

        // Create a dictionary to store combinations with their lengths
        Dictionary<float, List<string>> lengthCombinations = new Dictionary<float, List<string>>();

        // Iterate over all combinations of amplitude and frequency
        foreach (var amplitude in amplitudes)
        {
            foreach (var frequency in frequencies)
            {
                float length = CalculateMeshLengthForCombination(frequency, numCompleteWaves);

                bool foundCloseMatch = false;
                // Look for a close match based on the tolerance
                foreach (var entry in lengthCombinations)
                {
                    if (Math.Abs(entry.Key - length) <= tolerance)
                    {
                        // If the length is within tolerance, add the combination to the list
                        lengthCombinations[entry.Key].Add($"Amplitude: {amplitude}, Frequency: {frequency}, Length: {length}");
                        foundCloseMatch = true;
                        break;
                    }
                }

                // If no close match was found, create a new entry
                if (!foundCloseMatch)
                {
                    lengthCombinations[length] = new List<string>
                    {
                        $"Amplitude: {amplitude}, Frequency: {frequency}, Length: {length}"
                    };
                }
            }
        }

        // Print out combinations with matching lengths within the tolerance
        foreach (var entry in lengthCombinations)
        {
            if (entry.Value.Count > 1) // Only print lengths with more than one combination
            {
                Debug.Log($"Matching Length: {entry.Key}");
                foreach (var combination in entry.Value)
                {
                    Debug.Log(combination);
                }
            }
        }
    }

    // Calculate the mesh length for a given frequency and number of complete waves
    public float CalculateMeshLength(float frequency, float numCompleteWaves)
    {
        float wavelength = 2f * Mathf.PI / frequency;
        return wavelength * numCompleteWaves;
    }

    // Find the number of complete waves required to achieve a target length
    public float CalculateNumWavesForTargetLength(float frequency, float targetLength)
    {
        float wavelength = 2f * Mathf.PI / frequency;
        return targetLength / wavelength;
    }

    // Main function to find matching combinations of frequencies and numCompleteWaves
    public void FindMatchingCombinations(float targetLength)
    {
        // Frequencies to check
        float[] frequencies = { 0.1f, 0.25f, 0.2f, 0.15f }; // Example frequency range
        float[] amplitudes = { 1f,2f, 3f }; // Amplitudes

        // Store the matching combinations
        List<string> matchingCombinations = new List<string>();

        foreach (var amplitude in amplitudes)
        {
            foreach (var frequency in frequencies)
            {
                // Calculate the number of waves needed to achieve the target length
                float numWaves = CalculateNumWavesForTargetLength(frequency, targetLength);

                // Calculate the actual length for this combination
                float actualLength = CalculateMeshLength(frequency, numWaves);

                // If the actual length is close enough to the target length, store the combination
                if (Math.Abs(actualLength - targetLength) < 0.1f) // Tolerance for matching lengths
                {
                    matchingCombinations.Add($"Amplitude: {amplitude}, Frequency: {frequency}, NumWaves: {numWaves}, Length: {actualLength}");
                }
            }
        }

        // Print the combinations with matching lengths
        foreach (var combination in matchingCombinations)
        {
            Debug.Log(combination);
        }
    }

}