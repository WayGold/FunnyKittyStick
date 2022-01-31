using UnityEngine;

/// <summary>
/// The Vexpot namespace contains all core functionalities used in the library.
/// </summary>
namespace Vexpot
{
    /// <summary>
    /// Implements the Kalman filter algorithm. Used to reduce the position jitter in tracks.
    /// </summary>
    public class KalmanFilter
    {
        struct KalmanState
        {
            /// <summary>
            /// Process noise covariance.
            /// </summary>
            public float q;
            /// <summary>
            /// Sensor noise covariance.
            /// </summary>
            public float r;
            /// <summary>
            /// Estimation error covariance.
            /// </summary>                         
            public float p;
            /// <summary>
            /// Kalman gain factor.
            /// </summary>
            public float k;
            /// <summary>
            /// The value treated with the filter.
            /// </summary>
            public Vector3 value; 
        };

        /// <summary>
        /// Creates a new KalmanFilter instance.
        /// </summary>
        /// <param name="q">Process noise covariance.</param>
        /// <param name="r">Sensor noise covariance.</param>
        /// <param name="p">Estimation error covariance.</param>
        public KalmanFilter(float q = 0.06f, float r =1.5f, float p = 0.08f)
        {
            _state = new KalmanState();
            _state.value = new Vector3();
            _state.q = q;
            _state.r = r;
            _state.p = p;        
        }

        /// <summary>
        /// Resets the internal measurement without the filtering.
        /// </summary>
        /// <param name="value"></param>
        public void ResetTo(Vector3 value)
        {
            _state.value = value;
        }

        /// <summary>
        /// Applies the filter to the sensor measurement.
        /// </summary>
        /// <param name="measurement">The input noisy measurement</param>
        /// <param name="adaptativeThreshold">The max allowed distance difference to enables or disables the filter.</param>
        public void Predict(ref Vector3 measurement, float adaptativeThreshold = 25.0f)
        {
            if (Vector2.Distance(_state.value, measurement) <= adaptativeThreshold)
            {
                _state.p = _state.p + _state.q;
                _state.k = _state.p / (_state.p + _state.r);
                _state.value.x = _state.value.x + _state.k * (measurement.x - _state.value.x);
                _state.value.y = _state.value.y + _state.k * (measurement.y - _state.value.y);
                _state.value.z = _state.value.z + _state.k * (measurement.z - _state.value.z) * 0.5f;

                _state.p = (1 - _state.k) * _state.p;
                measurement = _state.value;
            }
            else
              _state.value = measurement;
        }

        private KalmanState _state;        
    }
}
