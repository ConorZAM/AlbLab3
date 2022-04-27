using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WindTunnelExperiment))]
public class WindTunnelExperimentEditor : Editor
{
    private readonly string[] headerStrings = {"This script automates testing of a range of independent variables",
        "Lists of values are used when a default value is needed, otherwise an evenly spaced range of values are used",
        "The aircraft parameters will default to the first value in each list during testing",
        "Data is saved in the Assets folder in \"Unity Wind Tunnel Data.txt\""};
    public override void OnInspectorGUI()
    {
        for (int i = 0; i < headerStrings.Length; i++)
        {
            EditorGUILayout.LabelField(headerStrings[i], EditorStyles.boldLabel);
        }
        
        //GUILayout.Label("Data is saved in the Assets folder in \"Unity Wind Tunnel Data.txt\"");

        DrawDefaultInspector();

        //if(GUILayout.Button("Set Aircraft Rotation"))
        //{
        //    WindTunnelExperiment manager = (WindTunnelExperiment)target;
        //    manager.SetAircraftRotation(manager.desiredAlpha);
        //}
    }
}
