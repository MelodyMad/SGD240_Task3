using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomMapGenerator))]
public class EditorCustomMapGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        CustomMapGenerator mapGen = (CustomMapGenerator)target;

        // Draw all default fields (the normal inspector UI)
        if (DrawDefaultInspector())
        {
            // Optional: auto update whenever you change a field
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateMap();
            }
        }

        // Manual Generate button
        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
}


