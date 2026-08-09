using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraViewVisualiser : EditorWindow {
    private bool _isEnabled = true;
    private Shader _shader;
    private Material _mat;
    private Camera _gameCam;

    [MenuItem("Tools/Camear view visualiser")]
    private static void ShowWindow() {
        var window = GetWindow<CameraViewVisualiser>();
        window.titleContent = new GUIContent("Camera View Visualiser");
        window.Show();
    }

    private void OnGUI() {
        _shader = (Shader)EditorGUILayout.ObjectField(
            "Effect Shader",
            _shader,
            typeof(Shader),
            true
        );

        _gameCam = (Camera)EditorGUILayout.ObjectField(
            "Game Camera",
            _gameCam,
            typeof(Camera),
            true
        );

        if (GUILayout.Button($"{(_isEnabled ? "OFF" : "ON")} Visualiser",
                GUILayout
                    .Height(30))) {

            _isEnabled = !_isEnabled;

            _gameCam.depthTextureMode = DepthTextureMode.Depth;

            EditorToRenderFeatureBridge._gameCam = _gameCam;
            EditorToRenderFeatureBridge.isEnabled = _isEnabled;

        }
    }

}
