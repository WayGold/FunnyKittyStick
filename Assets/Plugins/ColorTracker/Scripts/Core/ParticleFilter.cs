using System;
using UnityEngine;

/// <summary>
/// The Vexpot namespace contains all core functionalities used in the library.
/// </summary>
namespace Vexpot
{
    /// <summary>
    /// Implements the one particle filter algorithm. Used to track colors on image frames.
    /// https://en.wikipedia.org/wiki/Particle_filter
    /// </summary>
    public class ParticleFilter
    {
        /// <summary>
        /// Encapsulates the particle parameters.
        /// </summary>
        public class Particle
        {
            /// <summary>
            /// 
            /// </summary>
            public int[]  x;
            /// <summary>
            /// 
            /// </summary>
            public double weight = 0.0f;

            /// <summary>
            /// Initializes a new instance of the <see cref="Particle"/> class.
            /// </summary>
            public Particle()
            {
                x = new int[4];
            }

            /// <summary>
            /// Deep copy of the object.
            /// </summary>
            /// <param name="src">Source object to be copied.</param>
            public void CopyFrom(Particle src)
            {
                for (int i = 0; i < 4; ++i)
                     x[i] = src.x[i];

                weight = src.weight;
            }
        }

        private int          _dimension;
        private int          _particleCount;
        private int          _width;
        private int          _height;
        private int []       _upper;
        private int []       _lower;
        private int []       _noise;        
        private Particle []  _samples;
        private Particle[]   _preSamples;
        Color32              _targetColor;
        private float        _colorTolerance;
        private double[]     _measureX;
        private double[]     _prevWeight;

        private static System.Random rndObj = new System.Random();
        private static int maxRandomValue = 32767;

        /// <summary>
        /// Fast implementation of the Particle Filter algorithm.
        /// </summary>
        /// <param name="particleCount">Amount of particles to be processed.</param>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        public ParticleFilter(int particleCount, int width, int height)
        {
            _upper = new int [] { width, height, 10, 10 };
            _lower = new int [] { 0, 0, -10, -10 };
            _noise = new int [] { 35, 35, 10, 10 };
            _dimension = 4;
            _measureX = new double[_dimension];
            _colorTolerance = 30.0f;
            _particleCount = particleCount;
            _width = width;
            _height = height;

            _targetColor = new Color32();
            _targetColor.r = _targetColor.g = _targetColor.b = 0;

            _samples = new Particle[particleCount];
            _preSamples = new Particle[particleCount];
            _prevWeight = new double[particleCount];

            for (int i = 0; i< particleCount; i++){
                _samples[i] = new Particle();
            }

            Reset();       
        }

        /// <summary>
        /// The total amount of particles used in the filter.
        /// </summary>
        /// <returns></returns>
        public int ParticleCount()
        {
            return _particleCount;
        }

        /// <summary>
        /// Resets the particle filter to its default state.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < _particleCount; i++)
            {
                Particle p = _samples[i];
                for (int j = 0; j < _dimension; j++)
                {
                    int r = rndObj.Next(0, maxRandomValue);
                    p.x[j] = r % (_upper[j] - _lower[j]) + _lower[j];
                }
                p.weight = 1.0 / _particleCount;               
            }
        }

        /// <summary>
        /// Set color to use on scanning phase.
        /// </summary>
        public Color32 color 
        {
            set
            {
                _targetColor = value;
            }
        }

        /// <summary>
        /// Defines the allowed difference from the target color.
        /// </summary>
        public float colorTolerance
        {
            set
            {
                _colorTolerance = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double ComputeWeight(byte[] pixelsRGB)
        {
            if (pixelsRGB == null) return 0;

            double sum = 0.0f;
            int bpp = 3;
            int maxPos = pixelsRGB.Length - bpp;
            for (int i = 0; i < _particleCount; ++i)
            {
                Particle smp = _samples[i];
                int x = smp.x[0];
                int y = smp.x[1];

                if (x > _width)
                {
                    x = smp.x[0] = _width;
                }

                if (y > _height)
                {
                    y = smp.x[1] = _height;
                }

                int pos = (y * _width + x) * bpp;

                if (pos <= maxPos)
                {
                    byte r = pixelsRGB[pos];
                    byte g = pixelsRGB[pos + 1];
                    byte b = pixelsRGB[pos + 2];
                    smp.weight = Likelihood(r, g, b);
                }
                else {
                    smp.weight = 0.0001;
                }
                sum += smp.weight;
            }

            for (int k = 0; k < _particleCount; ++k)
            {
                _samples[k].weight /= sum;
            }
            return sum;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Measure(ref Vector3 result)
        {
            Array.Clear(_measureX, 0, _measureX.Length); 

            for (int i = 0; i < _particleCount; i++)
            {
                double w = _samples[i].weight;
                _measureX[0] += _samples[i].x[0] * w;
                _measureX[1] += _samples[i].x[1] * w;
                _measureX[2] += _samples[i].x[2] * w;
                _measureX[3] += _samples[i].x[3] * w;
            }

            float rx = 0, ry = 0;
            for (int k = 0; k < 2; k++)
            {
                if(k == 0)
                    rx = (int)_measureX[k];
                else
                    ry = (int)_measureX[k];
            }

            result.x = rx;
            result.y = ry;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Predict()
        {
            for (int i = 0; i < _particleCount; i++)
            {
                int[] n = new int[_dimension];
                int rnd = rndObj.Next(0, maxRandomValue);
                n[0] = (int)(rnd % (_noise[0] * 2) - _noise[0]);
                rnd = rndObj.Next(0, maxRandomValue);
                n[1] = (int)(rnd % (_noise[1] * 2) - _noise[1]);
                rnd = rndObj.Next(0, maxRandomValue);
                n[2] = (int)(rnd % (_noise[2] * 2) - _noise[2]);
                rnd = rndObj.Next(0, maxRandomValue);
                n[3] = (int)(rnd % (_noise[3] * 2) - _noise[3]);
                
                int[] v = _samples[i].x;
                v[0] += v[2] + n[0];
                v[1] += v[3] + n[1];
                v[2] = n[2];
                v[3] = n[3];
                if (v[0] < _lower[0]) v[0] = _lower[0];
                if (v[1] < _lower[1]) v[1] = _lower[1];
                if (v[2] < _lower[2]) v[2] = _lower[2];
                if (v[3] < _lower[3]) v[3] = _lower[3];
                if (v[0] >= _upper[0]) v[0] = _upper[0];
                if (v[1] >= _upper[1]) v[1] = _upper[1];
                if (v[2] >= _upper[2]) v[2] = _upper[2];
                if (v[3] >= _upper[3]) v[3] = _upper[3];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Resample(int width, int height)
        {
            _width = width;
            _height = height;
            _upper[0] = width;
            _upper[1] = height;

            double [] w = _prevWeight;

            w[0] = _samples[0].weight;
            for (int i = 1; i < _particleCount; i++)
            {
                w[i] = w[i - 1] + _samples[i].weight;
            }
                       
            Array.Copy(_samples, _preSamples, _particleCount);

            for (int j = 0; j < _particleCount; j++)
            {
                int rnd = rndObj.Next(0, maxRandomValue);
                double div = (rnd % 10000) / 10000.0;
                for (int k = 0; k < _particleCount; k++)
                {
                    if (div > w[k])
                        continue;
                    else {
                        _samples[j].x[0] = _preSamples[k].x[0];  // x
                        _samples[j].x[1] = _preSamples[k].x[1];  // y
                        _samples[j].x[2] = _preSamples[k].x[2];  // u
                        _samples[j].x[3] = _preSamples[k].x[3];  // v
                        _samples[j].weight = 0.0;
                        break;
                    }
                }
            }            
        } 

        /// <summary>
        /// 
        /// </summary>
        private double Likelihood(byte red, byte green, byte blue)
        {
            double sigma = _colorTolerance;
            byte tr = _targetColor.r;
            byte tg = _targetColor.g;
            byte tb = _targetColor.b;

            int dr = tr - red;
            int dg = tg - green;
            int db = tb - blue;

            double dist = Math.Sqrt(db * db + dg * dg + dr * dr);
            return 1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma) * Math.Exp(-dist * dist / (2.0 * sigma * sigma));
        }
  
    }
}
