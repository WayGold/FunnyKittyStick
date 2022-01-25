using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovementOutputs;

namespace AIService
{
    public static class aiService
    {

        public static KinematicSteeringOutput kinematicSeek(Rigidbody characterRB, Rigidbody targetRB,
                                                                float maxSpeed)
        {
            KinematicSteeringOutput retVal;
            float orientation;

            Vector3 seekVec = targetRB.position - characterRB.position;
            seekVec = Vector3.Normalize(seekVec);
            seekVec *= maxSpeed;

            // Ignore rotation on y axis
            orientation = Mathf.Atan2(seekVec.x, seekVec.z);

            retVal.linearVelocity = seekVec;
            retVal.rotVelocity = orientation;

            return retVal;
        }

        // FIXME
        public static DynamicSteeringOutput wander()
        {
            DynamicSteeringOutput retVal;

            retVal.linearAccel = new Vector3(0, 0, 0);
            retVal.rotAccel = 0;

            return retVal;
        }
    }
}

