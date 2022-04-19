using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WindTunnelExperiment))]
public class WindTunnelExperimentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Set Aircraft Rotation"))
        {
            WindTunnelExperiment manager = (WindTunnelExperiment)target;
            manager.SetAircraftRotation(manager.desiredAlpha);
        }
    }
}
