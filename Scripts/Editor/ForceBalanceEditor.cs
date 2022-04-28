using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ForceBalance))]
public class ForceBalanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Only allow for taring when the joint holds the aircraft in place
        if (GUILayout.Button("Tare"))
        {
            ForceBalance forceBalance = (ForceBalance)target;
            forceBalance.Tare();
        }

    }
}