using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(RipMaps))]

public class RipMapsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RipMaps ripMaps = (RipMaps)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Rip"))
        {
            ripMaps.Rip();
        }
    }
}
