using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The Vexpot namespace contains all core functionalities used in the library.
/// </summary>
namespace Vexpot
{
    /// <summary>
    /// Defines the color tracks state. When the state changes to <see cref="Lost"/> the data contained in 
    /// the result is not longer updated until it becomes <see cref="Tracked"/>.
    /// </summary>
    public enum TrackingState
    {
        /// <summary>
        /// Default tracking state. 
        /// </summary>
        Unknown,
        /// <summary>
        /// There is no <see cref="TrackerResult"/> data available.
        /// </summary>
        Lost,
        /// <summary>
        /// Data is available. Since the <see cref="TrackerResult"/> is tracked, confidence in the data is very high.
        /// </summary>
        Tracked
    }

    /// <summary>
    /// Defines the tracker accuracy level. <see cref="Optimal"/> will deliver more steady and accurate results 
    /// but is more computationally expensive.
    /// </summary>
    public enum TrackerAccuracy
    {
        /// <summary>
        /// The computing process will be the fastest but not accurate.
        /// </summary>
        Fast,
        /// <summary>
        /// The tracker will estimate result with acceptable level of accuracy and also good performance.
        /// </summary>
        Balanced,
        /// <summary>
        /// The tracker will be more accurate but performance could be affected if many color targets are added.
        /// </summary>
        Optimal
    }

    /// <summary>
    /// Defines all attributes for tracking results.
    /// </summary>
    public class TrackerResult
    {
        /// <summary>
        /// Unique identifier assigned to this result. The id is persistent along frames.
        /// </summary>
        public int id;
        /// <summary>
        /// Represent the estimated centroid within the image coordinates 
        /// system provided by the <see cref="IInputSource"/>.
        /// </summary>
        public Vector3 center;
        /// <summary>
        /// The current tracking status. See <see cref="TrackingState"/>.
        /// </summary>
        public TrackingState state;
        /// <summary>
        /// Computed linear velocity in the latest frames.
        /// </summary>
        public Vector2 linearVelocity;

        /// <summary>
        /// Initializes a new instance of the TrackerResult class.
        /// </summary>
        public TrackerResult()
        {
            id = -1;
            state = TrackingState.Unknown;
            center = Vector3.zero;
            linearVelocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct ColorPoint
    {
        public Vector2 position;
        public int targetIndex;       
    }

    /// <summary>
    ///  Encapsulates color extraction operations.
    /// </summary>
    public class ColorMap
    {
        public List<ColorPoint> points { get; set; }
        public Texture2D texture2D { get; set; }

        private byte[] _mapColors;

        /// <summary>
        /// Initializes a new instance of the ColorMap class.
        /// </summary>
        public ColorMap()
        {
            points = new List<ColorPoint>();
            texture2D = new Texture2D(1, 1, TextureFormat.RGB24, false);
            _mapColors = new byte[3];
        }

        /// <summary>
        /// Computes the color map from image buffer.
        /// </summary>
        /// <param name="bufferRGB"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="targets"></param>
        /// <param name="downscaleAmount"></param>
        /// <param name="pointSpacing"></param>
        public void ComputeColorMap(byte[] bufferRGB, int w, int h, List<ColorTarget> targets, int downscaleAmount, int pointSpacing = 1)
        {
            if (texture2D.height != h || texture2D.width != w)
            {
                texture2D.Resize(w, h, TextureFormat.RGB24, false);
                _mapColors = new byte[texture2D.width * texture2D.height * 3];              
            }

            Array.Clear(_mapColors, 0, _mapColors.Length);
            points.Clear();

            int bpp = 3;
            int maxPos = bufferRGB.Length - bpp;
            Color32 pixelColor = Color.black;
            for (var x = 0; x < w; x += pointSpacing)
            {
                for (var y = 0; y < h; y += pointSpacing)
                {
                    int pos = (y * w + x) * bpp;
                    if (pos < maxPos)
                    {
                        for (var targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                        {
                            Color32 targetColor = targets[targetIndex].color;
                            pixelColor.r = bufferRGB[pos];
                            pixelColor.g = bufferRGB[pos + 1];
                            pixelColor.b = bufferRGB[pos + 2];

                            if (CompareColors(targetColor, pixelColor, targets[targetIndex].tolerance * 2.0f))
                            {
                                _mapColors[pos] = bufferRGB[pos];
                                _mapColors[pos + 1] = bufferRGB[pos + 1];
                                _mapColors[pos + 2] = bufferRGB[pos + 2];
                                points.Add(new ColorPoint { position = new Vector2(x * downscaleAmount, y * downscaleAmount), targetIndex = targetIndex });
                            }
                        }
                    }
                    else break;
                }
            }
            texture2D.LoadRawTextureData(_mapColors);
            texture2D.Apply(false);
        }

        /// <summary>
        /// Compares to colors with certain tolerance.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private bool CompareColors(Color32 a, Color32 b, float tolerance)
        {
            //TODO: Improve the compare method
            float dr = a.r - b.r;
            float dg = a.g - b.g;
            float db = a.b - b.b;
            return (dr * dr + dg * dg + db * db) <= (tolerance * tolerance);
        }
    }

    /// <summary>
    /// Defines a color target to be processed.
    /// </summary>
    [System.Serializable]
    public class ColorTarget
    {
        /// <summary>
        /// Color to track.
        /// </summary>
        public Color32 color;

        /// <summary>
        /// The color tolerance. Defines the allowed amount of color variation.
        /// </summary>
        public float tolerance;

        /// <summary>
        /// Initializes a new instance of the ColorTarget class.
        /// </summary>
        /// <param name="color">The color to track in RGBA format. (Alpha component is not used)</param>
        /// <param name="tolerance">The allowed color tolerance level for this target. A higher level decrease the color track 
        /// precision but is useful for environments with changing lighting conditions.
        /// </param>
        public ColorTarget(Color32 color, float tolerance = 20.0f)
        {
            this.color = color;
            this.tolerance = tolerance;
        }
    }

    /// <summary>
    /// Interface that allows to implement the delegate pattern.
    /// </summary>
    public interface ITrackerEventListener
    {
        /// <summary>
        /// Called when tracker starts.
        /// </summary>
        /// <param name="tracker">The tracker sending this event.</param>       
        void TrackerDidStart(ColorTracker tracker);
        /// <summary>
        /// Called when tracker stops.
        /// </summary>
        /// <param name="tracker">The tracker sending this event.</param>  
        void TrackerDidStop(ColorTracker tracker);
        /// <summary>
        /// Called when tracker has new results available. 
        /// This is an optimal way to listen results without requesting the tracker every frame.
        /// </summary>
        /// <param name="tracker">The tracker sending this event.</param>  
        void WhenNewResultAvailable(ColorTracker tracker);

    }

    /// <summary>
    /// Controls all modules to perform the color tracking. <see cref="ColorTracker"/> class encapsulates 
    /// filters and processors to achieve color tracking. It also handles targets collection to be used 
    /// during the tracking session. You can obtain the tracker result implementing the <see cref="ITrackerEventListener"/> interface
    /// or by pooling the <see cref="GetLatestResult"/> method every frame. 
    /// </summary>
    public class ColorTracker
    {
        /// <summary>
        /// Returns the current library version.
        /// </summary>
        public static readonly string version = "1.0.1";

        /// <summary>
        /// Encapsulates color processing methods and other useful data.
        /// </summary>
        protected class ColorProcessor
        {
            /// <summary>
            /// Initializes a new instance of the ColorProcessor class.
            /// </summary>
            /// <param name="particleCount">The amount of particles to be used by <see cref="ParticleFilter"/> class.</param>
            /// <param name="width">The initial frame width.</param>
            /// <param name="height">The initial frame height.</param>
            public ColorProcessor(int particleCount, int width, int height)
            {
                _particle = new ParticleFilter(particleCount, width, height);
                _kalman = new KalmanFilter();
            }

            /// <summary>
            /// Sets or gets the color target to this processor.
            /// </summary>
            public ColorTarget target
            {
                get
                {
                    return _target;
                }

                set
                {
                    _target = value;
                    _particle.color = _target.color;
                }
            }

            /// <summary>
            /// Search the color on the image buffer. When color is found it enables a new <see cref="TrackerResult"/> and sets the 
            /// proper <see cref="TrackingState"/> to the object. To improve the memory usage the results are cached and persistent so 
            /// it is safe to store reference to results.
            /// </summary>
            /// <param name="pixelsRGB">The image buffer in RGB format.</param>
            /// <param name="result">A reference to the cached result to write on.</param>
            /// <param name="width">The current frame width. (useful for occasional frame dimension changes)</param>
            /// <param name="height">The current frame height. (useful for occasional frame dimension changes)</param>
            /// <param name="weightThreshold">The allowed amount of particles distribution to consider a good result.</param>
            /// <param name="useKalmanFilter">Toggles the use of <see cref="KalmanFilter"/>.</param>
            public void Compute(byte[] pixelsRGB, TrackerResult result, int width, int height, float weightThreshold, bool useKalmanFilter)
            {
                if (target == null || pixelsRGB.Length == 0) return;

                _particle.colorTolerance = target.tolerance;
                _particle.Resample(width, height);
                _particle.Predict();

                double w = _particle.ComputeWeight(pixelsRGB);

                if (w <= weightThreshold)
                {
                    result.state = TrackingState.Lost;
                    _particle.Reset();
                }
                else
                {
                    Vector3 prevCenter = result.center;
                    _particle.Measure(ref result.center);

                    float depth = ((float)w * 100 / _particle.ParticleCount());
                    result.center.z = depth;

                    if (useKalmanFilter)
                    {
                        if (result.state == TrackingState.Tracked)
                            _kalman.Predict(ref result.center);
                        else
                            _kalman.ResetTo(result.center);
                    }

                    Vector2 lvel = (result.center - prevCenter);
                    lvel.y *= -1;
                    result.linearVelocity = lvel;
                    result.state = TrackingState.Tracked;
                }
            }

            private ParticleFilter _particle;
            private KalmanFilter _kalman;
            private ColorTarget _target;
        }

        /// <summary>
        ///  Enables color map generation.
        /// </summary>
        public bool enableColorMap { get; set; }

        /// <summary>
        ///  Enables color centroid estimations.
        /// </summary>
        public bool enableColorTrack { get; set; }

        /// <summary>
        ///  Defines spacing to be used when color map is computed. 
        /// </summary>
        public int  colorMapPointSpacing { get; set; }

        /// <summary>
        /// Property used to enable or disable the tracks jitter removal.
        /// </summary>
        public bool useKalmanFilter { get; set; }
        /// <summary>
        /// Defines the accuracy level when analyze the scene. 
        /// </summary>
        public TrackerAccuracy accuracy { get; set; }

        /// <summary>
        /// Returns a boolean value indicating whether the tracker is running.
        /// </summary>
        public bool isRunning { get { return _running; } }

        /// <summary>
        /// Allows to set a listener that reacts to events generated by 
        /// the <see cref="ColorTracker"/> class.
        /// </summary>
        public ITrackerEventListener listener { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ColorTracker"/> class attached to the input defined by parameter. 
        /// </summary>
        /// <param name="input"></param>
        public ColorTracker(IInputSource input)
        {
            _input = input;
            _colorTargets = new List<ColorTarget>();
            _result = new List<TrackerResult>();
            _colorTargets = new List<ColorTarget>();
            _processors = new List<ColorProcessor>();
            _resultHash = new Dictionary<int, TrackerResult>();
            _colorMap = new ColorMap();

            accuracy = TrackerAccuracy.Fast;
            enableColorTrack = true;
            useKalmanFilter = true;
            enableColorMap = false;
            colorMapPointSpacing = 1;
        }

        /// <summary>
        /// Gets or sets the list of color targets to track. 
        /// </summary>
        public List<ColorTarget> colorTargets
        {
            get
            {
                return _colorTargets;
            }

            set
            {
                _colorTargets = value;

                if (_colorTargets.Count != _processors.Count)
                {
                    ResetInternal();
                    for (var i = 0; i < _colorTargets.Count; i++)
                    {
                        var proc = new ColorProcessor(GetParticleCountByAccurancy(accuracy), _input.width, _input.height);
                        proc.target = _colorTargets[i];
                        _processors.Add(proc);

                        TrackerResult storedResult = new TrackerResult();
                        storedResult.id = i + 1;
                        _resultHash[i] = storedResult;
                    }
                }
                else
                {
                    for (var i = 0; i < _colorTargets.Count; i++)
                    {
                        var proc = _processors[i];
                        proc.target = _colorTargets[i];
                    }
                }
            }
        }

        /// <summary>
        /// Returns the current input source attached to this tracker 
        /// </summary>
        public IInputSource input
        {
            get
            {
                return _input;
            }
        }

        /// <summary>
        /// The tracker will dispatch results.
        /// </summary>
        /// 
        public bool Start()
        {
            if (_processors.Count > 0)
            {
                _running = true;

                if (accuracy == TrackerAccuracy.Optimal)
                    _downscaleAmount = 1;
                else
                    _downscaleAmount = 2;

                if (listener != null)
                    listener.TrackerDidStart(this);

                return true;
            }
            return false;
        }

        /// <summary>
        /// This will release all internal modules. 
        /// After calling this function the tracker is disabled until you call <see cref="Start"/>. 
        /// </summary>
        public void Stop()
        {
            if (_running)
            {
                _running = false;

                if (listener != null)
                {
                    listener.TrackerDidStop(this);
                }
            }
        }

        /// <summary>
        /// You can get the tracking results by calling this function. 
        /// </summary>
        /// <returns>The latest results computed by the <see cref="ColorTracker"/></returns>
        public List<TrackerResult> GetLatestResult()
        {
            return _result;
        }

        /// <summary>
        ///  Retrieves the computed color map for the latest frame.
        /// </summary>
        /// <returns>The color map instance.</returns>
        public ColorMap GetColorMap()
        {
            if (!enableColorMap)
                throw new InvalidOperationException("You must enable color mapping before use it!");
            return _colorMap;
        }

        /// <summary>
        /// Analyzes the image frame, applies filters and creates new <see cref="TrackerResult"/> objects. 
        /// This method performs a deep analysis over the frame and is computationally expensive.
        /// Use this method wisely and never more than once per frame. If you need to get the tracker results
        /// multiple times in a frame use <see cref="GetLatestResult"/> instead.
        /// </summary>
        /// <param name="weightThreshold">The allowed amount of particles distribution to consider a good result.</param>
        /// <returns>The latest tracking result.</returns>
        public List<TrackerResult> Compute(float weightThreshold = 0.01f)
        {
            _result.Clear();

            if (_input != null && _running)
            {
                int dstW = _input.width / _downscaleAmount;
                int dstH = _input.height / _downscaleAmount;

                if (_input.data == null) return _result; 

                if (_downscaleAmount > 1)
                {
                    int requiredBufferLenght = dstW * dstH * 3;

                    if (_resizedBuffer == null || _resizedBuffer.Length != requiredBufferLenght)
                        _resizedBuffer = new byte[requiredBufferLenght];

                    ResizeBuffer(_resizedBuffer, _input.data, _input.width, _input.height, dstW, dstH, 3, 3);
                }
                else
                    _resizedBuffer = _input.data;

                if (enableColorTrack)
                {
                    for (int i = 0; i < _processors.Count; ++i)
                    {
                        ColorProcessor clp = _processors[i];
                        TrackerResult result = _resultHash[i];

                        clp.Compute(_resizedBuffer, result, dstW, dstH, weightThreshold, useKalmanFilter);

                        if (!IsPositionNearToBounds(ref result.center, 10.0f))
                        {
                            result.center *= _downscaleAmount;
                            _result.Add(result);
                        }
                    }
                }

                if (enableColorMap)
                {
                    _colorMap.ComputeColorMap(_resizedBuffer, dstW, dstH, colorTargets, _downscaleAmount, colorMapPointSpacing);
                }

                if (listener != null && _result.Count > 0)
                {
                    listener.WhenNewResultAvailable(this);
                }
            }

            return _result;
        }

        /// <summary>
        /// Implicit boolean operator
        /// </summary>
        public static implicit operator bool(ColorTracker t)
        {
            return t != null;
        }

        /// <summary>
        /// Resets tracker to initial state.
        /// </summary>
        private void ResetInternal()
        {
            _processors.Clear();
            _resultHash.Clear();
        }

        /// <summary>
        /// Returns the number of particles to use by <see cref="TrackerAccuracy"/> category.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private int GetParticleCountByAccurancy(TrackerAccuracy a)
        {
            int particleCount = 50;
            switch (a)
            {
                case TrackerAccuracy.Fast: particleCount = 100; break;
                case TrackerAccuracy.Balanced: particleCount = 200; break;
                case TrackerAccuracy.Optimal: particleCount = 300; break;
            }
            return particleCount;
        }

        /// <summary>
        /// Detects when a point is closer to image bounds.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        private bool IsPositionNearToBounds(ref Vector3 pos, float padding)
        {
            int w = _input.width;
            int h = _input.height;
            return (pos.x >= w - padding || pos.x <= padding || pos.y >= h - padding || pos.y <= padding);
        }

        /// <summary>
        /// Computes the enclosing rectangle for a group of particles.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="rect"></param>
        private void BoundingRect(ParticleFilter.Particle[] points, ref Rect rect)
        {
            var minX = points.Min(p => p.x[0]);
            var minY = points.Min(p => p.x[1]);
            var maxX = points.Max(p => p.x[0]);
            var maxY = points.Max(p => p.x[1]);
            rect.Set(minX, _input.height - minY - (maxY - minY), maxX - minX, maxY - minY);
        }

        /// <summary>
        ///  Resizes the image buffer.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="srcWidth"></param>
        /// <param name="srcHeight"></param>
        /// <param name="destWidth"></param>
        /// <param name="destHeight"></param>
        /// <param name="srcBpp"></param>
        /// <param name="dstBpp"></param>
        private void ResizeBuffer(byte[] dest, byte[] src, int srcWidth, int srcHeight, int destWidth, int destHeight, int srcBpp, int dstBpp)
        {
            double scaleWidth = 1 / ((double)destWidth / (double)srcWidth);
            double scaleHeight = 1 / ((double)destHeight / (double)srcHeight);
            for (int cy = 0; cy < destHeight; cy++)
            {
                for (int cx = 0; cx < destWidth; cx++)
                {
                    int pixel = (cy * (destWidth * dstBpp)) + (cx * dstBpp);
                    int nearestMatch = (((int)(cy * scaleHeight) * (srcWidth * srcBpp)) + ((int)(cx * scaleWidth) * srcBpp));
                    dest[pixel] = src[nearestMatch];
                    dest[pixel + 1] = src[nearestMatch + 1];
                    dest[pixel + 2] = src[nearestMatch + 2];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected IInputSource _input;
        /// <summary>
        /// 
        /// </summary>
        protected List<TrackerResult> _result;
        /// <summary>
        /// 
        /// </summary>
        protected List<ColorProcessor> _processors;
        /// <summary>
        /// 
        /// </summary>
        protected List<ColorTarget> _colorTargets;
        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<int, TrackerResult> _resultHash;
        /// <summary>
        /// 
        /// </summary>
        protected bool _running = false;
        /// <summary>
        /// 
        /// </summary>
        protected byte[] _resizedBuffer;
        /// <summary>
        /// 
        /// </summary>
        protected int _downscaleAmount = 2;
        /// <summary>
        /// 
        /// </summary>
        protected ColorMap _colorMap;
    }
}
