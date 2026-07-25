using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Floater : MonoBehaviour {
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MeshRenderer mr;
    [Tooltip("has to be child of the floater")]
    [SerializeField] private Vector3 customCOM;
    [field: SerializeField] public List<Vector3> FloatPointsObjSpace { get; private set; }
    [SerializeField] private float waterDrag;

    private WaterManager _waterManager;

    private float _volumn;
    private float _volumnPerPoint;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        mr = GetComponent<MeshRenderer>();

        _volumn = mr.bounds.size.x * mr.bounds.size.z * mr.bounds.size.y;
        _volumnPerPoint = _volumn / FloatPointsObjSpace.Count;
    }

    private void Start() {
        _waterManager = FindAnyObjectByType<WaterManager>();
    }

    private void FixedUpdate() {
        if (!_waterManager) {
            return;
        }

        foreach (Vector3 fPos  in FloatPointsObjSpace) {
            var pos =  transform.TransformPoint(fPos);
            float waterHeight = _waterManager.GetWaterHeight(pos);

            float underWaterHeight = Mathf.Clamp(waterHeight - pos.y, 0,
                Vector3.Distance(mr.bounds.max , mr.bounds.min));

            float immersion = underWaterHeight / Vector3.Distance(mr.bounds.max, mr.bounds.min);

            // this is not the real lifeway , but a working approximation
            float mag =(_volumnPerPoint * Physics.gravity.magnitude * immersion);
            if (underWaterHeight  > 0) {
                Debug.DrawRay(pos, Vector3.up * mag, Color.yellow);
                rb.AddForceAtPosition( Vector3.up * mag, pos, ForceMode.Force);

                Vector3 pointVelocity = rb.GetPointVelocity(pos);
                rb.AddForceAtPosition(-pointVelocity * waterDrag , pos , ForceMode.Force);
            }
        }

        rb.AddTorque(-rb.angularVelocity * waterDrag, ForceMode.Force);
        Debug.DrawRay(rb.transform.position, rb.linearVelocity, Color.yellow);
    }

    public void AddFloatPoint(Vector3 pos) {
        FloatPointsObjSpace.Add(pos);
    }

    public void RemoveFloatPoint(Vector3 pos) {
        FloatPointsObjSpace.Remove(pos);
    }

    private void OnValidate() {
        _volumn = mr.bounds.size.x * mr.bounds.size.z * mr.bounds.size.y;
        _volumnPerPoint = _volumn / FloatPointsObjSpace.Count;
        rb.ResetCenterOfMass();
        rb.centerOfMass = rb.centerOfMass + customCOM;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rb.worldCenterOfMass, rb.linearVelocity);
        Gizmos.DrawSphere(rb.worldCenterOfMass, 0.1f);

        Gizmos.color = Color.dodgerBlue;
        foreach (Vector3 p in FloatPointsObjSpace) {
            Gizmos.DrawSphere(transform.TransformPoint(p), 0.1f);
        }
    }

#endif

}
