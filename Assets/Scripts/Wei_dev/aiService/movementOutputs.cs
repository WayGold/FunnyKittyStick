using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementOutputs {
	public struct KinematicSteeringOutput {
		public Vector3 linearVelocity;
		public float rotVelocity;
	};
	public struct DynamicSteeringOutput {
		public Vector3 linearAccel;
		public float rotAccel;
	};
}
