using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovementOutputs;

namespace AIService
{
    public static class KinematicSeek
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
    }

    public class DynamicArrive
    {
        public Rigidbody characterRB;
        public Rigidbody targetRB;

        public float maxAcceleration;
        public float maxSpeed;

        public float targetRadius;
        public float slowRadius;
        public float timeToTarget;

        public DynamicArrive(Rigidbody i_character, Rigidbody i_target, float i_maxAcceleration,
        float i_maxSpeed, float i_targetRadius, float i_slowRadius, float i_timeToTarget = 0.1f)
        {
            characterRB = i_character;
            targetRB = i_target;
            maxAcceleration = i_maxAcceleration;
            maxSpeed = i_maxSpeed;
            targetRadius = i_targetRadius;
            slowRadius = i_slowRadius;
            timeToTarget = i_timeToTarget;
        }

        public DynamicSteeringOutput getSteering()
        {
            DynamicSteeringOutput result;
            Vector3 targetVelocity;
            float targetSpeed;

            // Get the direction to the target.
            Vector3 direction = targetRB.position - characterRB.position;
            float distance = Vector3.Distance(targetRB.position, characterRB.position);

            // Check if we are there, return no steering.
            if (distance < targetRadius)
            {
                result.linearAccel = new Vector3(0, 0, 0);
                result.rotAccel = 0;
                return result;
            }

            // If we are outside the slowRadius, then move at max speed.
            if (distance > slowRadius)
            {
                targetSpeed = maxSpeed;
            }
            // Otherwise calculate a scaled speed.
            else
            {
                targetSpeed = maxSpeed * distance / slowRadius;
            }

            // The target velocity combines speed and direction.
            targetVelocity = Vector3.Normalize(direction);
            targetVelocity *= targetSpeed;

            // Acceleration tries to get to the target velocity.
            result.linearAccel = targetVelocity - characterRB.velocity;
            result.linearAccel /= timeToTarget;

            // Check if the acceleration is too fast.
            if (Vector3.Magnitude(result.linearAccel) > maxAcceleration)
            {
                result.linearAccel = Vector3.Normalize(result.linearAccel) * maxAcceleration;
            }

            result.rotAccel = 0;
            return result;
        }
    }

    public class DynamicFace
    {
        public DynamicAlign dynamicAlign;
        public Rigidbody characterRB;
        public Rigidbody targetRB;
        public Rigidbody explicitTarget;

        public float maxAngularAcceleration;
        public float maxRotation;

        public float targetRadius;
        public float slowRadius;
        public float timeToTarget;

        public DynamicFace(Rigidbody i_character, Rigidbody i_target, Rigidbody i_explicitTarget, float i_maxAngularAcceleration,
        float i_maxRotation, float i_targetRadius, float i_slowRadius, float i_timeToTarget = 0.1f)
        {
            explicitTarget = i_explicitTarget;
            characterRB = i_character;
            targetRB = i_target;

            maxAngularAcceleration = i_maxAngularAcceleration;
            maxRotation = i_maxRotation;
            targetRadius = i_targetRadius;
            slowRadius = i_slowRadius;
            timeToTarget = i_timeToTarget;
        }

        public DynamicSteeringOutput getSteering()
        {
            DynamicSteeringOutput result;
            Vector3 direction;

            // 1. Calculate the target to delegate to align
            //  Work out the direction to target.
            direction = targetRB.position - characterRB.position;

            // Check for a zero direction, and make no change if so.
            if (Vector3.Magnitude(direction) < 0.001)
            {
                result.linearAccel = new Vector3(0, 0, 0);
                result.rotAccel = 0.0f;
                return result;
            }

            // Put the target together.
            Quaternion deltaRot = Quaternion.Euler(new Vector3(0, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0));
            explicitTarget.rotation = deltaRot;

            // 2. Delegate to align.
            dynamicAlign = new DynamicAlign(characterRB, explicitTarget, maxAngularAcceleration,
                                            maxRotation, targetRadius, slowRadius);
            result = dynamicAlign.getSteering();

            return result;
        }
    }

    public class DynamicAlign
    {
        public Rigidbody characterRB;
        public Rigidbody targetRB;

        public float maxAngularAcceleration;
        public float maxRotation;

        public float targetRadius;
        public float slowRadius;
        public float timeToTarget;

        public DynamicAlign(Rigidbody i_character, Rigidbody i_target, float i_maxAngularAcceleration,
        float i_maxRotation, float i_targetRadius = 0.01f, float i_slowRadius = 0.5f, float i_timeToTarget = 0.1f)
        {
            characterRB = i_character;
            targetRB = i_target;
            maxAngularAcceleration = i_maxAngularAcceleration;
            maxRotation = i_maxRotation;
            targetRadius = i_targetRadius;
            slowRadius = i_slowRadius;
            timeToTarget = i_timeToTarget;
        }

        public DynamicSteeringOutput getSteering()
        {
            DynamicSteeringOutput result;
            float rotation, rotationSize, targetRotation, angularAcceleration;

            Vector3 targetForward = targetRB.gameObject.transform.forward;
            Vector3 characterForward = characterRB.gameObject.transform.forward;

            // Get the naive direction to the target.
            rotation = Vector3.SignedAngle(new Vector3(targetForward.x, 0, targetForward.z),
                                            new Vector3(characterForward.x, 0, characterForward.z), new Vector3(0, 1, 0));
            rotation = Mathf.Deg2Rad * rotation;

            // Map the result to the (-pi, pi) interval.
            rotation = mapToRange(rotation);
            rotationSize = Mathf.Abs(rotation);

            // Check if we are there, return no steering.
            if (rotationSize < targetRadius)
            {
                result.linearAccel = new Vector3(0, 0, 0);
                result.rotAccel = 0;
                return result;
            }

            // If we are outside the slowRadius, then use maximum rotation.
            if (rotationSize > slowRadius)
            {
                targetRotation = maxRotation;
            }
            // Otherwise calculate a scaled rotation.
            else
            {
                targetRotation = maxRotation * rotationSize / slowRadius;
            }

            // The final target rotation combines speed (already in the variable) and direction.
            targetRotation *= rotation / rotationSize;

            // Acceleration tries to get to the target rotation.
            result.rotAccel = (targetRotation - characterRB.angularVelocity.y) / timeToTarget;

            // Check if the acceleration is too great. Clamp to maxAngularAccel if too great.
            angularAcceleration = Mathf.Abs(result.rotAccel);
            if (angularAcceleration > maxAngularAcceleration)
            {
                result.rotAccel /= angularAcceleration;
                result.rotAccel *= maxAngularAcceleration;
            }

            result.linearAccel = new Vector3(0, 0, 0);

            return result;
        }

        public float mapToRange(float i_rotation)
        {
            float pi = Mathf.PI;

            while (i_rotation > pi)
            {
                i_rotation -= 2 * pi;
            }

            while (i_rotation < -pi)
            {
                i_rotation += 2 * pi;
            }

            return i_rotation;
        }
    }

    public class DynamicLWYAG
    {
        public DynamicAlign dynamicAlign;
        public Rigidbody characterRB;
        public Rigidbody explicitTarget;

        public float maxAngularAcceleration;
        public float maxRotation;

        public float targetRadius;
        public float slowRadius;
        public float timeToTarget;

        public DynamicLWYAG(Rigidbody i_character, Rigidbody i_explicitTarget, float i_maxAngularAcceleration,
        float i_maxRotation, float i_targetRadius, float i_slowRadius, float i_timeToTarget = 0.1f)
        {
            explicitTarget = i_explicitTarget;
            characterRB = i_character;

            maxAngularAcceleration = i_maxAngularAcceleration;
            maxRotation = i_maxRotation;
            targetRadius = i_targetRadius;
            slowRadius = i_slowRadius;
            timeToTarget = i_timeToTarget;
        }

        public DynamicSteeringOutput getSteering()
        {
            DynamicSteeringOutput result;
            Vector3 velocity;

            // 1. Calculate the target to delegate to align
            // Check for a zero direction, and make no change if so.
            velocity = characterRB.velocity;
            if (Vector3.Magnitude(velocity) < 0.001)
            {
                result.linearAccel = new Vector3(0.0f, 0.0f, 0.0f);
                result.rotAccel = 0.0f;
                return result;
            }

            // Otherwise set the target based on the velocity.
            Quaternion deltaRot = Quaternion.Euler(new Vector3(0, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 0));
            explicitTarget.MoveRotation(deltaRot);

            // 2. Delegate to align.
            dynamicAlign = new DynamicAlign(characterRB, explicitTarget, maxAngularAcceleration, maxRotation);
            result = dynamicAlign.getSteering();

            return result;
        }
    }

    public static class ProjectionThrow
    {
        public static float G = 9.81f;

        public static Vector3 CaculateThrowVelocity(GameObject i_cat, Vector3 i_targetPos, float i_addHeight)
        {
            //if you need catSize offset, use this!
            Vector3 catSize = i_cat.GetComponent<Collider>().bounds.size;
            float h = i_targetPos.y - (i_cat.transform.position.y - catSize.y / 2) + i_addHeight;

            //if you don't need catSize offset, use this!
            //float h = i_targetPos.y - i_cat.transform.position.y + i_addHeight;

            float s = Vector2.Distance(new Vector2(i_targetPos.x, i_targetPos.z),
                                        new Vector2(i_cat.transform.position.x, i_cat.transform.position.z));

            return (h >= 0) ? UpThrow(i_cat.transform.position, i_targetPos, h, s) : DownThrow(i_cat, i_targetPos, h, s);
        }

        public static Vector3 UpThrow(Vector3 i_catPos, Vector3 i_targetPos, float h, float s)
        {
            float v0 = Mathf.Sqrt(2 * G * h);
            float v1 = s / (Mathf.Sqrt((2 / G) * v0 - 2 * h / G));

            //horizontal velocity
            Vector3 vs = new Vector3(i_targetPos.x - i_catPos.x,
                                        0,
                                        i_targetPos.z - i_catPos.z).normalized * v1;
            //vertical velocity
            Vector3 vh = new Vector3(0, i_targetPos.y - i_catPos.y, 0).normalized * v0;

            Debug.Log("UpThrow:" + "vh= " + vh + " vs=" + vs);
            return vh + vs;
        }

        public static Vector3 DownThrow(GameObject i_cat, Vector3 i_targetPos, float h, float s)
        {
            float v0 = 0;
            float v1 = s / (Mathf.Sqrt((-2 * h) / G));

            //horizontal velocity
            Vector3 vs = new Vector3(i_targetPos.x - i_cat.transform.position.x,
                                      0,
                                      i_targetPos.z - i_cat.transform.position.z).normalized * v1;
            //vertical velocity
            Vector3 vh = new Vector3(0, i_targetPos.y - i_cat.transform.position.y, 0).normalized * v0;

            Debug.Log("DownThrow:" + "vh = " + vh + " vs=" + vs);
            return vs;
        }
    }
}

