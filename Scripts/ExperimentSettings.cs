using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ExperimentSettings", menuName = "Experiment Settings", order = 1)]
public class ExperimentSettings : ScriptableObject
{
    public enum JointState
    {
        Fixed,
        Lateral,
        Longitudinal,
        Gimbal,
        Free
    };
    // Everything we need for setting up the scene

    public Vector3 aircraftPosition = new Vector3(10, 2, -35);
    public Vector3 cameraPosition;
    public Vector3 cameraEulerAngles;
    public JointState jointState = JointState.Fixed;
    public string DataManagerName = "Name of GameObject";
}