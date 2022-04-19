using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PilotCameraLookAt : MonoBehaviour
{
    public Transform target;
   
    Camera pilotCamera;
    public float fovRadius;
    // Start is called before the first frame update
    void Start()
    {
        pilotCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 range = target.position - transform.position;
        pilotCamera.fieldOfView =  Mathf.Clamp(Mathf.Atan(fovRadius / range.magnitude)*6.24f,5,60);
        transform.rotation = Quaternion.LookRotation(range);
    }
}
