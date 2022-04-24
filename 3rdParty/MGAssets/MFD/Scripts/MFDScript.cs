///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////// MFD - Digital Flight Instruments - Script Version 1.0.191007 - Unity 2018.3.4f1 - Maloke Games 2019
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////
////// This script is used for repassing the values calculated on the MainMFD to a separeted GUI
//////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;

public class MFDScript : MonoBehaviour
{
    //Config Variables
    [Header("MFD GUI")]
    public bool isActive = false;
    
    [Space]
    public string activeMsg = "MFD Activated";
    public DisplayMsg consoleMsg;
    public Image[] mfdGUI;
    
    [Space]
    public AircraftManager aircraftManager;

    [Space(5)]
    [Header("Roll")]
    public bool useRoll = true;
    public RectTransform horizonRoll;
    public Text horizonRollTxt;

    [Space(5)]
    [Header("Pitch")]
    public bool usePitch = true;
    public RectTransform horizonPitch;
    public Text horizonPitchTxt;

    [Space(5)]
    [Header("Heading & TurnRate")]
    public bool useHeading = true;
    public RectTransform compassHSI;
    public Text headingTxt;
    public CompassBar compassBar;
    public RollDigitIndicator headingRollDigit;


    [Space]
    public bool useTurnRate = true;
    public Text turnRateTxt;
    public ArrowIndicator turnRateIndicator;
    public PointerIndicator turnRatePointer;


    [Space(5)]
    [Header("Altitude")]
    public bool useAltitude = true;
    public RollDigitIndicator altitudeRollDigit;
    public PointerIndicator altitudePointer;
    public Text altitudeTxt;

    [Space(5)]
    [Header("AirSpeed")]
    public bool useSpeed = true;
    public NeedleIndicator speedNeedle;
    public RollDigitIndicator speedRollDigit;
    public PointerIndicator speedPointer;
    public Text speedTxt;


    [Space(5)]
    [Header("Vertical Velocity")]
    public bool useVV = true;
    public NeedleIndicator vvNeedle;
    public ArrowIndicator vvArrow;
    public RollDigitIndicator vvRollDigit;
    public bool roundVV = true, showDecimalVV = true;
    public float roundFactorVV = 0.1f;
    public Text verticalSpeedTxt;

    [Space(5)]
    [Header("Horizontal Velocity")]
    public bool useHV = true;
    public NeedleIndicator hvNeedle;
    public ArrowIndicator hvArrow;
    public bool roundHV = true, showDecimalHV = true;
    public float roundFactorHV = 0.1f;
    public Text horizontalSpeedTxt;


    [Space(5)]
    [Header("G-Force")]
    public bool useGForce = true;
    public Text gForceTxt, maxGForceTxt, minGForceTxt;


    [Space(5)]
    [Header("AOA, AOS and GlidePath")]
    public bool useAlphaBeta = true;
    public NeedleIndicator alphaNeedle;
    public ArrowIndicator alphaArrow;
    public Text alphaTxt;

    [Space]
    public NeedleIndicator betaNeedle;
    public ArrowIndicator betaArrow;
    public Text betaTxt;

    [Space]
    public bool useGlidePath = true;
    public RectTransform glidePath;


    [Space(5)]
    [Header("Engine and Fuel")]
    public bool useEngine = true;
    public PointerIndicator enginePointer;
    public RollDigitIndicator engineRollDigit;
    public Slider engineSliderUI;
    public Image engineFillUI;
    public Text engineTxt;

    [Space]
    public bool useFuel = true;
    public PointerIndicator fuelPointer;
    public RollDigitIndicator fuelRollDigit;
    public Slider fuelSliderUI;
    public Image fuelFillUI;
    public Text fuelTxt;

    [Space]
    public Image fuelFlowFillUI;
    public Text fuelFlowTxt;


    [Space(5)]
    [Header("Temperature")]
    public bool useTemperature = false;
    public RollDigitIndicator temperatureRollDigit;
    public PointerIndicator temperaturePointer;
    public Slider temperatureSliderUI;
    public Image temperatureFillUI;
    public Text temperatureTxt;

    [Space(5)]
    [Header("Flaps & Gear")]
    public bool useFlaps = true;
    public Slider flapsSliderUI;
    public Image flapsFillUI;
    public Text flapsTxt;

    [Space]
    public bool useGear = false;
    public GameObject[] gearLights;
    public FlashImg[] gearFlashLights;
    public Text gearTxt;



    //All Flight Variables
    [Space(10)]
    [Header("Flight Variables - ReadOnly!")]
    public bool gearDown = false;
    public int flapsIndex = 0;
    public float currentFlap, gear, speed;
    public float altitude, pitch, roll, heading, turnRate, gForce, maxGForce, minGForce, alpha, beta, vv, hv, engine, fuel, fuelFlow, temperature;
    //



    //Set Default Values via Editor -> This will be implemented in future updates
    //[ContextMenu("Default Simulation")] void setDefaultSimulation() { Debug.Log("Default Simulation!"); }
    //


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Inicialization
    void OnEnable() { ResetHud();}
    //
    public void ResetHud()
    {
        //Values to Reset        
        if (useGForce)
        {
            maxGForce = 0f; minGForce = 0f;
            if (maxGForceTxt != null) maxGForceTxt.text = "0.0";
            if (minGForceTxt != null) minGForceTxt.text = "0.0";
        }
        //
        if (useGear) gear = 0.5f;
        //

        isActive = true;
        if (consoleMsg != null) consoleMsg.displayQuickMsg(activeMsg); else DisplayMsg.showAll(activeMsg, 5);
    }
    //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Inicialization




    ////////////////////////////////////////// Calls for Configs
    public void setGUIs(bool active = true) { for (int i = 0; i < mfdGUI.Length; i++) mfdGUI[i].enabled = active; }
    public void setAllGUIs(bool active = true)
    {
        if (MainMFD.current == null) return;
        MFDScript[] mfd = MainMFD.current.GetComponentsInChildren<MFDScript>(true);
        for (int i = 0; i < mfd.Length; i++) mfd[i].setGUIs(active);
    }
    public void setTextOutline(bool active = true)
    {
        if (MainMFD.current == null) return;
        foreach (Outline outLine in MainMFD.current.GetComponentsInChildren<Outline>(true)) outLine.enabled = active;
    }
    public void setGaugesColor(int index = -1)
    {
        if (MainMFD.current == null) return;
        ColorImg[] colorImg = MainMFD.current.GetComponentsInChildren<ColorImg>(true);
        for (int i = 0; i < colorImg.Length; i++) if (index == -1) colorImg[i].toogleColor(); else colorImg[i].setColor(index); //colorImg[i].setColor((colorImg[i].indexColor == 0) ? 1 : 0);
    }
    public void setTextColor(int index = -1)
    {
        if (MainMFD.current == null) return;
        ColorTxt[] colorTxt = MainMFD.current.GetComponentsInChildren<ColorTxt>(true);
        for (int i = 0; i < colorTxt.Length; i++) if (index == -1) colorTxt[i].toogleColor(); else colorTxt[i].setColor(index);
    }
    public void setBackgroundBlack(bool black = true)
    {
        if (MainMFD.current == null) return;
        ToogleImg[] masks = MainMFD.current.GetComponentsInChildren<ToogleImg>(true);
        if (black) for (int i = 0; i < masks.Length; i++) masks[i].disableMaskGraph(); else for (int i = 0; i < masks.Length; i++) masks[i].enableMaskGraph();
    }
    ////////////////////////////////////////// Calls for Configs



    /////////////////////////////////////////////////////// Update GUIs
    void FixedUpdate()
    {
        // Return if not active
        if (!isActive) return;
        if (MainMFD.current == null) return;


        //////////////////////////////////////////// Compass, Heading and/or HSI + Turn Rate
        if (useHeading)
        {
            heading = MainMFD.current.heading;

            //Send values to Gui and Instruments
            if (compassHSI != null) compassHSI.localRotation = Quaternion.Euler(0, 0, heading);
            if (compassBar != null) compassBar.setValue(heading);
            if (headingRollDigit != null) headingRollDigit.setValue((heading < 0) ? (heading + 360f) : heading);
            if (headingTxt != null) { if (heading < 0) headingTxt.text = (heading + 360f).ToString("000"); else headingTxt.text = heading.ToString("000"); }

        }
        //
        if (useTurnRate)
        {
            turnRate = MainMFD.current.turnRate;

            //Send values to Gui and Instruments
            if (turnRateIndicator != null) turnRateIndicator.setValue(turnRate);
            if (turnRatePointer != null) turnRatePointer.setValue(turnRate);
            if (turnRateTxt != null) { turnRateTxt.text = turnRate.ToString("0"); }
        }
        //////////////////////////////////////////// Compass, Heading and/or HSI + Turn Rate


        //////////////////////////////////////////// Roll
        if (useRoll)
        {
            roll = MainMFD.current.roll;

            //Send values to Gui and Instruments
            if (horizonRoll != null) horizonRoll.localRotation = Quaternion.Euler(0, 0, MainMFD.current.rollAmplitude * roll);
            if (horizonRollTxt != null)
            {
                //horizonRollTxt.text = roll.ToString("##");
                if (roll > 180) horizonRollTxt.text = (roll - 360).ToString("00");
                else if (roll < -180) horizonRollTxt.text = (roll + 360).ToString("00");
                else horizonRollTxt.text = roll.ToString("00");
            }
            //
        }
        //////////////////////////////////////////// Roll


        //////////////////////////////////////////// Pitch
        if (usePitch)
        {
            pitch = MainMFD.current.pitch;

            //Send values to Gui and Instruments
            if (horizonPitch != null) horizonPitch.localPosition = new Vector3(-MainMFD.current.pitchAmplitude * pitch * Mathf.Sin(horizonPitch.transform.localEulerAngles.z * Mathf.Deg2Rad) + MainMFD.current.pitchXOffSet, MainMFD.current.pitchAmplitude * pitch * Mathf.Cos(horizonPitch.transform.localEulerAngles.z * Mathf.Deg2Rad) + MainMFD.current.pitchYOffSet, 0);
            if (horizonPitchTxt != null) horizonPitchTxt.text = pitch.ToString("0");
        }
        //////////////////////////////////////////// Pitch


        //////////////////////////////////////////// Altitude
        if (useAltitude)
        {
            altitude = MainMFD.current.altitude;

            //Send values to Gui and Instruments
            if (altitudeRollDigit != null) altitudeRollDigit.setValue(altitude);
            if (altitudePointer != null) altitudePointer.setValue(altitude);
            if (altitudeTxt != null) altitudeTxt.text = altitude.ToString("0").PadLeft(5);
        }
        //////////////////////////////////////////// Altitude


        //////////////////////////////////////////// Speed
        if (useSpeed)
        {
            speed = MainMFD.current.speed;

            //Send values to Gui and Instruments
            if (speedNeedle != null) speedNeedle.setValue(speed);
            if (speedRollDigit != null) speedRollDigit.setValue(speed);
            if (speedPointer != null) speedPointer.setValue(speed);
            if (speedTxt != null) speedTxt.text = speed.ToString("0").PadLeft(5);//.ToString("##0");
        }
        //////////////////////////////////////////// Speed


        //////////////////////////////////////////// Vertical Velocity - VV
        if (useVV)
        {
            vv = MainMFD.current.vv;

            //Send values to Gui and Instruments
            if (vvNeedle != null) vvNeedle.setValue(vv);
            if (vvArrow != null) vvArrow.setValue(vv);
            if (vvRollDigit != null) vvRollDigit.setValue(vv);
            if (verticalSpeedTxt != null)
            {
                if (roundVV)
                {
                    if (showDecimalVV) verticalSpeedTxt.text = (System.Math.Round(vv / roundFactorVV, System.MidpointRounding.AwayFromZero) * roundFactorVV).ToString("0.0").PadLeft(4);
                    else verticalSpeedTxt.text = (System.Math.Round(vv / roundFactorVV, System.MidpointRounding.AwayFromZero) * roundFactorVV).ToString("0").PadLeft(3);
                }
                else
                {
                    if (showDecimalVV) verticalSpeedTxt.text = (vv).ToString("0.0").PadLeft(4);
                    else verticalSpeedTxt.text = (vv).ToString("0").PadLeft(3);
                }

            }
        }
        //////////////////////////////////////////// Vertical Velocity - VV


        //////////////////////////////////////////// Horizontal Velocity - HV
        if (useHV)
        {
            hv = MainMFD.current.hv;

            //Send values to Gui and Instruments
            if (hvNeedle != null) hvNeedle.setValue(hv);
            if (hvArrow != null) hvArrow.setValue(hv);
            if (horizontalSpeedTxt != null)
            {
                if (roundHV)
                {
                    if (showDecimalHV) horizontalSpeedTxt.text = (System.Math.Round(hv / roundFactorHV, System.MidpointRounding.AwayFromZero) * roundFactorHV).ToString("0.0").PadLeft(4);
                    else horizontalSpeedTxt.text = (System.Math.Round(hv / roundFactorHV, System.MidpointRounding.AwayFromZero) * roundFactorHV).ToString("0").PadLeft(3);
                }
                else
                {
                    if (showDecimalHV) horizontalSpeedTxt.text = (hv).ToString("0.0").PadLeft(4);
                    else horizontalSpeedTxt.text = (hv).ToString("0").PadLeft(3);
                }
            }
        }
        //////////////////////////////////////////// Horizontal Velocity - HV


        //////////////////////////////////////////// Vertical G-Force 
        if (useGForce)
        {
            //G-FORCE -> Gravity + Vertical Acceleration + Centripetal Acceleration (v * w) radians
            gForce = MainMFD.current.gForce;

            //Send values to Gui and Instruments
            if (gForceTxt != null) gForceTxt.text = gForce.ToString("0.0").PadLeft(3);
            if (gForce > maxGForce)
            {
                maxGForce = gForce;
                if (maxGForceTxt != null) maxGForceTxt.text = maxGForce.ToString("0.0").PadLeft(3);
            }
            if (gForce < minGForce)
            {
                minGForce = gForce;
                if (minGForceTxt != null) minGForceTxt.text = minGForce.ToString("0.0").PadLeft(3);
            }
            //
        }
        ////////////////////////////////////////////  Vertical G-Force 


        //////////////////////////////////////////////// AOA (Alpha) + AOS (Beta) + GlidePath (Velocity Vector)
        if (useAlphaBeta || useGlidePath)
        {
            //Calculate both Angles
            alpha = MainMFD.current.alpha;
            beta = MainMFD.current.beta;

            //Apply angle values to the glidePath UI element
            if (useGlidePath && glidePath != null) glidePath.localPosition = Vector3.Lerp(glidePath.localPosition, new Vector3(Mathf.Clamp(-beta * MainMFD.current.pitchAmplitude, -MainMFD.current.glideXDeltaClamp, MainMFD.current.glideXDeltaClamp), Mathf.Clamp(alpha * MainMFD.current.pitchAmplitude, -MainMFD.current.glideYDeltaClamp, MainMFD.current.glideYDeltaClamp), 0), MainMFD.current.glidePathFilterFactor);


            //Send values to Instruments
            if (useAlphaBeta)
            {
                if (alphaNeedle != null) alphaNeedle.setValue(alpha);
                if (alphaArrow != null) alphaArrow.setValue(alpha);
                if (betaNeedle != null) betaNeedle.setValue(beta);
                if (betaArrow != null) betaArrow.setValue(beta);

                //Send values to Gui Text
                if (alphaTxt != null) alphaTxt.text = alpha.ToString("0").PadLeft(3);
                if (betaTxt != null) betaTxt.text = beta.ToString("0").PadLeft(3);
            }
            //
        }
        //////////////////////////////////////////////// AOA (Alpha) + AOS (Beta)


        //////////////////////////////////////////// Engine & Fuel
        if (useEngine)
        {
            //Updates current Engine RPM
            //engine = MainMFD.current.engine;
            engine = aircraftManager.thrust;
            

            //Send values to Gui and Instruments
            if (engineRollDigit != null) engineRollDigit.setValue(engine);
            if (enginePointer != null) enginePointer.setValue(engine);
            if (engineSliderUI != null) engineSliderUI.value = (engine / aircraftManager.maxThrust);
            if (engineFillUI != null) engineFillUI.fillAmount = (engine / aircraftManager.maxThrust);
            if (engineTxt != null) engineTxt.text = engine.ToString("##0" +"N");
        }
        //
        if (useFuel)
        {
            //Updates current Fuel value
            fuel = MainMFD.current.fuel;
            fuelFlow = MainMFD.current.fuelFlow;

            //Send values to Gui and Instruments
            if (fuelRollDigit != null) fuelRollDigit.setValue(fuel);
            if (fuelPointer != null) fuelPointer.setValue(fuel);
            if (fuelSliderUI != null) fuelSliderUI.value = (fuel / MainMFD.current.maxFuel);
            if (fuelFillUI != null) fuelFillUI.fillAmount = (fuel / MainMFD.current.maxFuel);
            if (fuelTxt != null) fuelTxt.text = fuel.ToString("##0");

            //Consumption in Minutes for 100% Fuel
            if (fuelFlowFillUI != null) fuelFlowFillUI.fillAmount = fuelFlow / (Time.fixedDeltaTime / (60f * MainMFD.current.fuelMinTime));
            if (fuelFlowTxt != null) fuelFlowTxt.text = (MainMFD.current.fuelAmplitude * fuelFlow * MainMFD.current.fuelFlowAmplitude / Time.fixedDeltaTime).ToString("##0.0");//.ToString("0.0").PadLeft(4);  //.ToString("##0");     
            //
        }
        //////////////////////////////////////////// Engine & Fuel


        //////////////////////////////////////////// Temperature
        if (useTemperature)
        {
            //Updates current Engine Temperature
            temperature = MainMFD.current.temperature;

            //Send values to Gui and Instruments
            if (temperatureRollDigit != null) temperatureRollDigit.setValue(temperature);
            if (temperaturePointer != null) temperaturePointer.setValue(temperature / MainMFD.current.maxTemperature);
            if (temperatureSliderUI != null) temperatureSliderUI.value = (temperature / MainMFD.current.maxTemperature);
            if (temperatureFillUI != null) temperatureFillUI.fillAmount = (temperature / MainMFD.current.maxTemperature);
            if (temperatureTxt != null) temperatureTxt.text = temperature.ToString("##0");
        }
        //////////////////////////////////////////// Temeprature



        ////////////////////////////////////////////// Flaps
        if (useFlaps)
        {
            //Update Flap
            currentFlap = MainMFD.current.currentFlap;
            flapsIndex = MainMFD.current.flapsIndex;

            //Send values to Gui and Instruments
            if (flapsSliderUI != null) flapsSliderUI.value = currentFlap;
            if (flapsFillUI != null) flapsFillUI.fillAmount = currentFlap;
            if (flapsTxt != null) flapsTxt.text = MainMFD.current.flaps[flapsIndex];
        }
        ////////////////////////////////////////////// Flaps


        ////////////////////////////////////////////// Gear
        if (useGear)
        {
            gearDown = MainMFD.current.gearDown;


            //Gear Changes
            if (gearDown && gear < 1)
            {
                if(gear != 0.5f) foreach (FlashImg light in gearFlashLights) light.flash();
                foreach (GameObject light in gearLights) light.gameObject.SetActive(true);

                if (gear != 0.5f) SndPlayer.play(3);
                gear = 1;

                
                if (gearTxt != null) gearTxt.text = "DOWN";
            }
            else if (!gearDown && gear > 0)
            {

                if (gear != 0.5f) foreach (FlashImg light in gearFlashLights) light.flash();
                foreach (GameObject light in gearLights) light.gameObject.SetActive(false);

                if (gear != 0.5f) SndPlayer.play(3);
                gear = 0;

                if (gearTxt != null) gearTxt.text = "UP";
            }
        }
        ////////////////////////////////////////////// Gear



    }
    /////////////////////////////////////////////////////// Update GUIs
}
