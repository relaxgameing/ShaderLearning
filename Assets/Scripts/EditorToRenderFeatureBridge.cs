using UnityEngine;

public class EditorToRenderFeatureBridge {
    public static Camera _gameCam;
    public static bool isEnabled = false;
    public static bool hasData => _gameCam != null;
}
