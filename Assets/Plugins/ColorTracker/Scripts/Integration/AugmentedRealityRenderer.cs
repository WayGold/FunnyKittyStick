using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The Vexpot.Integration namespace contains all scripts used to achieve an easy integration
/// with the Unity 3D editor.
/// </summary>
namespace Vexpot.Integration
{
    /// <summary>
    /// An utility class to render objects in augmented reality. 
    /// </summary>
    [AddComponentMenu("Vexpot/AugmentedRealityRenderer")]
    public class AugmentedRealityRenderer : MonoBehaviour
    {
        /// <summary>
        /// Camera used to render the 3D scene. 
        /// </summary>
        public Camera perspectiveCamera;
        /// <summary>
        /// The reference model to be copied when targets are found. 
        /// This object can be a prefab or any object on the scene.
        /// </summary>
        public GameObject objectModel;
        /// <summary>
        /// A reference to the GameObject containing the tracker panel.
        /// </summary>
        public ColorTrackerPanel colorTrackerPanel;

        /// <summary>
        /// 
        /// </summary>
        protected float _scaleRatio = 0.90f;
        /// <summary>
        /// 
        /// </summary>
        protected List<GameObject> _targets  = new List<GameObject>();
        /// <summary>
        /// 
        /// </summary>
        protected Vector3 _reusablePosition = new Vector3();

        /// <summary>
        /// Create objects to be displayed in augmented reality.
        /// </summary>
        /// <param name="count">Amount of objects to create.</param>
        /// <returns></returns>
        protected bool CreateObjectsIfNeeded(int count)
        {
            if (count != _targets.Count)
            {
                for (var i = 0; i < _targets.Count; i++)
                {
                    Destroy(_targets[i]);
                }

                _targets.Clear();

                for (var i = 0; i < count; i++)
                {
                    GameObject obj = Instantiate(objectModel) as GameObject;
                    obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    obj.SetActive(false);
                    _targets.Add(obj);
                }

                return true;
            }
            else
                return false;
        }

        /// <summary>
        ///  Request new tracker results when new frame is available.
        /// </summary>
        public virtual void OnPostRender()
        {
            if (colorTrackerPanel != null && colorTrackerPanel.GetColorTracker() != null)
            {
                ColorTracker tracker = colorTrackerPanel.GetColorTracker();
                if (tracker == null) return;

                List<TrackerResult> colorTrackerResults = tracker.Compute();

                CreateObjectsIfNeeded(colorTrackerResults.Count);

                for (var t = 0; t < colorTrackerResults.Count; t++)
                {
                    TrackerResult r = colorTrackerResults[t];

                    if (r.state == TrackingState.Tracked)
                    {
                        CoordinateMapper.ConvertInputToScreen(tracker.input, r.center, ref _reusablePosition);
                        _reusablePosition.z = perspectiveCamera.nearClipPlane + _scaleRatio - Mathf.Clamp(_reusablePosition.z, 0.0f, 0.8f);
                                              
                        _targets[t].transform.position = perspectiveCamera.ScreenToWorldPoint(_reusablePosition);
                        _targets[t].SetActive(true);
                    }
                    else
                    {
                        _targets[t].SetActive(false);
                    }
                                     
                }
            }
           
        }
    }
}
