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

    public Vector3 aircraftPosition = new Vector3(13, 7, 25);
    public Vector3 cameraPosition;
    public Vector3 cameraEulerAngles;
    public JointState jointState = JointState.Fixed;
    public string DataManagerName = "Name of GameObject";
    public Vector3 aircraftVelocity = Vector3.zero;
    public float windSpeed = 10f;
    public float windAzimuth = 0f;
    public float windElevation = 0f;
}