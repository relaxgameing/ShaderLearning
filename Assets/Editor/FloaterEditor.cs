using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(Floater))]
public class FloaterEditor : Editor {
    private Floater floater;
    private MeshFilter _mf;

    private bool _isSelecting = false;
    private HashSet<Vector3> _selectedVertex;
    private List<Vector3> _shownVertex;

    private Vector3 pointerPos;

    private void OnEnable() {
        floater = target.GetComponent<Floater>();
        if (_selectedVertex == null) {
            _selectedVertex = new();
        }

        _mf = floater.GetComponent<MeshFilter>();
    }

    public override void OnInspectorGUI() {
        // 1. Draw default fields (width, height, cellScale)
        DrawDefaultInspector();

        // 2. Get reference to target script

        EditorGUILayout.Space(10);

        // 3. Render Inspector Button
        if (GUILayout.Button($"{(_isSelecting ? "Stop selection" : "Start selecting face")}",
                GUILayout
                    .Height(30))) {
            _isSelecting = !_isSelecting;
        }
    }


    private void OnSceneGUI() {
        if (!_isSelecting) {
            return;
        }

        DrawSelectedVertices();

        Event e = Event.current;
        SceneView view = SceneView.currentDrawingSceneView;
        Camera cam = view.camera;

        if (view == null || cam == null) {
            Debug.Log("no scene or cam");
            return;
        }

        Handles.color = Color.green;
        Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        bool hit = Physics.Raycast(r, out RaycastHit hitInfo);

        if (!hit) {
            return;
        }

        Debug.Log("hit");

        if (hitInfo.collider.gameObject != target.GameObject()) {
            return;
        }


        var vert = _mf.sharedMesh.vertices;

        var min = floater.transform.TransformPoint(vert[0]);
        var minDist = Vector3.Distance(hitInfo.point, min);
        foreach (Vector3 v in vert) {
            var worldPos = floater.transform.TransformPoint(v);
            var curDist = Vector3.Distance(worldPos, hitInfo.point);
            if (curDist < minDist) {
                min = worldPos;
                minDist = curDist;
            }
        }

        Handles.color = Color.white;
        Handles.DrawWireCube(
            min,
            Vector3.one * 0.2f
        );

        if (e.type == EventType.MouseDown) {
            if (_selectedVertex.Contains(min)) {
                floater.RemoveFloatPoint(min);
            }
            else {
                floater.AddFloatPoint(min);
            }
        }

        if (e.isMouse) {
            e.Use();
        }
    }

    private void DrawSelectedVertices() {
        foreach (Vector3 v in floater.FloatPoints) {
            Handles.color = Color.green;
            Handles.DrawWireCube(
                v,
                Vector3.one * 0.2f
            );
        }
    }

}
