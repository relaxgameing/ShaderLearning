using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum FloaterEditorState {
    NotActive,
    FloaterPointSelection,
    // CenterOfMassSelection,
}

[CustomEditor(typeof(Floater))]
public class FloaterEditor : Editor {
    private Floater _floater;
    private MeshFilter _mf;
    private FloaterEditorState _state = FloaterEditorState.NotActive;

    private void OnEnable() {
        _floater = target.GetComponent<Floater>();
        _mf = _floater.GetComponent<MeshFilter>();
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        GUILayout.Label("Floater editor options");
        EditorGUILayout.Space(5);
        if (GUILayout.Button($"{(_state == FloaterEditorState.FloaterPointSelection ? "Stop selection" : "Start selecting face")}",
                GUILayout
                    .Height(30))) {

            if (_state == FloaterEditorState.NotActive) {
                _state = FloaterEditorState.FloaterPointSelection;
            }else {
                _state = FloaterEditorState.NotActive;
            }
        }
    }


    private void OnSceneGUI() {
        FloatingPointSelection();
    }

    private void FloatingPointSelection() {
        if (_state != FloaterEditorState.FloaterPointSelection) {
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


        if (hitInfo.collider.gameObject != target.GameObject()) {
            return;
        }

        Debug.Log("hit");

        var vert = _mf.sharedMesh.vertices;

        var min = vert[0];
        var minDist = Vector3.Distance(hitInfo.point, _floater.transform.TransformPoint(min));
        foreach (Vector3 v in vert) {
            var worldPos = _floater.transform.TransformPoint(v);
            var curDist = Vector3.Distance(worldPos, hitInfo.point);
            if (curDist < minDist) {
                min = v;
                minDist = curDist;
            }
        }

        Handles.color = Color.white;
        Handles.DrawWireCube(
            _floater.transform.TransformPoint(min),
            Vector3.one * 0.2f
        );

        if (e.type == EventType.MouseDown) {
            if (_floater.FloatPointsObjSpace.Contains(min)) {
                _floater.RemoveFloatPoint(min);
            }
            else {
                _floater.AddFloatPoint(min);
            }
        }

        if (e.isMouse) {
            e.Use();
        }
    }

    private void DrawSelectedVertices() {
        foreach (Vector3 v in _floater.FloatPointsObjSpace) {
            Handles.color = Color.green;
            Handles.DrawWireCube(
                _floater.transform.TransformPoint(v),
                Vector3.one * 0.2f
            );
        }
    }

}
