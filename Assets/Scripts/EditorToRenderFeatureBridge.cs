using UnityEngine;
using UnityEngine.Rendering;

public class EditorToRenderFeatureBridge {
    public static Camera _gameCam;
    public static RTHandle _gameCamDepthTexture;
    public static bool isEnabled = false;
    public static bool hasData => _gameCam != null;
}
