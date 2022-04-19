using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AlignAxis))]

public class AlignAxisEditor : Editor
{

    public override void OnInspectorGUI()
    {
        AlignAxis myAlignAxisScript = (AlignAxis)target;
       

        DrawDefaultInspector();

        if (GUILayout.Button("Setup axis now"))
        {

            myAlignAxisScript.SetUpAxis();
        }

    }
}
