using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
[CustomEditor(typeof(ProcessFBXPrefabFromSolidworks))]

public class TreeEditor : Editor
    
{
    
    public override void OnInspectorGUI()
    {
        ProcessFBXPrefabFromSolidworks myPrefabPrcessingScript = (ProcessFBXPrefabFromSolidworks)target;
        //myTidyTreeScript.depron = EditorGUILayout.(myTidyTreeScript.depron);
        //Material depron = new 

        if (GUILayout.Button("Process this prefab now"))
        {

            myPrefabPrcessingScript.ProcessNow();
        }
        
    }
    
}
