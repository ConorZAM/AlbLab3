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

        ForceBalance forceBalance = (ForceBalance)target;
        if (forceBalance.jointMode == ForceBalance.JointMode.Fixed || forceBalance.jointMode == ForceBalance.JointMode.Gimbal)
        {
            // Only allow for taring when the joint holds the aircraft in place
            if (GUILayout.Button("Tare"))
            {

                forceBalance.Tare();
            }
        }
    }
}
