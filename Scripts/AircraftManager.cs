using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftManager : MonoBehaviour
{
    public bool isControlling = false;

    public Transform centreOfGravity;
    public Transform portAileron, starboardAileron, portElevator, starboardElevator, portFlap, starboardFlap;
    public WheelCollider noseGear;
    public AeroBody portWingOuter, portWingInner, starboardWingOuter, starboardWingInner, portTailPlane, starboardTailPlane;
    public Rigidbody rb;
    public float elevatorTrim, aileronTrim, rudderTrim;
    Quaternion portAileronTrim, starboardAileronTrim, portElevatorTrim, starboardElevatorTrim, starboardFlapTrim, portFlapTrim;
    public float maxControlThrow = 35; // in deg
    public float flapDelta; //high lift device deflection in deg
    public float maxThrust; //in N
    public float aileronDelta, elevatorDelta, rudderDelta, thrust; // control inputs indeg
    ConfigurableJoint cj;

    public Thruster thruster;
    enum Flapsetting { up, down };
    Flapsetting flapSetting = Flapsetting.up;
    float flapAngle = 0;
    float flapVelocity, flapTarget;
    public float flapDeployTime;

    // Start is called before the first frame update
    void Start()
    {
        if (isControlling)
        {
            portAileronTrim = portAileron.localRotation;
            starboardAileronTrim = starboardAileron.localRotation;
            portElevatorTrim = portElevator.localRotation;
            starboardElevatorTrim = starboardElevator.localRotation;
            portFlapTrim = portFlap.localRotation;
            starboardFlapTrim = starboardFlap.localRotation;
            //rudderTrim = rudderHinge.localRotation;

            rb.centerOfMass = centreOfGravity.localPosition;
            if (cj) { cj.anchor = centreOfGravity.localPosition; }

            // Setting this here just for ease
            portWingInner.dynamicallyVariableShape = true;
            portWingOuter.dynamicallyVariableShape = true;
            starboardWingInner.dynamicallyVariableShape = true;
            starboardWingOuter.dynamicallyVariableShape = true;
            portTailPlane.dynamicallyVariableShape = true;
            starboardTailPlane.dynamicallyVariableShape = true;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isControlling)
        {
            // Apply thrust input
            thrust = Mathf.Clamp(maxThrust * Input.GetAxis("Thrust"), 0, maxThrust);
            thruster.ApplyThrust(thrust);

            // Get control flap inputs
            aileronDelta = -maxControlThrow * Input.GetAxis("Aileron");
            elevatorDelta = -maxControlThrow * Input.GetAxis("Elevator");
            rudderDelta = -maxControlThrow * Input.GetAxis("Rudder");
            if (Mathf.Sign(Input.GetAxis("Flap")) == 1) flapSetting = Flapsetting.down;
            else flapSetting = Flapsetting.up;


            // Apply control flap inputs
            SetControlSurface(portAileron, portWingOuter, portAileronTrim, aileronDelta);
            SetControlSurface(starboardAileron, starboardWingOuter, starboardAileronTrim, -aileronDelta);
            SetControlSurface(portElevator, portTailPlane, portElevatorTrim, elevatorDelta + rudderDelta);
            SetControlSurface(starboardElevator, starboardTailPlane, starboardElevatorTrim, elevatorDelta - rudderDelta);


            //apply high lift devices if activated
            if (flapSetting == Flapsetting.down) flapTarget = flapDelta;
            else flapTarget = 0;

            //apply nose gear rotation
            noseGear.steerAngle = -rudderDelta;

            flapAngle = Mathf.SmoothDamp(flapAngle, flapTarget, ref flapVelocity, flapDeployTime);
            SetControlSurface(portFlap, portWingInner, portFlapTrim, flapAngle);
            SetControlSurface(starboardFlap, starboardWingInner, starboardFlapTrim, flapAngle);
        }
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
        aeroBody.camber = 0.1f * camber;
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