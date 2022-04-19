using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTunnelExperiment : MonoBehaviour
{
    public Environment environment;
    public SaveData saveData;
    Rigidbody rb;
    public float startAlpha, deltaAlpha;
    public int numberOfTestPoints;
    ConfigurableJoint configurableJoint;
    public float alpha, beta;
    Vector3  measuredForceCoefficients, measuredTorqueCoefficients, measuredForce, measuredTorque;
    public float timeDelta=0.5f;
    public Transform centreOfGravity;
    Vector3 centreOfGravityPosition;
    public List<float> testPoints;
    bool firstRun = true, proceed = false;
    float timer = 0;
    public float delayTime = 0.1f;
    Vector3 forceZero, torqueZero;
    bool initialise = true;
    public float wingArea, chord, density=1.2f, q;
    public AeroBody aeroBody;
    public ThinAerofoilComponent thinAerofoil;
    public float aeroBodyAlpha, aerobodyCL;
    //public Vector3 aeroBodyEarthFrameForce;


    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(-startAlpha, 0, 0);
        
        rb = GetComponent<Rigidbody>();
        centreOfGravityPosition = centreOfGravity.position;
        configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
        configurableJoint.anchor = centreOfGravity.localPosition;
        configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
        configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
        configurableJoint.xMotion = ConfigurableJointMotion.Locked;
        configurableJoint.yMotion = ConfigurableJointMotion.Locked;
        configurableJoint.zMotion = ConfigurableJointMotion.Locked;
        testPoints = new List<float>();
        
        for (int i=0; i<numberOfTestPoints; i++)
        {
            testPoints.Add(startAlpha + i * deltaAlpha);
        }
        environment.windSpeed = 0;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (initialise == true)
        {
            StartCoroutine(TakeZeros());
        }
        else if(proceed==true)
        {

            environment.windAzimuth = 180;
            environment.windElevation = 0;// 90 * Mathf.Sin(0.5f * Time.time);
            environment.windSpeed = 10;
            q = 0.5f * density * environment.windSpeed * environment.windSpeed ;

            timer += Time.deltaTime;

            if (timer > timeDelta)
            {
                timer = 0;
                Destroy(configurableJoint);
                transform.RotateAround(centreOfGravityPosition, Vector3.right, -5);

                configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
                configurableJoint.anchor = centreOfGravity.localPosition;
                configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
                configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
                configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
                configurableJoint.xMotion = ConfigurableJointMotion.Locked;
                configurableJoint.yMotion = ConfigurableJointMotion.Locked;
                configurableJoint.zMotion = ConfigurableJointMotion.Locked;

                StartCoroutine(DelayedSave());
            }
            measuredForce =  configurableJoint.currentForce;
            measuredTorque = configurableJoint.currentTorque;
            measuredForceCoefficients = -(measuredForce + forceZero)/(q*wingArea);
            measuredTorqueCoefficients = -(configurableJoint.currentTorque + torqueZero)/(q*wingArea*chord);
            
            alpha = 360f-transform.localEulerAngles.x;
            if (alpha > 180) alpha = alpha-360f;
        }
        aeroBodyAlpha = aeroBody.alpha_deg;
        aerobodyCL = thinAerofoil.CL;
        //aeroBodyEarthFrameForce = thinAerofoil.resultantForce_earthFrame;
        
        
    }
    IEnumerator DelayedSave()
    {
        yield return new WaitForSeconds(delayTime);
        saveData.Save();
        
        print("Im in delayed save");
        yield return null ;
    }

    IEnumerator TakeZeros()
    {
        initialise = false;
        yield return new WaitForSeconds(delayTime);
        forceZero = -configurableJoint.currentForce;
        torqueZero = -configurableJoint.currentTorque;
        print("Im in takes zeros, force zero is " + forceZero);
        proceed = true;
        ; 
    }
    
}
