using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter) , typeof(MeshRenderer))]
public class WaterManager : MonoBehaviour {
    [Header("Ocean Properties")]
    [SerializeField] private Material waterMat;
    [SerializeField]private Color waterColor = new Color(0f, 0.4f, 0.8f, 1f);
    [SerializeField]private float waveAmplitude = 1.0f;
    [SerializeField]private float waveLength = 10.0f;
    [SerializeField]private float waveSpeed = 2.0f;
    [SerializeField]private float waveSteepness = 0.5f;
    [SerializeField]private Vector2 waveDirection = new Vector2(1.0f, 0.5f);


    private static WaterManager _instance;

    [Header("Mesh Settings")]
    public int width = 100;
    public int height = 100;
    public float cellScale = 1.0f;


    private void Awake() {
        if (_instance == null) {
            _instance = this;
        }
        else {
            Destroy(this);
        }
    }

    private void Start() {
        UpdateMaterialProperties();
    }

    public void UpdateMaterialProperties()
    {
        if (waterMat == null) return;

        waterMat.SetColor("_WaterColor", waterColor);
        waterMat.SetFloat("_amp", waveAmplitude);
        waterMat.SetFloat("_waveLen", waveLength);
        waterMat.SetFloat("_speed", waveSpeed);
        waterMat.SetFloat("_steep", waveSteepness);
        waterMat.SetVector("_dir", new Vector4(waveDirection.x, waveDirection.y, 0, 0));
    }

    public float GetWaterHeight(Vector3 pos) {
        float w = 6.2831853f / waveLength;
        Vector2 d = waveDirection.normalized;
        float proj = Vector2.Dot(d, new Vector2(pos.x , pos.z));

        float phase = waveSpeed * w * Time.time;
        float angle = w * proj + phase ;

        // Vector3 p = new Vector3();
        // p.x = pos.x + waveSteepness * waveAmplitude * d.x * Mathf.Cos(angle);
        // p.z = pos.z + waveSteepness * waveAmplitude * d.y * Mathf.Cos(angle);
        return waveAmplitude * Mathf.Sin(angle);
    }


    public void GenerateOceanMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.name = "OceanPlane";

        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[width * height * 6];

        // 1. Generate Vertices & UVs
        for (int z = 0, i = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                vertices[i] = new Vector3(x * cellScale, 0, z * cellScale);
                uvs[i] = new Vector2((float)x / width, (float)z / height);
            }
        }

        // 2. Generate Triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        // 3. Assign to Mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh; // Use sharedMesh in Editor!
        Debug.Log($"[OceanGenerator] Mesh generated with {vertices.Length} vertices!");
    }
}
