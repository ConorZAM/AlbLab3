using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WindTunnelExperiment))]
public class WindTunnelExperimentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Data is saved in the Assets folder in \"Unity Wind Tunnel Data.txt\"");

        DrawDefaultInspector();

        if(GUILayout.Button("Set Aircraft Rotation"))
        {
            WindTunnelExperiment manager = (WindTunnelExperiment)target;
            manager.SetAircraftRotation(manager.desiredAlpha);
        }
    }
}
