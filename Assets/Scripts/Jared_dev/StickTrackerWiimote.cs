using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class StickTrackerWiimote : MonoBehaviour
{
    private Wiimote wiimote;
    public Vector3 wmpOffset = Vector3.zero;

    [SerializeField]
    private GameObject stickHolder;
    [SerializeField]
    private GameObject stickObject;

    private float resetOffsetHeartbeatTimeInSeconds = 0.2f;

    private Vector3 originalStickPosition;
    private Vector3 offsetVector = Vector3.zero;
    private Vector3 prevOffsetVector = Vector3.zero;
    private Vector2 prevPointerValues = Vector2.zero;
    private float xBounds = 8f;
    private float yBounds = 7f;

    private Vector3 rotationEulers = Vector3.zero;
    private float maxXRotation = 20f;
    private float maxYRotation = 20f;

    private float maxYOffset = 20f;
    private float maxZOffset = 12f;
    private float shakeBuffer = 0.2f;
    private float shakeOffset = 0.5f;

    //private float maxXOffset = 1f;
    //private float maxYOffset = 1f;
    //private float maxZOffset = 1f;

    private float minDotDistance = 0.15f;
    private float maxDotDistance = 0.25f;
    private float sensitivity = 0.5f;

    private float dotDistance = 0.0f;

    public float rayDistance = 70f;

    private bool gyroMode = false;

    // Start is called before the first frame update

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    IEnumerator Start()
    {
        this.originalStickPosition = stickHolder.transform.position;

        WiimoteManager.FindWiimotes();

        while (WiimoteManager.HasWiimote() == false)
        {
            yield return null;
        }

        WiimoteManager.Cleanup(WiimoteManager.Wiimotes[0]);

        WiimoteManager.FindWiimotes();

        while (WiimoteManager.HasWiimote() == false)
        {
            yield return null;
        }

        this.wiimote = WiimoteManager.Wiimotes[0];

        //Set up gyroscope
        this.wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);

        this.wiimote.RequestIdentifyWiiMotionPlus();
        wiimote.ReadWiimoteData();

        while (this.wiimote.wmp_attached == false)
        {
            Debug.LogError("Initializing WMP...");
            yield return null;
            this.wiimote = WiimoteManager.Wiimotes[0];
            this.wiimote.RequestIdentifyWiiMotionPlus();
            wiimote.ReadWiimoteData();
        }
        

        this.wiimote.ActivateWiiMotionPlus();
        yield return new WaitForSeconds(1.0f);
        MotionPlusData wmpData = this.wiimote.MotionPlus;
        wmpData.SetZeroValues();
        
        stickObject.transform.SetPositionAndRotation(this.stickObject.transform.position, Quaternion.Euler(new Vector3(0f, -45f, 0f)));

        //stickObject.transform.rotation = Quaternion.FromToRotation(stickObject.transform.rotation.eulerAngles, Vector3.up) * stickObject.transform.rotation;
        //stickObject.transform.rotation = Quaternion.FromToRotation(stickObject.transform.forward, Vector3.forward) * stickObject.transform.rotation;

        this.wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_IR12);
        this.wiimote.SetupIRCamera(IRDataType.EXTENDED);

        StartCoroutine(ResetOffsetHeartbeat());
        StartCoroutine(TrackStick());
    }

    // Update is called once per frame
    void Update()
    {
        if (!WiimoteManager.HasWiimote() || this.wiimote.wmp_attached == false)
        {
            return;
        }
        
        int ret;
        do
        {
            this.wiimote = WiimoteManager.Wiimotes[0];

            ret = this.wiimote.ReadWiimoteData();

            if (wiimote.current_ext == ExtensionController.MOTIONPLUS && wiimote.Button.b)
            {
                if (this.gyroMode == false)
                {
                    this.gyroMode = true;
                    this.wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
                }

                //Debug.LogError("TADA: " + wiimote.MotionPlus.PitchSpeed + " " + wiimote.MotionPlus.YawSpeed + " " + wiimote.MotionPlus.RollSpeed);

                Vector3 offset = new Vector3(wiimote.MotionPlus.PitchSpeed * this.sensitivity,
                                                -wiimote.MotionPlus.YawSpeed * this.sensitivity,
                                                wiimote.MotionPlus.RollSpeed * this.sensitivity) / 95f; // Divide by 95Hz (average updates per second from wiimote)
                this.wmpOffset += offset;

                this.stickObject.transform.Rotate(this.wmpOffset, Space.Self);
            }
            else
            {
                if (this.gyroMode == true)
                {
                    this.gyroMode = false;
                    this.wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_IR12);
                    this.wiimote.SetupIRCamera(IRDataType.EXTENDED);
                }
            }      
        } while (ret > 0);
    }

    private void SetXYZPosition()
    {
        this.prevOffsetVector = this.offsetVector;
        
        float[] pointer = this.wiimote.Ir.GetPointingPosition();

        //Debug.LogError("IR Pointing Position: " + pointer[0]);

        //X Position
        //This ain't perfect, but it's what I got
        //Maps 0 - 1 to -1 - 1
        float sanitizedValue = pointer[0];
        if (sanitizedValue > 1.0f)
        {
            sanitizedValue = 1.0f;
        }
        else if (sanitizedValue < 0.0f)
        {
            sanitizedValue = 0.0f;
        }

        float xPointer = (sanitizedValue * 2) - 1.0f;

        float xOffset = xPointer * this.xBounds;
        if (Mathf.Abs(xOffset - this.offsetVector.x) < this.shakeBuffer)
        {
            xOffset = this.offsetVector.x;
        }

        float xRotation = xPointer * this.maxXRotation;
        
        //Y Position
        sanitizedValue = pointer[1];
        if (sanitizedValue > 1.0f)
        {
            sanitizedValue = 1.0f;
        }
        else if (sanitizedValue < 0.0f)
        {
            sanitizedValue = 0.0f;
        }

        float yPointer = (sanitizedValue * 2) - 1.0f;

        float yOffset = yPointer * this.yBounds;
        if (Mathf.Abs(yOffset - this.offsetVector.y) < this.shakeBuffer)
        {
            yOffset = this.offsetVector.y;
        }

        float yRotation = yPointer * this.maxYRotation;

        //Z Position
        Vector2[] sensorDots = new Vector2[2];
        float[,] irData = wiimote.Ir.GetProbableSensorBarIR();
        for (int i = 0; i < 2; i++)
        {
            float normalizedX = (float)irData[i, 0] / 1023f;
            float normalizedY = (float)irData[i, 1] / 767f;

            if (normalizedX != -1 && normalizedY != -1)
            {
                sensorDots[i] = new Vector2(normalizedX, normalizedY);
            }
        }

        this.dotDistance = Vector2.Distance(sensorDots[0], sensorDots[1]);
        float zOffset = 0.0f;

        if (this.dotDistance > 0.0f)
        {
            zOffset = ((((this.dotDistance / this.maxDotDistance) * 2) - 1) * 2) * this.maxZOffset;
            if (Mathf.Abs(zOffset - this.offsetVector.z) < this.shakeBuffer)
            {
                zOffset = this.offsetVector.z;
            }

            //offsetVector = new Vector3(offsetVector.x - zOffset, offsetVector.y, offsetVector.z + zOffset);
        }

        if ((pointer[0] > 0.0f && pointer[0] < 1.0f) && (pointer[1] > 0.0f && pointer[1] < 1.0f))
        {
            this.offsetVector = new Vector3(xOffset - zOffset, yOffset, xOffset + zOffset);
            this.rotationEulers = new Vector3(yRotation, xRotation, 0.0f);
        }

        this.prevPointerValues = new Vector2(pointer[0], pointer[1]);
    }

    private void CenterToCamera()
    {
        Ray cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        this.stickHolder.transform.position = cameraRay.GetPoint(this.rayDistance);
    }

    private IEnumerator ResetOffsetHeartbeat()
    {
        while (true)
        {
            this.wmpOffset = Vector3.zero;
            yield return null;
        }
    }

    private IEnumerator TrackStick()
    {
        while (!WiimoteManager.HasWiimote()) { yield return null; }

        while (true)
        {
            this.CenterToCamera();
          
            if (wiimote.Button.b == false)
            {
                this.SetXYZPosition();             
            }

            Debug.LogError("MagDiff: " + (this.offsetVector - this.prevOffsetVector).magnitude);

            if (Mathf.Abs(this.offsetVector.x - this.prevOffsetVector.x) > this.shakeOffset && 
                Mathf.Abs(this.offsetVector.y - this.prevOffsetVector.y) > this.shakeOffset &&
                Mathf.Abs(this.offsetVector.z - this.prevOffsetVector.z) > this.shakeOffset)
            {
                this.stickHolder.transform.position += this.offsetVector;
            }
            else
            {
                this.stickHolder.transform.position += Vector3.Lerp(this.prevOffsetVector, this.offsetVector, 0.1f);
            }
            //this.stickObject.transform.SetPositionAndRotation(this.stickHolder.transform.position, Quaternion.Euler(this.rotationEulers));
            //Debug.LogError("Rotation Eulers: " + this.rotationEulers);
            yield return null;
        }
    }

    private void OnApplicationQuit()
    {
        this.wiimote = null;
        //WiimoteManager.Cleanup(WiimoteManager.Wiimotes[0]);
    }
}
