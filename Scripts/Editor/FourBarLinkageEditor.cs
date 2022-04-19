using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FourBarLinkage))]

public class FourBarLinkageEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        FourBarLinkage myFourBarScript = (FourBarLinkage)target;
        //myTidyTreeScript.depron = EditorGUILayout.(myTidyTreeScript.depron);
        //Material depron = new 

        base.DrawDefaultInspector();

        if (GUILayout.Button("Setup linkage now"))
        {

            myFourBarScript.Setup();
        }

    }
}
