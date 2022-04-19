using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]

public class SetCouplerLength : MonoBehaviour
{
    [Range(0.06f,0.1f)]
    //public float couplerLength;
    Transform couplerFlapEnd;

    void Start()
    {
        couplerFlapEnd = transform.Find("Coupler flap end");
    }
    public void setLength(float length)
    {
        couplerFlapEnd = transform.Find("Coupler flap end");
        couplerFlapEnd.localPosition = new Vector3(0,0,length);
        
    }

    // Update is called once per frame
   
}
