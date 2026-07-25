using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

[System.Serializable]
public struct GerstnerWave {
    public Vector2 direction;
    public float amplitude;
    public float steepness;
    public float wavelength;
    public float speed;
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterManager : MonoBehaviour {
    [Header("Ocean Properties")] [SerializeField]
    private Material waterMat;
    [SerializeField] private Color waterColor = new Color(0f, 0.4f, 0.8f, 1f);

    [Header("Wave Definitions")] [SerializeField]
    private List<GerstnerWave> waves = new List<GerstnerWave>();
    // [SerializeField]private float waveAmplitude = 1.0f;
    // [SerializeField]private float waveLength = 10.0f;
    // [SerializeField]private float waveSpeed = 2.0f;
    // [SerializeField]private float waveSteepness = 0.5f;
    // [SerializeField]private Vector2 waveDirection = new Vector2(1.0f, 0.5f);

    private static readonly int WaveCountID = Shader.PropertyToID("_waveCount");
    private static readonly int WaveDataID = Shader.PropertyToID("_waveData");
    private static readonly int WaveDirID = Shader.PropertyToID("_waveDir");
    private const int MAX_WAVES = 16;

    private static WaterManager _instance;

    // [Header("Mesh Settings")] public int width = 100;
    // public int height = 100;
    // public float cellScale = 1.0f;


    private void Awake() {
        if (_instance == null) {
            _instance = this;
        }
        else {
            Destroy(this.gameObject);
        }
    }

    private void Start() {
        UpdateMaterialProperties();
    }

    public float GetWaterHeight(Vector3 worldPos)
    {
        Vector3 displacement = Vector3.zero;

        for (int i = 0; i < waves.Count; i++)
        {
            Vector2 dir = waves[i].direction.normalized;
            float w = 6.2831853f / Mathf.Max(0.0001f, waves[i].wavelength);
            float phase = waves[i].speed * w * Time.time;
            float angle = w * Vector2.Dot(dir, new Vector2(worldPos.x, worldPos.z)) + phase;

            displacement.y += waves[i].amplitude * Mathf.Sin(angle);
        }

        return  displacement.y;
    }


    public void UpdateMaterialProperties() {
        if (waterMat == null || waves == null) return;

        int count = Mathf.Min(waves.Count, MAX_WAVES);

        var _waveData = new Vector4[MAX_WAVES];
        var _waveDir = new Vector4[MAX_WAVES];

        for (int i = 0; i < count; i++) {
            var curWave = waves[i];

            // Pack:  (Amp , Wave length , speed , steepness)
            _waveData[i] = new Vector4(
                curWave.amplitude ,
                curWave.wavelength ,
                curWave.speed,
                curWave.steepness);

            // Pack: (direction.xy , 0 , 0 )
            var dir = curWave.direction.normalized;
            _waveDir[i] = new Vector4(dir.x , dir.y, 0, 0);
        }

        waterMat.SetInt(WaveCountID, count);
        waterMat.SetVectorArray(WaveDataID, _waveData);
        waterMat.SetVectorArray(WaveDirID, _waveDir);
    }

    private void OnValidate() {
        UpdateMaterialProperties();
    }


    // public void GenerateOceanMesh() {
    //     MeshFilter meshFilter = GetComponent<MeshFilter>();
    //     Mesh mesh = new Mesh();
    //     mesh.name = "OceanPlane";
    //
    //     Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
    //     Vector2[] uvs = new Vector2[vertices.Length];
    //     int[] triangles = new int[width * height * 6];
    //
    //     // 1. Generate Vertices & UVs
    //     for (int z = 0, i = 0; z <= height; z++) {
    //         for (int x = 0; x <= width; x++, i++) {
    //             vertices[i] = new Vector3(x * cellScale, 0, z * cellScale);
    //             uvs[i] = new Vector2((float)x / width, (float)z / height);
    //         }
    //     }
    //
    //     // 2. Generate Triangles
    //     int vert = 0;
    //     int tris = 0;
    //     for (int z = 0; z < height; z++) {
    //         for (int x = 0; x < width; x++) {
    //             triangles[tris + 0] = vert + 0;
    //             triangles[tris + 1] = vert + width + 1;
    //             triangles[tris + 2] = vert + 1;
    //             triangles[tris + 3] = vert + 1;
    //             triangles[tris + 4] = vert + width + 1;
    //             triangles[tris + 5] = vert + width + 2;
    //
    //             vert++;
    //             tris += 6;
    //         }
    //
    //         vert++;
    //     }
    //
    //     // 3. Assign to Mesh
    //     mesh.vertices = vertices;
    //     mesh.uv = uvs;
    //     mesh.triangles = triangles;
    //     mesh.RecalculateNormals();
    //     mesh.RecalculateBounds();
    //
    //     meshFilter.sharedMesh = mesh; // Use sharedMesh in Editor!
    //     Debug.Log($"[OceanGenerator] Mesh generated with {vertices.Length} vertices!");
    // }
}
