using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapRipper))]

public class MapTileRipperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapRipper myMapRipperScript = (MapRipper)target;


        DrawDefaultInspector();

        if (GUILayout.Button("rip now"))
        {
            Debug.Log("Ripping!");
            myMapRipperScript.RipNow();
        }

    }
}
