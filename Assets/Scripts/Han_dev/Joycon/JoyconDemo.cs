/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoyConLibrary
{
	public class JoyconDemo : MonoBehaviour
	{

		private List<Joycon> joycons;

		// Values made available via Unity
		public float[] stick;
		public Vector3 gyro;
		public Vector3 accel;
		public int jc_ind = 0;
		public Vector3 rot;
		public Vector3 dir;
		public Vector3 rotateOffset;
		private Vector3 centerVector;

		public float movingSpeed = 5;
		private float movingOffset = -45;

		public Rigidbody fishRigibody;
		public float pullForce = 2500;

		private Transform parentTransform;
		void Start()
		{
			gyro = new Vector3(0, 0, 0);
			accel = new Vector3(0, 0, 0);
			// get the public Joycon array attached to the JoyconManager in scene
			joycons = JoyconManager.Instance.j;

			parentTransform = gameObject.GetComponentInParent<Transform>();


			joycons[jc_ind].Recenter();
			centerVector = joycons[jc_ind].GetVector().eulerAngles;
		}

		// Update is called once per frame
		void Update()
		{
			// make sure the Joycon only gets checked if attached
			if (joycons.Count > 0)
			{
				Joycon j = joycons[jc_ind];
				// GetButtonDown checks if a button has been pressed (not held)
				if (j.GetButton(Joycon.Button.SHOULDER_2))
				{
					Debug.Log("Shoulder button 2 held");
					// GetStick returns a 2-element vector with x/y joystick components
					transform.Translate(Vector3.down * Time.deltaTime * movingSpeed, Space.World);
				}
				if (j.GetButton(Joycon.Button.SHOULDER_1))
				{
					Debug.Log("Shoulder button 1 held");
					transform.Translate(Vector3.up * Time.deltaTime * movingSpeed, Space.World);
				}

				if (j.GetButtonDown(Joycon.Button.DPAD_DOWN))
				{
					Debug.Log("Recenter");
					j.SetRumble(160, 320, 0.6f, 200);
					// The last argument (time) in SetRumble is optional. Call it with three arguments to turn it on without telling it when to turn off.
					// (Useful for dynamically changing rumble values.)
					// Then call SetRumble(0,0,0) when you want to turn it off.

					Debug.Log(string.Format("Stick x: {0:N} Stick y: {1:N}", j.GetStick()[0], j.GetStick()[1]));
					// Joycon has no magnetometer, so it cannot accurately determine its yaw value. Joycon.Recenter allows the user to reset the yaw value.
					j.Recenter();
					centerVector = j.GetVector().eulerAngles;
				}

				if (j.GetButtonDown(Joycon.Button.DPAD_RIGHT))
				{
					j.SetRumble(160, 320, 0.6f, 200);
					PullFish();
				}

				Move(j);

				// Gyro values: x, y, z axis values (in radians per second)
				gyro = j.GetGyro();

				// Accel values:  x, y, z axis values (in Gs)
				accel = j.GetAccel();

				rot = j.GetVector().eulerAngles;
				gameObject.transform.localEulerAngles = rot;

				rotateOffset = new Vector3((int)(rot.x - centerVector.x), (int)(rot.y - centerVector.y), (int)(rot.z - centerVector.z));
			}
		}
		private void Move(Joycon j)
		{
			stick = j.GetStick();
			if (stick[0] != 0 || stick[1] != 0)
			{
				dir = new Vector3(stick[0], 0, stick[1]);
				dir = Quaternion.AngleAxis(movingOffset, new Vector3(0, 1, 0)) * dir;
				parentTransform.Translate(dir * Time.deltaTime * movingSpeed, Space.World);
			}
		}
		public void PullFish()
		{
			fishRigibody.AddForce(0, pullForce * -1, 0);
			Debug.Log("PullFish");
		}
	}
}
*/