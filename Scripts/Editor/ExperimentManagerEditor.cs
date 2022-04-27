using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlightDynamicsLabManager))]
public class ExperimentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Update Experiment Setup"))
        {
            FlightDynamicsLabManager manager = (FlightDynamicsLabManager)target;
            manager.DoExperimentSetup();
        }
    }
}
