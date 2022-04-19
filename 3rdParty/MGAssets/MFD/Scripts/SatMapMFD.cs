using UnityEngine;
using UnityEngine.UI;


public class SatMapMFD : MonoBehaviour
{
    [Header("Main (Master-Slave)")]
    public bool isActive = true;
    public Camera mapCam;

    public enum Mode { Default = 0, Master = 1, Slave = 2 }
    [Space] public Mode mode = Mode.Default;

    public SatMapMFD masterParent;


    [Space]
    [Header("Map Configuration")]
    [Range(1, 10000)] public float zoom = 100;
    public float zoomStep = 100, zoomMin = 1, zoomMax = 2000;

    [Space]
    public Vector3 mapCamOffSet = new Vector3(0, 100, 0);
    public Vector3 mapCamAngle = new Vector3(90, 0, 0);

    [Space]
    [Header("Keys")]
    public bool useKeys = true;
    public bool overrideMaster = false;
    public KeyCode zoomInKey = KeyCode.Equals, zoomOutKey = KeyCode.Minus, zoomResetKey = KeyCode.Backspace;


    [Header("References")]

    public Transform aircraft;

    public RectTransform aircraftCenter;
    public RectTransform mapMainPanel;
    
    [Space]
    [Header("Strings Data")]
    public Text zoomTxt;


    //Internal Variables
    float currentZoom = -1, resetZoom = 500;



    //////////////////////////////////// Initialization
    void getAllRefs()
    {
        //Look for the Aircraft Reference on MainMFD
        if (mode == Mode.Default || mode == Mode.Master)
        {
            if (aircraft == null)
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
                SatMapMFD[] searchMasterParent = gameObject.GetComponentsInParent<SatMapMFD>(true);
                foreach (SatMapMFD satMap in searchMasterParent) if (satMap.mode == Mode.Master && satMap.isActive) { masterParent = satMap; break; }
            }

            if (aircraft == null && masterParent != null) aircraft = masterParent.aircraft;
        }

    }
    //
    void Awake()
    {
        if (mapMainPanel == null) mapMainPanel = GetComponent<RectTransform>();
        getAllRefs();
        resetZoom = zoom;

        //
        if (mode == Mode.Slave)
        {
            if (masterParent == null)
            {
                SatMapMFD[] searchMasterParent = gameObject.GetComponentsInParent<SatMapMFD>(true);
                foreach (SatMapMFD satMap in searchMasterParent) if (satMap.mode == Mode.Master && satMap.isActive) { masterParent = satMap; break; }
            }
        }
        //
    }
    //
    void OnEnable()
    {
        if (!isActive) return;

        //Look for the Aircraft or MasterParent References
        if (aircraft == null || (mode == Mode.Slave && masterParent == null)) getAllRefs();

        //Update Slave Initial Data from the Master
        if (mode == Mode.Slave) updateFromMaster();

        //Initial State
        if (mode != Mode.Slave || masterParent == null) resetZoom = zoom; else resetZoom = masterParent.zoom;
        if (mapCam != null) mapCam.orthographicSize = resetZoom;
        //

        if (mapCam != null) DisplayMsg.showAll("SAT Map Enabled", 5);
    }
    //////////////////////////////////// Initialization


    ///////////////////////////////////////////////////// Get Values from Master
    void updateFromMaster()
    {
        if (masterParent == null) { getAllRefs(); return; }

        zoom = masterParent.zoom;
        zoomStep = masterParent.zoomStep;
        zoomMin = masterParent.zoomMin;
        zoomMax = masterParent.zoomMax;
        mapCamOffSet = masterParent.mapCamOffSet;
        mapCamAngle = masterParent.mapCamAngle;
        aircraft = masterParent.aircraft;
        if(masterParent.mapCam != null) mapCam = masterParent.mapCam;
    }
    ///////////////////////////////////////////////////// Get Values from Master




    //////////////////////////////////////////////////////////////////////////////////// Update Map
    void Update()
    {
        if (!isActive || mapCam == null) return;
        if (aircraft == null) { getAllRefs(); return; }


        //Update Data from the Master script
        if (mode == Mode.Slave) updateFromMaster();

        //Move SatCam
        if (mode != Mode.Slave || masterParent == null)
        {
            mapCam.transform.position = new Vector3(aircraft.transform.position.x + mapCamOffSet.x, mapCamOffSet.y, aircraft.transform.position.z + mapCamOffSet.z);
            mapCam.transform.rotation = Quaternion.Euler(mapCamAngle);
        }
        //

        //Rotate Aircraft Center Image
        if (aircraftCenter != null) aircraftCenter.localRotation = Quaternion.Euler(0, 0, -MainMFD.current.heading);


        //Zoom Keys
        if (useKeys)
        {
            if (mode != Mode.Slave || overrideMaster)
            {
                if (Input.GetKeyDown(zoomInKey)) zoomIn();
                if (Input.GetKeyDown(zoomOutKey)) zoomOut();
                if (Input.GetKeyDown(zoomResetKey)) zoomReset();
            }
            if (masterParent != null)
            {
                if (Input.GetKeyDown(masterParent.zoomInKey)) zoomIn();
                if (Input.GetKeyDown(masterParent.zoomOutKey)) zoomOut();
                if (Input.GetKeyDown(masterParent.zoomResetKey)) zoomReset();
            }
        }
        //

        //Zoom Update
        if (currentZoom != zoom)
        {
            if (mode == Mode.Slave && masterParent != null) currentZoom = zoom;
            else
            {
                zoom -= zoom % zoomStep;
                zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
                currentZoom = zoom;
            }

            if (mapCam != null) mapCam.orthographicSize = zoom;
            if (zoomTxt != null) zoomTxt.text = currentZoom.ToString("000");// + "x";
        }
        //
    }
    //////////////////////////////////////////////////////////////////////////////////// Update Map



    ///////////////////////////////////////////////////////////////////// External Calls
    public void toogleMap()
    {
        if (mapMainPanel == null) return;

        SndPlayer.playClick();

        if (mapMainPanel.gameObject.activeSelf)
        {
            mapMainPanel.gameObject.SetActive(false);
            DisplayMsg.show("SAT Map Disabled", 5);
        }
        else
        {
            isActive = true;
            mapMainPanel.gameObject.SetActive(true);
            DisplayMsg.show("SAT Map Enabled", 5);
        }
    }
    //
    public void zoomIn(float value = 0)
    {
        if (value == 0) value = zoomStep;
        if (mode == Mode.Slave && masterParent != null) masterParent.zoom -= value; else zoom -= value;
        SndPlayer.playClick();
    }
    public void zoomOut(float value = 0)
    {
        if (value == 0) value = zoomStep;
        if (mode == Mode.Slave && masterParent != null) masterParent.zoom += value; else zoom += value;
        SndPlayer.playClick();
    }
    public void zoomReset(float value = 0)
    {
        if (value == 0) value = resetZoom;
        if (mode == Mode.Slave && masterParent != null) masterParent.zoom = masterParent.resetZoom; else zoom = value;
        SndPlayer.playClick();
    }
    //
    ///////////////////////////////////////////////////////////////////// External Calls


}