using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(WaterManager))]
public class WaterManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Draw default fields (width, height, cellScale)
        DrawDefaultInspector();

        // 2. Get reference to target script
        WaterManager generator = (WaterManager)target;
        MeshRenderer mr = (MeshRenderer)target;

        EditorGUILayout.Space(10);

        // 3. Render Inspector Button
        if (GUILayout.Button("Generate Ocean Mesh", GUILayout.Height(30)))
        {
            // Register undo so pressing Ctrl+Z reverts the generated mesh
            Undo.RegisterCompleteObjectUndo(generator.gameObject, "Generate Ocean Mesh");

            // generator.GenerateOceanMesh();
            Mesh m = mr.additionalVertexStreams;
        }
    }
}
