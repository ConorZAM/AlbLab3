using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.MoveTowards(  target.position, target.TransformVector( -offset), 1);
        transform.position = target.position - offset;
        transform.rotation = Quaternion.LookRotation(target.position - transform.position);
    }
}
