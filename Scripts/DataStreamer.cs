using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataStreamer : MonoBehaviour
{
    // Quick script just to log things to Grapher
    ExperimentManager Manager { get { return ExperimentManager.Singleton(); } }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    void FixedUpdate()
    {
        // Manually calculating each angle...
        float pitch = 90f + Vector3.SignedAngle(Manager.aircraftRb.transform.forward, Vector3.up, Vector3.right);
        float yaw = Vector3.SignedAngle(Manager.aircraftRb.transform.forward, Vector3.forward, Vector3.up);
        float roll = Vector3.SignedAngle(Manager.aircraftRb.transform.right, Vector3.right, Vector3.forward);


        Vector3 aircraftAngles = CoordinateTransform.UnityToAircraftMoment(Manager.aircraftRb.transform.rotation.eulerAngles); // - new Vector3(180f, 180f, 180f);
        //if (aircraftAngles.x > 180) aircraftAngles.x -= 180f;
        //if (aircraftAngles.y > 180) aircraftAngles.y -= 180f;
        //if (aircraftAngles.z > 180) aircraftAngles.z -= 180f;
        //if (aircraftAngles.x < -180) aircraftAngles.x += 180f;
        //if (aircraftAngles.y < -180) aircraftAngles.y += 180f;
        //if (aircraftAngles.z < -180) aircraftAngles.z += 180f;
        Grapher.Log(pitch, "Pitch Angle (deg)");
        Grapher.Log(yaw, "Yaw Angle (deg)");
        Grapher.Log(roll, "Roll Angle (deg)");
    }
}