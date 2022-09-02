using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftManager : MonoBehaviour
{
    static AircraftManager _singleton;
    public static AircraftManager Singleton()
    {
        if (_singleton == null)
        {
            _singleton = FindObjectOfType<AircraftManager>();
        }
        return _singleton;
    }

    [Tooltip("Your student number determines the initial wiring setup of the aircraft. Your job is to adjust the polarities of each control surface so each of the inputs has its desired effect")]
    public int studentNumber;

    [Tooltip("Enables Keyboard Inputs")]
    public bool usePilotControls = false;

    [Header("Aircraft inertial properties")]
    public Rigidbody aircraftRigidBody;
    public float mass = 7f;
    private readonly Vector3 inertiaTensor = new Vector3(0.37f, 1.546f, 1.12f);

    
    int elevatorSign, aileronSign, flapSign, rudderSign;

    [Header("Control Inputs")]
    public float aileronInput;
    public float elevatorInput, rudderInput, thrustInput;
    Flapsetting flapSettingInput = Flapsetting.up; 
    

    // Control settings
    [Header("Control Polarity")]
    public bool ReverseElevator;
    public bool ReverseAileron;
    public bool ReverseRudder;
    public bool ReverseFlap;
    public bool ReverseThrottle;

    [Header("Control Trim")]
    public float elevatorTrim;
    public float aileronTrim, rudderTrim;

    [Header("Control Limits")]
    public float maxControlThrow = 25; // in deg
    public float maxFlapDeflection = 40f;
    public float maxThrust = 30f; //in N
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
    public float aileronDelta, elevatorDelta, rudderDelta, flapDelta, thrust; // control inputs in deg

    freeWheels wheels;

    Thruster thruster;
    enum Flapsetting { up, down };
    
    float flapAngle = 0;
    float flapVelocity, flapTarget;
    

    // Start is called before the first frame update
    void Start()
    {
        aircraftRigidBody.mass = mass;
        aircraftRigidBody.inertiaTensor = inertiaTensor;
        aircraftRigidBody.inertiaTensorRotation = Quaternion.identity;

        thruster = FindObjectOfType<Thruster>();
        wheels = FindObjectOfType<freeWheels>();

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

        if (particleSystems != null)
        {
            //enable particle systems for wing tips
            if (useParticleSystems) particleSystems.SetActive(true);
            else particleSystems.SetActive(false);
        }

        //set up the random intial control signs
        Random.InitState(studentNumber);
        elevatorSign = Random.Range(0, 2) * 2 - 1;
        rudderSign = Random.Range(0, 2) * 2 - 1;
        aileronSign = Random.Range(0, 2) * 2 - 1;
        flapSign = Random.Range(0, 2) * 2 - 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetControlInputs();
        ApplyTrim();
        ApplyControls();
    }

    public void UpdateTransparency(float transparency)
    {
        bodyTransparency = transparency;
        //change aircraft transparency
        var col = aircraftMaterial.color;
        col.a = bodyTransparency;
        aircraftMaterial.color = col;
        
    }

    void GetControlInputs()
    {
        if (usePilotControls)
        {
            thrustInput = maxThrust * Input.GetAxis("Thrust");

            // Get control flap inputs
            aileronInput = Mathf.Clamp(-maxControlThrow * Input.GetAxis("Aileron"), -maxControlThrow, maxControlThrow);
            rudderInput = Mathf.Clamp(-maxControlThrow * Input.GetAxis("Rudder"), -maxControlThrow, maxControlThrow);
            elevatorInput = Mathf.Clamp(-maxControlThrow * Input.GetAxis("Elevator"), -maxControlThrow, maxControlThrow);
            
            // Get trim
            elevatorTrim = Input.GetAxis("Elevator Trim") * maxControlThrow;

            if (Input.GetButton("FlapDown"))
            {
                flapSettingInput = Flapsetting.down;
                flapVelocity = 0;
            }
            // No else here, could have both buttons pressed
            if (Input.GetButton("FlapUp"))
            {
                flapSettingInput = Flapsetting.up;
            }

            // Funky switch expression
            flapTarget = flapSettingInput switch
            {
                Flapsetting.up => 0,
                Flapsetting.down => flapDelta,
                _ => 0,
            };

            // wheel brakes
            if (Input.GetKey("space"))
            {
                wheels.brakeTorque = 1;
                wheels.applyBrakes = true;
            }
            else
            {
                wheels.brakeTorque = 0;
                wheels.applyBrakes = false;
            }


            // Apply deltas from controls... this is a bit weird but it's to make sure the correct
            // control inputs are shown
            aileronDelta = aileronInput;
            elevatorDelta = elevatorInput;
            rudderDelta = rudderInput;
            thrust = Mathf.Clamp(thrustInput, 0, maxThrust);

        }
        else
        {
            aileronDelta = 0;
            elevatorDelta = 0;
            rudderDelta = 0;
        }


    }

    void ApplyTrim()
    {

        // Get control flap inputs
        aileronDelta = aileronSign * Mathf.Clamp(aileronDelta - aileronTrim, -maxControlThrow, maxControlThrow);
        elevatorDelta = elevatorSign * Mathf.Clamp(elevatorDelta - elevatorTrim, -maxControlThrow, maxControlThrow);
        rudderDelta = rudderSign * Mathf.Clamp(rudderDelta - rudderTrim, -maxControlThrow, maxControlThrow);

        // Polarity
        if (ReverseThrottle)
            thrust *= -1f;
        if (ReverseAileron)
            aileronDelta *= -1f;
        if (ReverseElevator)
            elevatorDelta *= -1f;
        if (ReverseRudder)
            rudderDelta *= -1f;
        if (ReverseFlap)
            flapDelta = flapSign * maxFlapDeflection;
        else flapDelta = -1f * flapSign * maxFlapDeflection;

        // Funky switch expression
        flapTarget = flapSettingInput switch
        {
            Flapsetting.up => 0,
            Flapsetting.down => flapDelta,
            _ => 0,
        };
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
        flapAngle = Mathf.SmoothDamp(flapAngle, flapTarget, ref flapVelocity, flapDeployTime, 100000f, Time.fixedDeltaTime);
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
        aeroBody.SetFlapCamber(camberScale * camber);
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








/*          Bill's version!
 * 
 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftManager : MonoBehaviour
{
    [Tooltip("Enables Keyboard Inputs")]
    public bool usePilotControls = false;

    [Header("Aircraft inertial properties")]
    public Rigidbody aircraftRigidBody;
    public float mass = 7f;
    private readonly Vector3 inertiaTensor = new Vector3(0.37f, 1.546f, 1.12f);

    public int studentNumber;
    int elevatorSign, aileronSign, flapSign, rudderSign;
    // Control settings
    [Header("Control Polarity")]
    public bool ReverseElevator;
    public bool ReverseAileron;
    public bool ReverseRudder;
    public bool ReverseFlap; //doesnt make physical sense as is
    public bool ReverseThrottle;
     

    [Header("Control Trim")]
    public float elevatorTrim;
    public float aileronTrim, rudderTrim;

    [Header("Control Limits")]
    public float maxControlThrow = 35; // in deg
    public float maxFlapDeflection; //high lift device deflection in deg
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
    public float aileronDelta, elevatorDelta, rudderDelta, flapDelta, thrust; // control inputs in deg

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

        //set up the random intial control signs
        Random.InitState(studentNumber);
        elevatorSign = Random.Range(0, 2)*2-1;
        rudderSign = Random.Range(0, 2) * 2 - 1;
        aileronSign = Random.Range(0, 2) * 2 - 1;
        flapSign = Random.Range(0, 2) * 2 - 1;
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
        aileronDelta =aileronSign* Mathf.Clamp(-maxControlThrow * Input.GetAxis("Aileron") - aileronTrim, -maxControlThrow, maxControlThrow);
        elevatorDelta =elevatorSign* Mathf.Clamp(-maxControlThrow * Input.GetAxis("Elevator") - elevatorTrim, -maxControlThrow, maxControlThrow);
        rudderDelta = rudderSign*Mathf.Clamp(-maxControlThrow * Input.GetAxis("Rudder") - rudderTrim, -maxControlThrow, maxControlThrow);

        elevatorTrim = Input.GetAxis("Elevator Trim")*10;
        
        // Flap is more like a button
        if (Input.GetButtonDown("FlapDown"))
        {
            //note the reverse logic was not as intended here - the direction of flap defl is what is needed to change, not the key that does it
           
            
                flapSetting = Flapsetting.down;
                
            
        }
        // No else here, could have both buttons pressed
        if (Input.GetButtonDown("FlapUp"))
        {
            
                flapSetting = Flapsetting.up;

            
        }

        
        

       

        // wheel brakes
        if (Input.GetKey("space")) wheels.brakeTorque = 100;
        else wheels.brakeTorque = 0;

        // Polarity
        if (ReverseThrottle)
            thrust *= -1f;
        if (ReverseAileron)
            aileronDelta *= -1f;
        if (ReverseElevator)
            elevatorDelta *= -1f;
        if (ReverseRudder)
            rudderDelta *= -1f;
        if (ReverseFlap)
            flapDelta = flapSign* maxFlapDeflection;
        else flapDelta = -1f *flapSign* maxFlapDeflection;

        // Funky switch expression
        flapTarget = flapSetting switch
        {
            Flapsetting.up => 0,
            Flapsetting.down => flapDelta,
            _ => 0,
        };
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

*/
