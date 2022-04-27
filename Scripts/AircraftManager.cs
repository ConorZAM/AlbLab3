using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftManager : MonoBehaviour
{
    [Tooltip("Enables Keyboard Inputs")]
    public bool usePilotControls = false;

    [Header("Aircraft inertial properties")]
    public Rigidbody aircraftRigidBody;
    public float mass = 6;
    public Vector3 inertiaTensor = new Vector3(0.92f, 1.96f, 1.08f);

    // Control settings
    [Header("Control Polarity")]
    public bool ReverseEvelevator;
    public bool ReverseAileron;
    public bool ReverseRudder;
    public bool ReverseFlap;
    public bool ReverseThrottle;

    [Header("Control Trim")]
    public float elevatorTrim;
    public float aileronTrim, rudderTrim;

    [Header("Control Limits")]
    public float maxControlThrow = 35; // in deg
    public float flapDelta; //high lift device deflection in deg
    public float maxThrust; //in N
    public float flapDeployTime;

    [Header("Aircraft Visuals")]
    public Material aircraftMaterial;
    [Range(0, 1)]
    public float bodyTransparency;
    public bool useParticleSystems;
    public GameObject particleSystems;

    [Header("Components")]
    public Transform portAileron;
    public Transform starboardAileron, portElevator, starboardElevator, portFlap, starboardFlap;
    public WheelCollider noseGear;
    public AeroBody portWingOuter, portWingInner, starboardWingOuter, starboardWingInner, portTailPlane, starboardTailPlane;
    
    // Other settings
    Quaternion portAileronTrim, starboardAileronTrim, portElevatorTrim, starboardElevatorTrim, starboardFlapTrim, portFlapTrim;
    float camberScale = 0.05f;
    [HideInInspector]
    public float aileronDelta, elevatorDelta, rudderDelta, thrust; // control inputs in deg

    public freeWheels wheels;

    Thruster thruster;
    enum Flapsetting { up, down };
    Flapsetting flapSetting = Flapsetting.up;
    float flapAngle = 0;
    float flapVelocity, flapTarget;
    

    // Start is called before the first frame update
    void Start()
    {
        aircraftRigidBody.mass = mass;
        aircraftRigidBody.inertiaTensor = inertiaTensor;
        aircraftRigidBody.inertiaTensorRotation = Quaternion.identity;

        thruster = FindObjectOfType<Thruster>();

        portAileronTrim = portAileron.localRotation;
        starboardAileronTrim = starboardAileron.localRotation;
        portElevatorTrim = portElevator.localRotation;
        starboardElevatorTrim = starboardElevator.localRotation;
        portFlapTrim = portFlap.localRotation;
        starboardFlapTrim = starboardFlap.localRotation;

        //rudderTrim = rudderHinge.localRotation;

        // Setting this here just for ease
        portWingInner.dynamicallyVariableShape = true;
        portWingOuter.dynamicallyVariableShape = true;
        starboardWingInner.dynamicallyVariableShape = true;
        starboardWingOuter.dynamicallyVariableShape = true;
        portTailPlane.dynamicallyVariableShape = true;
        starboardTailPlane.dynamicallyVariableShape = true;

        //change aircraft transparency
        var col = aircraftMaterial.color;
        col.a = bodyTransparency;
        aircraftMaterial.color = col;

        if (particleSystems != null)
        {
            //enable particle systems for wing tips
            if (useParticleSystems) particleSystems.SetActive(true);
            else particleSystems.SetActive(false);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (usePilotControls)
        {
            GetControlInputs();
            ApplyControls();
        }
    }

    void GetControlInputs()
    {
        thrust = Mathf.Clamp(maxThrust * Input.GetAxis("Thrust"), 0, maxThrust);

        // Get control flap inputs
        aileronDelta = Mathf.Clamp(-maxControlThrow * Input.GetAxis("Aileron") - aileronTrim, -maxControlThrow, maxControlThrow);
        elevatorDelta = Mathf.Clamp(-maxControlThrow * Input.GetAxis("Elevator") - elevatorTrim, -maxControlThrow, maxControlThrow);
        rudderDelta = Mathf.Clamp(-maxControlThrow * Input.GetAxis("Rudder") - rudderTrim, -maxControlThrow, maxControlThrow);

        elevatorTrim = Input.GetAxis("Elevator Trim")*10;
        
        // Flap is more like a button
        if (Input.GetButtonDown("FlapDown"))
        {
            if (ReverseFlap)
            {
                flapSetting = Flapsetting.up;
            }
            else
            {
                flapSetting = Flapsetting.down;
                
            }
        }
        // No else here, could have both buttons pressed
        if (Input.GetButtonDown("FlapUp"))
        {
            if (ReverseFlap)
            {
                flapSetting = Flapsetting.down;
            }
            else
            {
                flapSetting = Flapsetting.up;

            }
        }

        // Funky switch expression
        flapTarget = flapSetting switch
        {
            Flapsetting.up => 0,
            Flapsetting.down => flapDelta,
            _ => 0,
        };

        // wheel brakes
        if (Input.GetKey("space")) wheels.brakeTorque = 100;
        else wheels.brakeTorque = 0;

        // Polarity
        if (ReverseThrottle)
            thrust *= -1f;
        if (ReverseAileron)
            aileronDelta *= -1f;
        if (ReverseEvelevator)
            elevatorDelta *= -1f;
        if (ReverseRudder)
            rudderDelta *= -1f;
    }

    void ApplyControls()
    {
        // Apply thrust input        
        thruster.ApplyThrust(thrust);

        // Apply control surface inputs
        SetControlSurface(portAileron, portWingOuter, portAileronTrim, aileronDelta);
        SetControlSurface(starboardAileron, starboardWingOuter, starboardAileronTrim, -aileronDelta);
        SetControlSurface(portElevator, portTailPlane, portElevatorTrim, elevatorDelta + rudderDelta);
        SetControlSurface(starboardElevator, starboardTailPlane, starboardElevatorTrim, elevatorDelta - rudderDelta);

        //apply nose gear rotation
        noseGear.steerAngle = -rudderDelta;

        // Apply flap angles
        flapAngle = Mathf.SmoothDamp(flapAngle, flapTarget, ref flapVelocity, flapDeployTime);
        SetControlSurface(portFlap, portWingInner, portFlapTrim, flapAngle);
        SetControlSurface(starboardFlap, starboardWingInner, starboardFlapTrim, flapAngle);
    }

    public void SetControlSurface(Transform hinge, AeroBody aeroBody, Quaternion trim, float delta)
    {
        //controlHinge.localRotation = trim * Quaternion.Euler(delta, 0, 0);
        hinge.localRotation = trim * Quaternion.Euler(0, 0, delta);

        //hinge.localEulerAngles = new Vector3(0, 0, delta);
        float camber = delta;
        if (camber > 180) camber -= 360;
        //if (camber < -180) camber += 360;

        // Minus sign here, not sure who's got things the wrong way around...
        camber = -camber * Mathf.Deg2Rad;
        aeroBody.SetCamber(camberScale * camber);
    }

    public void SetElevatorDeflection(float delta)
    {
        SetControlSurface(portElevator, portTailPlane, portElevatorTrim, delta);
        SetControlSurface(starboardElevator, starboardTailPlane, starboardElevatorTrim, delta);
    }

    public void SetAileronDeflection(float delta)
    {
        SetControlSurface(portAileron, portWingOuter, portAileronTrim, delta);
        SetControlSurface(starboardAileron, starboardWingOuter, starboardAileronTrim, -delta);
    }

    public void SetFlapDeflection(float delta)
    {
        SetControlSurface(portFlap, portWingInner, portFlapTrim, -delta);
        SetControlSurface(starboardFlap, starboardWingInner, starboardFlapTrim, -delta);
    }
}