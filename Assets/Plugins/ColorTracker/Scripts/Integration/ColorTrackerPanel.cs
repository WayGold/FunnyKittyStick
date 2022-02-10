using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The Vexpot.Integration namespace contains all scripts used to achieve an easy integration
/// with the Unity 3D editor.
/// </summary>
namespace Vexpot.Integration
{
    /// <summary>
    /// Input modes used by <see cref="ColorTrackerPanel"/>.
    /// </summary>
    public enum InputType
    {
        /// <summary>
        /// <see cref="ColorTracker"/> will use an instance of <see cref="ScreenInput"/> as input source. 
        /// </summary>
        Screen,
        /// <summary>
        /// <see cref="ColorTracker"/> will use an instance of <see cref="WebCamInput"/> as input source. 
        /// </summary>
        WebCam,

        /// <summary>
        /// 
        /// </summary>
        ScreenWithRoi
    }

    /// <summary>
    /// Defines the graphical user interface to setup the color tracker.
    /// Shows how to integrate core modules with Unity Editor in a very easy way.
    /// </summary>
    [AddComponentMenu("Vexpot/ColorTrackerPanel")]
    public class ColorTrackerPanel : MonoBehaviour, ITrackerEventListener
    {
        /// <summary>
        /// Input mode. You can implement your own input mode implementing the <see cref="IInputSource"/> interface. 
        /// </summary>
        public InputType inputType = InputType.Screen;
        /// <summary>
        /// Defines the tracker accuracy mode.
        /// </summary>
        public TrackerAccuracy accuracy = TrackerAccuracy.Fast;
        /// <summary>
        /// Enables or disables the use of Kalman filter.
        /// </summary>
        public bool useKalmanFilter = true;
        /// <summary>
        /// Auto-start the tracker on <see cref="Awake"/> 
        /// </summary>
        public bool playOnAwake = true;
        /// <summary>
        /// The list of color targets to be used.
        /// </summary>
        public List<ColorTarget> colorTargets = new List<ColorTarget>();

        /// <summary>
        /// 
        /// </summary>
        public bool enableColorMap = false;

        /// <summary>
        /// 
        /// </summary>
        public bool enableColorTrack = true;

        /// <summary>
        /// 
        /// </summary>
        public int colorMapPointSpacing = 1;

        private ColorTracker _tracker;
        private IInputSource _selectedInput;

        void Awake()
        {
            switch (inputType)
            {
                case InputType.WebCam: _selectedInput = new WebCamInput(); break;
                case InputType.Screen: _selectedInput = new ScreenInput(); break;                
            }
          
            _tracker = new ColorTracker(_selectedInput);
            _tracker.colorTargets = colorTargets;
            _tracker.accuracy = accuracy;
            _tracker.listener = this;

            if (playOnAwake)
                _tracker.Start();
         
        }

        void OnDestroy()
        {
            if (_tracker)
                _tracker.Stop();

            if (_selectedInput != null)
                _selectedInput.Close();
        }

        /// <summary>
        /// Gets the <see cref="ColorTracker"/> instance attached to this object.
        /// </summary>
        /// <returns></returns>
        public ColorTracker GetColorTracker()
        {
            return _tracker;
        }

        /// <summary>
        /// Starts the internal <see cref="ColorTracker"/> instance.
        /// </summary>
        public void StartColorTracker()
        {
            if (_tracker)
                _tracker.Start();
        }

        /// <summary>
        /// Stops the internal <see cref="ColorTracker"/> instance.
        /// </summary>
        public void StopColorTracker()
        {
            if (_tracker)
                _tracker.Stop();
        }

        /// <summary>
        /// Updates the internal <see cref="ColorTracker"/> target list.
        /// </summary>
        public void UpdateColorTargets()
        {
            if (_tracker)
                _tracker.colorTargets = colorTargets;
        }

        /// <summary>
        /// Toggles the use of Kalman filter in runtime.
        /// </summary>
        void Update()
        {
            if (_tracker)
            {
               _tracker.enableColorTrack = enableColorTrack;
               _tracker.useKalmanFilter = useKalmanFilter;          
               _tracker.enableColorMap = enableColorMap;
               _tracker.colorMapPointSpacing = colorMapPointSpacing;               
            }
        }

        /// <summary>
        /// Implements <see cref="ITrackerEventListener.TrackerDidStart(ColorTracker)"/> interface.
        /// </summary>
        /// <param name="tracker"></param>
        public void TrackerDidStart(ColorTracker tracker)
        {
            // do something here 
        }

        /// <summary>
        /// Implements <see cref="ITrackerEventListener.TrackerDidStop(ColorTracker)"/> interface.
        /// </summary>
        public void TrackerDidStop(ColorTracker tracker)
        {
            // do something here 
        }

        /// <summary>
        /// Implements <see cref="ITrackerEventListener.WhenNewResultAvailable(ColorTracker)"/> interface.
        /// </summary>
        public void WhenNewResultAvailable(ColorTracker tracker)
        {
            // do something here 
        }
    }
}
