using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Floater : MonoBehaviour {

    [SerializeField] private Rigidbody rg;
    [SerializeField] private MeshRenderer mr;
    [field: SerializeField] public List<Vector3> FloatPoints { get; private set; }
    private WaterManager _waterManager;

    private float _area;

    private void Awake() {
        rg = GetComponent<Rigidbody>();
        mr = GetComponent<MeshRenderer>();

        _area = mr.bounds.size.x * mr.bounds.size.z;
    }

    private void Start() {
        _waterManager = FindAnyObjectByType<WaterManager>();
    }

    private void FixedUpdate() {
        if (!_waterManager) {
            return;
        }

        Vector3 pos = transform.position;
        pos.y -= mr.bounds.extents.y;
        float waterHeight = _waterManager.GetWaterHeight(pos);

        float underWaterHeight = Mathf.Clamp(waterHeight - pos.y, 0,
                mr.bounds.size.y);

        if (underWaterHeight * _area > 0) {
            rg.AddForceAtPosition(Vector3.up * ( _area * underWaterHeight * 9.8f), pos,
                ForceMode.Force);
        }

        Debug.DrawRay(rg.transform.position, rg.linearVelocity, Color.yellow);
    }

    public void AddFloatPoint(Vector3 pos) {
        FloatPoints.Add(pos);
    }

    public void RemoveFloatPoint(Vector3 pos) {
        FloatPoints.Remove(pos);
    }


#if UNITY_EDITOR

    private void OnDrawGizmos() {
        var pos = transform.position;
        pos.y -= mr.bounds.extents.y;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(pos, rg.linearVelocity);
        Gizmos.DrawSphere(pos, 0.1f);
    }

#endif

}
