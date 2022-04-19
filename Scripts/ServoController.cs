using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServoController : MonoBehaviour
{
    public Transform controlHinge, rocker;
    public HingeJoint servoCrank;
    public float servoAngle;
    Quaternion rockerInitalRotation;
    JointSpring js;
    // Start is called before the first frame update
    void Start()
    {
        js = servoCrank.spring;
        rockerInitalRotation = rocker.localRotation;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        js.targetPosition = servoAngle;
        servoCrank.spring = js;
        controlHinge.localRotation =Quaternion.Inverse( rockerInitalRotation)* rocker.localRotation ;
    }
}
