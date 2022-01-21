using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementOutputs {
	struct KinematicSteeringOutput {
		public Vector3 linearVelocity;
		public float rotVelocity;
	};
	struct DynamicSteeringOutput {
		public Vector3 linearAccel;
		public float rotAccel;
	};
}
