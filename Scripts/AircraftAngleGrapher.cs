using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftAngleGrapher : MonoBehaviour
{
    // Quick script just to log things to Grapher
    FlightDynamicsLabManager Manager { get { return FlightDynamicsLabManager.Singleton(); } }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    void FixedUpdate()
    {
        // Manually calculating each angle...
        float pitch = Vector3.SignedAngle(Manager.aircraftRb.transform.forward, Vector3.up, Vector3.right) - 90f;
        float yaw = Vector3.SignedAngle(Manager.aircraftRb.transform.forward, Vector3.forward, Vector3.up) - 180f;
        float roll = Vector3.SignedAngle(Manager.aircraftRb.transform.right, Vector3.up, Vector3.forward);
        Grapher.Log(pitch, "Angle of attack (deg)");
        Grapher.Log(yaw, "Yaw (deg)");
        Grapher.Log(roll, "Roll (deg)");

        //Vector3 aircraftForward = Manager.aircraftRb.transform.forward;

        //Quaternion q = Manager.aircraftRb.rotation;
        //float roll = Mathf.Rad2Deg * Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);
        //float pitch = Mathf.Rad2Deg * Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
        //float yaw = Mathf.Rad2Deg * Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);

        //Grapher.Log(pitch, "Angle of attack (deg)");
        //Grapher.Log(yaw, "Yaw (deg)");
        //Grapher.Log(roll, "Roll (deg)");

        //Vector3 aircraftAngles = CoordinateTransform.UnityToAircraftMoment(Manager.aircraftRb.ro); // - new Vector3(180f, 180f, 180f);
        //Grapher.Log(aircraftAngles.x, "Angle of attack (deg)");
        //Grapher.Log(aircraftAngles.y, "Yaw (deg)");
        //Grapher.Log(aircraftAngles.z, "Roll (deg)");
        //if (aircraftAngles.x > 180) aircraftAngles.x -= 180f;
        //if (aircraftAngles.y > 180) aircraftAngles.y -= 180f;
        //if (aircraftAngles.z > 180) aircraftAngles.z -= 180f;
        //if (aircraftAngles.x < -180) aircraftAngles.x += 180f;
        //if (aircraftAngles.y < -180) aircraftAngles.y += 180f;
        //if (aircraftAngles.z < -180) aircraftAngles.z += 180f;

    }
}
