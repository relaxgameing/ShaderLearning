using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraViewVisualiser : EditorWindow {
    private bool _isEnabled = true;
    private Shader _shader;
    private Material _mat;
    private Camera _gameCam;

    private void OnSceneCameraPostRender(ScriptableRenderContext scriptableRenderContext, Camera camera) {
        if (!_isEnabled || camera.cameraType != CameraType.SceneView) {
            return;
        }

        camera.depthTextureMode |= DepthTextureMode.Depth;

        if (_gameCam == null) {
            Debug.LogError("camera whose volume to visualise is not selected");
            return;
        }

        if (_shader == null) {
            _shader = Shader.Find("Learning/CameraVolumeVisualiser");
        }

        if (_shader  != null && _mat == null ) {
            _mat = new Material(_shader);
        }

        if (_mat != null && _gameCam != null)
        {
            RenderTexture activeRT = RenderTexture.active;
            if (activeRT == null) {
                return;
            }
            RenderTexture tempRT = RenderTexture.GetTemporary(
                activeRT.width,
                activeRT.height,
                activeRT.depth,
                activeRT.format
            );

            // 2. Copy current scene render into the temporary texture
            Graphics.Blit(activeRT, tempRT);

            _mat.SetMatrix("_GameCamViewMat" , camera.worldToCameraMatrix);
            _mat.SetVector("_GameCamPosWS" , _gameCam.transform.position);

            // Debug.Log($"{_gameCam.transform.position}");

            // 3. Blit from temporary texture back to scene target through your material
            Graphics.Blit(tempRT, activeRT, _mat);

            // 4. Release the temporary texture back to Unity memory pool
            RenderTexture.ReleaseTemporary(tempRT);
            Debug.Log("camera view visualiser working");
        }
    }

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

            EditorToRenderFeatureBridge._gameCam = _gameCam;
            EditorToRenderFeatureBridge.isEnabled = _isEnabled;
        }
    }

}
