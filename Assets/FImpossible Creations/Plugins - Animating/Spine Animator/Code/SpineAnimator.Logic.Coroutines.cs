using System.Collections;
using UnityEngine;

namespace FIMSpace.FSpine
{
    public partial class FSpineAnimator
    {
        /// <summary> Helper counter for start after t-pose feature </summary>
        int initAfterTPoseCounter = 0;

        /// <summary>
        /// Support for 'animate physics' option inside unity's Animator
        /// </summary>
        //IEnumerator AnimatePhysicsClock()
        //{
        //    animatePhysicsWorking = true;
        //    while (true)
        //    {
        //        yield return new WaitForFixedUpdate();
        //        triggerAnimatePhysics = true;
        //    }
        //}

        //bool animatePhysicsWorking = false;
        //bool triggerAnimatePhysics = false;

    }
}