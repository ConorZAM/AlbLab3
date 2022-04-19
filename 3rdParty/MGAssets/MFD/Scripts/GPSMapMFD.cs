using UnityEngine;
using UnityEngine.UI;


public class GPSMapMFD : MonoBehaviour
{
    [Header("Main (Master-Slave)")]
    public bool isActive = true;

    public enum Mode { Default = 0, Master = 1, Slave = 2 }
    public Mode mode = Mode.Default;

    public GPSMapMFD masterParent;
         

    [Space]
    [Header("Map Configuration")]
    public bool modeA = true; 
    public float mapScale = 1;

    [Space]
    [Range(1, 100)] public float zoom = 1;
    public float zoomStep = 1, zoomMin = 1, zoomMax = 1000;

    [Space]
    public bool autoCenterOffSet = true;
    [Range(-1, 1)] public float offSetX, offSetY;

    [Space]
    public Vector2 worldZeroXY = Vector2.zero;
    public Transform getZeroFrom;

    [Space]
    [Header("Keys")]
    public bool useKeys = true;
    public bool overrideMaster = false;
    public KeyCode zoomInKey = KeyCode.Equals, zoomOutKey = KeyCode.Minus, zoomResetKey = KeyCode.Backspace, toogleModeKey = KeyCode.M;


    [Header("References")]
    public Transform aircraft;

    public RectTransform aircraftCenter;

    public RawImage mapRaw;
    public RectTransform mapRect;
    public RectTransform mapMainPanel;


    [Space]
    [Header("Strings Data")]
    public Text zoomTxt;
    public Text modeTXT;


    [Header("Current Position - ReadOnly!")]
    public Vector2 currentPosition;


    //Internal Variables
    float currentZoom = -1, resetZoom, mapSizeX = 0, mapSizeY = 0;
    bool lastModeA = true;




    /////////////////////////////////////////////////////////////// Inicialization
    void Awake() { getAllRefs(); resetZoom = zoom;}
    void getAllRefs()
    {
        //Look for the Aircraft Reference on MainMFD
        if (mode == Mode.Default || mode == Mode.Master)
        {
            if(aircraft == null)
            {
                if (MainMFD.current != null && MainMFD.current.aircraft != null) aircraft = MainMFD.current.aircraft; 
                //if (aircraft == null) aircraft = Camera.main.transform;
            }
        }

        //Look for the Master script and Aircraf Ref when in Slave mode
        if (mode == Mode.Slave)
        {
            if (masterParent == null)
            {
                GPSMapMFD[] searchMasterParent = gameObject.GetComponentsInParent<GPSMapMFD>(true);
                foreach (GPSMapMFD gpsMap in searchMasterParent) if (gpsMap.mode == Mode.Master && gpsMap.isActive) { masterParent = gpsMap; break; }
            }

            if (aircraft == null && masterParent != null) aircraft = masterParent.aircraft;
        }

    }
    //
    void OnEnable()
    {
        if (!isActive) return;

        //Look for the Aircraft or MasterParent References
        if (aircraft == null || (mode == Mode.Slave && masterParent == null) ) getAllRefs();

        //Update Slave Initial Data from the Master
        if (mode == Mode.Slave) updateFromMaster();

        //World Zero Reference
        if (getZeroFrom != null) worldZeroXY = new Vector2(getZeroFrom.position.x, getZeroFrom.position.z);
        //

        //Sets Initial Position
        if (mapSizeX == 0 && mapRect != null) mapSizeX = mapRect.sizeDelta.x;
        if (mapSizeY == 0 && mapRect != null) mapSizeY = mapRect.sizeDelta.y;
            
        if (autoCenterOffSet)
        {
            autoCenterOffSet = false;
            offSetX = -(aircraft.position.x - worldZeroXY.x) / mapSizeX;
            offSetY = -(aircraft.position.z - worldZeroXY.y) / mapSizeY;
        }
        //

        DisplayMsg.showAll("GPS Map Enabled", 5);
    }
    /////////////////////////////////////////////////////////////// Inicialization




    ///////////////////////////////////////////////////// Get Values from Master
    bool firstTime = true;
    void updateFromMaster()
    {
        if (masterParent == null) { getAllRefs(); return; }

        if (firstTime)
        {
            firstTime = false;
            zoom = masterParent.zoom;
            modeA = masterParent.modeA;
        }

        mapScale = masterParent.mapScale;
        zoomStep = masterParent.zoomStep;
        zoomMin = masterParent.zoomMin;
        zoomMax = masterParent.zoomMax;
        resetZoom = masterParent.resetZoom;
        autoCenterOffSet = masterParent.autoCenterOffSet;
        offSetX = masterParent.offSetX;
        offSetY = masterParent.offSetY;
        worldZeroXY = masterParent.worldZeroXY;
        getZeroFrom = masterParent.getZeroFrom;
        aircraft = masterParent.aircraft;
        currentPosition = masterParent.currentPosition;

        //////mapSizeX = masterParent.mapSizeX; //Local value, do not get from master
        //////mapSizeY = masterParent.mapSizeY; //Local value, do not get from master
    }
    ///////////////////////////////////////////////////// Get Values from Master






    //////////////////////////////////////////////////////////////////////////////////// Update Map
    void Update()
    {
        if (!isActive) return;
        if (aircraft == null) { getAllRefs(); return; }

        //Update Data from the Master script
        if (mode == Mode.Slave) updateFromMaster();


        //Zoom Keys
        if (useKeys)
        {
            if (mode != Mode.Slave || overrideMaster)
            {
                if (Input.GetKeyDown(zoomInKey)) zoomIn();
                if (Input.GetKeyDown(zoomOutKey)) zoomOut();
                if (Input.GetKeyDown(zoomResetKey)) zoomReset();
                if (Input.GetKeyDown(toogleModeKey)) toogleMode();
            }
            else
            if(masterParent != null)
            {
                if (Input.GetKeyDown(masterParent.zoomInKey)) zoomIn();
                if (Input.GetKeyDown(masterParent.zoomOutKey)) zoomOut();
                if (Input.GetKeyDown(masterParent.zoomResetKey)) zoomReset();
                if (Input.GetKeyDown(masterParent.toogleModeKey)) toogleMode();
            }
        }
        //

        //Zoom Update
        if (currentZoom != zoom)
        {
            zoom -= zoom % zoomStep;
            zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
            currentZoom = zoom;

            if (mapRect != null) mapRect.sizeDelta = zoom * new Vector2(mapSizeX, mapSizeY);
            if (zoomTxt != null) zoomTxt.text = currentZoom + "x";
        }
        //
        if(lastModeA != modeA)
        {
            lastModeA = modeA;

            if (modeA) { if (aircraftCenter != null) aircraftCenter.localRotation = Quaternion.identity; }
            else { if(mapRect != null) mapRect.localRotation = Quaternion.identity; }                

            if (modeTXT != null) modeTXT.text = (modeA)? "Mode A" : "Mode B";
        }
        //

        //Update World Zero Reference
        if (mode != Mode.Slave)
        {
            if (getZeroFrom != null) worldZeroXY = new Vector2(getZeroFrom.position.x, getZeroFrom.position.z);
        }
        //

        //Translation - Position
        currentPosition = new Vector2(offSetX + (aircraft.position.x - worldZeroXY.x) / mapSizeX, offSetY + (aircraft.position.z - worldZeroXY.y) / mapSizeY);
        if(mapRaw != null) mapRaw.uvRect = new Rect(mapScale * currentPosition, mapRaw.uvRect.size);
        //


        // Rotation - Heading -> ModeA: FixedAircraft + RotateMap,  ModeB: RotateAircraft + FixedMap
        if (modeA)
        {
            if (mapRect != null) mapRect.localRotation = Quaternion.Euler(0, 0, MainMFD.current.heading); //= Quaternion.Euler(0, 0, aircraft.eulerAngles.y);
        }
        else
        {
            if (aircraftCenter != null) aircraftCenter.localRotation = Quaternion.Euler(0, 0, -MainMFD.current.heading); //= Quaternion.Euler(0, 0, -aircraft.eulerAngles.y);
        }
        //

    }
    //////////////////////////////////////////////////////////////////////////////////// Update Map






    ///////////////////////////////////////////////////////////////////// External Calls
    public void toogleMap()
    {
        SndPlayer.playClick();

        if (mapMainPanel.gameObject.activeSelf)
        {
            mapMainPanel.gameObject.SetActive(false);
            DisplayMsg.showAll("GPS Map Disabled", 5);
        }
        else
        {
            isActive = true;
            mapMainPanel.gameObject.SetActive(true);
            DisplayMsg.showAll("GPS Map Enabled", 5);
        }
    }
    //
    public void toogleMode() { modeA = !modeA; SndPlayer.playClick();}
    public void zoomIn(float value = 0) { if (value == 0) value = zoomStep; zoom += value; SndPlayer.playClick(); }
    public void zoomOut(float value = 0){ if (value == 0) value = zoomStep; zoom -= value; SndPlayer.playClick(); }
    public void zoomReset(float value = 0) { if (value == 0) value = resetZoom; zoom = value; SndPlayer.playClick();}
    //
    ///////////////////////////////////////////////////////////////////// External Calls

}