using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class StickTrackerWiimote : MonoBehaviour
{
    private Wiimote wiimote;
    public Vector3 wmpOffset = Vector3.zero;

    [SerializeField]
    private GameObject stickObject;

    private float resetOffsetHeartbeatTimeInSeconds = 0.2f;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (WiimoteManager.HasWiimote() == false)
        {
            yield return null;
        }

        this.wiimote = WiimoteManager.Wiimotes[0];

        //Set up gyroscope
        wiimote.SendDataReportMode(InputDataType.REPORT_EXT21);

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

    private Vector3 GetAccelVector()
    {
        float accel_x;
        float accel_y;
        float accel_z;

        float[] accel = this.wiimote.Accel.GetCalibratedAccelData();
        accel_x = accel[0];
        accel_y = -accel[2];
        accel_z = -accel[1];

        //Debug.LogError("X: " + accel_x + " Y: " + accel_y + " Z: " + accel_z);

        return new Vector3(accel_x, accel_y, accel_z).normalized;
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

            if (ret > 0 && wiimote.current_ext == ExtensionController.MOTIONPLUS)
            {
                Vector3 offset = new Vector3(wiimote.MotionPlus.PitchSpeed,
                                                -wiimote.MotionPlus.YawSpeed,
                                                wiimote.MotionPlus.RollSpeed) / 95f; // Divide by 95Hz (average updates per second from wiimote)
                this.wmpOffset += offset;

                this.stickObject.transform.Rotate(this.wmpOffset, Space.Self);
            }
        } while (ret > 0);
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
