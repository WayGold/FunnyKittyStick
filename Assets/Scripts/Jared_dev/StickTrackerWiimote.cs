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
    private float maxXOffset = 30f;
    private float maxYOffset = 20f;
    private float maxZOffset = 17f;

    //private float maxXOffset = 1f;
    //private float maxYOffset = 1f;
    //private float maxZOffset = 1f;

    private float minDotDistance = 0.15f;
    private float maxDotDistance = 0.65f;
    private float sensitivity = 1.0f;


    // Start is called before the first frame update
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
        wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_IR10_EXT9);

        this.wiimote.RequestIdentifyWiiMotionPlus();
        wiimote.ReadWiimoteData();
        
        while (this.wiimote.wmp_attached == false)
        {
            Debug.LogError("Initializing WMP...");
            yield return null;
            this.wiimote.RequestIdentifyWiiMotionPlus();
            wiimote.ReadWiimoteData();
        }
        

        this.wiimote.ActivateWiiMotionPlus();
        yield return new WaitForSeconds(2.0f);
        MotionPlusData wmpData = this.wiimote.MotionPlus;
        wmpData.SetZeroValues();

        stickObject.transform.SetPositionAndRotation(this.stickObject.transform.position, Quaternion.Euler(new Vector3(0f, -45f, 0f)));

        //stickObject.transform.rotation = Quaternion.FromToRotation(stickObject.transform.rotation.eulerAngles, Vector3.up) * stickObject.transform.rotation;
        //stickObject.transform.rotation = Quaternion.FromToRotation(stickObject.transform.forward, Vector3.forward) * stickObject.transform.rotation;

        StartCoroutine(ResetOffsetHeartbeat());
    }

    // Update is called once per frame
    void Update()
    {
        int ret;
        do
        {
            if (this.wiimote == null)
            {
                return;
            }

            this.wiimote = WiimoteManager.Wiimotes[0];

            ret = this.wiimote.ReadWiimoteData();

            if (wiimote.current_ext == ExtensionController.MOTIONPLUS && wiimote.Button.b)
            {
                Vector3 offset = new Vector3(wiimote.MotionPlus.PitchSpeed,
                                                -wiimote.MotionPlus.YawSpeed,
                                                wiimote.MotionPlus.RollSpeed) / 95f; // Divide by 95Hz (average updates per second from wiimote)
                this.wmpOffset += offset;

                this.stickObject.transform.Rotate(this.wmpOffset, Space.Self);
            }
            else
            {
                Vector3 offsetVector = Vector3.zero;

                //For Tracking X and Y positioning
                float[] pointer = wiimote.Ir.GetPointingPosition();

                float xOffset = pointer[0] * this.sensitivity * this.maxXOffset;
                offsetVector = new Vector3(offsetVector.x - xOffset, offsetVector.y, offsetVector.z - xOffset);

                float yOffset = pointer[1] * this.sensitivity * this.maxYOffset;
                offsetVector = new Vector3(offsetVector.x, offsetVector.y - yOffset, offsetVector.z);

                //For tracking Z positioning
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

                float dotDistance = Vector2.Distance(sensorDots[0], sensorDots[1]);
                float zOffset = 0.0f;

                if (dotDistance > 0.0f)
                {
                    zOffset = (dotDistance / this.maxDotDistance) * this.sensitivity * this.maxZOffset;

                    offsetVector = new Vector3(offsetVector.x - zOffset, offsetVector.y, offsetVector.z + zOffset);

                    this.stickHolder.transform.position = this.originalStickPosition + offsetVector;

                    //this.stickHolder.transform.position = Vector3.Lerp(this.stickHolder.transform.position,
                       // this.stickHolder.transform.position + offsetVector, this.sensitivity);
                }
            }      
        } while (ret > 0);
    }

    private void SetXYPosition()
    { 
    
    }

    private IEnumerator ResetOffsetHeartbeat()
    {
        while (true)
        {
            this.wmpOffset = Vector3.zero;
            yield return null;
        }
    }
}
