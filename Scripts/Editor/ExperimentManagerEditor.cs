using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExperimentManager))]
public class ExperimentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Update Experiment Setup"))
        {
            ExperimentManager manager = (ExperimentManager)target;
            manager.DoExperimentSetup();
        }
    }
}
