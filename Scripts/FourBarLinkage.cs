using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourBarLinkage : MonoBehaviour
{
    //
    Transform servoCrankRoot,  couplerServoEnd, couplerFlapEnd, flapRockerRoot;
    Transform couplerServoEndLive, couplerFlapEndLive;
    [Range(-30f,30f)]
    public float servoAngle;
    GameObject servoCrank, coupler, rocker;
    Quaternion servoCrankInitialRotation, couplerInitialRotation, rockerInitialRotation;
    float a,b,c,d,e,alpha1, alpha1init,alpha2plus3init;
    //float oldServoAngle;
    //public float alpha1deg;
    public Transform controlSurfaceHinge;
    public void Setup()
        
    {
        //function used in edit mode only

        GameObject servoCrankpf=Resources.Load<GameObject>("Prefabs/Servo Crank");
        GameObject couplerpf = Resources.Load<GameObject>("Prefabs/Coupler");
        GameObject rockerpf = Resources.Load<GameObject>("Prefabs/Rocker");

        servoCrankRoot = transform.Find("nodes/Servo Crank Root");
        //if (servoCrankRoot==null) print("servo node not found");
        
        couplerFlapEnd = transform.Find("nodes/Coupler Flap End");
        couplerServoEnd = transform.Find("nodes/Coupler Servo End");
        flapRockerRoot = transform.Find("nodes/Flap Rocker Root");

        

        servoCrank = Instantiate(servoCrankpf, servoCrankRoot.position, Quaternion.LookRotation(couplerServoEnd.position - servoCrankRoot.position),transform);
        coupler = Instantiate(couplerpf, couplerServoEnd.position, Quaternion.LookRotation(couplerFlapEnd.position - couplerServoEnd.position, Vector3.down), transform);
        rocker = Instantiate(rockerpf, flapRockerRoot.position, Quaternion.LookRotation(couplerFlapEnd.position - flapRockerRoot.position,Vector3.down), transform);
        
        coupler.GetComponent<SetCouplerLength>().setLength((couplerServoEnd.position - couplerFlapEnd.position).magnitude);
        var vrscale = rocker.transform.Find("Rotation/Scale").localScale;
        rocker.transform.Find("Rotation/Scale").localScale = new Vector3(vrscale.x,  (couplerFlapEnd.position - flapRockerRoot.position).magnitude/2, vrscale.z);

        //add coupler transforms to rocker and crank objects for use later
        var t1=Instantiate(couplerServoEnd, servoCrank.transform);
        var t2 =Instantiate(couplerFlapEnd, rocker.transform);
        t1.position = couplerServoEnd.position;
        t2.position = couplerFlapEnd.position;
        //t1.parent = servoCrank.transform;
        //t2.parent = rocker.transform;


    }
    private void Start()
    {
        //STore refences to original transforms at set up
        servoCrankRoot = transform.Find("nodes/Servo Crank Root");
        couplerFlapEnd = transform.Find("nodes/Coupler Flap End");
        couplerServoEnd = transform.Find("nodes/Coupler Servo End");
        flapRockerRoot = transform.Find("nodes/Flap Rocker Root");

        couplerServoEndLive = transform.Find("Servo Crank(Clone)/Coupler Servo End(Clone)");
        couplerFlapEndLive = transform.Find("Rocker(Clone)/Coupler Flap End(Clone)");

        //store object refernces
        servoCrank = transform.Find("Servo Crank(Clone)").gameObject;
        coupler = transform.Find("Coupler(Clone)").gameObject;
        rocker = transform.Find("Rocker(Clone)").gameObject;
        //store initial rotations
        servoCrankInitialRotation = servoCrank.transform.localRotation;
        couplerInitialRotation = coupler.transform.localRotation;
        rockerInitialRotation = rocker.transform.localRotation;

        //calculate fixed math quantities for fourbar link kinematics

             

        a = (servoCrankRoot.position - flapRockerRoot.position).magnitude;
        b = (couplerServoEnd.position - servoCrankRoot.position).magnitude;
        c = (couplerFlapEnd.position - couplerServoEnd.position).magnitude;
        d = (couplerFlapEnd.position - flapRockerRoot.position).magnitude;

        Vector3 AB = couplerServoEnd.position - servoCrankRoot.position;
        Vector3 AD = flapRockerRoot.position - servoCrankRoot.position;

        alpha1init = Vector3.Angle(AD, AB);

        Vector3 DC = couplerFlapEnd.position - flapRockerRoot.position;
        alpha2plus3init = Vector3.Angle(-AD, DC);
    }
    private void FixedUpdate()
    {
        servoCrank.transform.localRotation = servoCrankInitialRotation * Quaternion.Euler(servoAngle, 0, 0);

        alpha1 = Mathf.Deg2Rad * (alpha1init + servoAngle);
       
        float esquared = a * a + b * b - 2 * a * b * Mathf.Cos(alpha1);
        
        float e = Mathf.Sqrt(esquared);
        float alpha2 = Mathf.Acos((esquared + a * a - b * b) / (2 * e * a));
        float alpha3 = Mathf.Acos((d * d + esquared - c * c) / (2 * d * e));
        float alpha2plus3 = alpha2 + alpha3;

        coupler.transform.position = couplerServoEndLive.position; //servoCrank.transform.localPosition+ new Vector3(0, b * Mathf.Sin( Mathf.PI - alpha1), b * Mathf.Cos( Mathf.PI - alpha1));
        coupler.transform.localRotation = Quaternion.LookRotation(couplerFlapEndLive.position - couplerServoEndLive.position);

        rocker.transform.localRotation = rockerInitialRotation * Quaternion.Euler(-Mathf.Rad2Deg* alpha2plus3+alpha2plus3init, 0, 0);
        if (controlSurfaceHinge != null) controlSurfaceHinge.localRotation = Quaternion.Inverse(rockerInitialRotation) * rocker.transform.localRotation;
    }

}
