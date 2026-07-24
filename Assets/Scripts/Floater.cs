using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Floater : MonoBehaviour {

    [SerializeField] private Rigidbody rg;
    [SerializeField] private MeshRenderer mr;
    [field: SerializeField] public List<Vector3> FloatPointsObjSpace { get; private set; }
    private WaterManager _waterManager;

    private float _volumn;
    private float _volumnPerPoint;

    private void Awake() {
        rg = GetComponent<Rigidbody>();
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

            float mag =(_volumnPerPoint * Physics.gravity.magnitude *   underWaterHeight );
            if (underWaterHeight  > 0) {
                Debug.DrawRay(pos, Vector3.up * mag, Color.yellow);

                rg.AddForceAtPosition( Vector3.up * mag, pos, ForceMode.Force);
            }
        }

        Debug.DrawRay(rg.transform.position, rg.linearVelocity, Color.yellow);
    }

    public void AddFloatPoint(Vector3 pos) {
        FloatPointsObjSpace.Add(pos);
    }

    public void RemoveFloatPoint(Vector3 pos) {
        FloatPointsObjSpace.Remove(pos);
    }


#if UNITY_EDITOR

    private void OnDrawGizmos() {
        var pos = transform.position;
        pos.y -= mr.bounds.extents.y;
        Gizmos.DrawRay(pos, rg.linearVelocity);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pos, 0.1f);

        Gizmos.color = Color.dodgerBlue;
        foreach (Vector3 p in FloatPointsObjSpace) {
            Gizmos.DrawSphere(transform.TransformPoint(p), 0.1f);
        }
    }

#endif

}
