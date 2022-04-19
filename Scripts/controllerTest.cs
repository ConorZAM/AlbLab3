using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerTest : MonoBehaviour
{
    public HingeJoint hj;
    public float angle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        JointSpring js = hj.spring;
        js.targetPosition = angle;
        hj.spring = js;
    }
}
