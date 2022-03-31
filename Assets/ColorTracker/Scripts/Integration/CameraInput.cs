using UnityEngine;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// The Vexpot.Integration namespace contains all scripts used to achieve an easy integration.
/// with the Unity 3D editor.
/// </summary>
namespace Vexpot.Integration
{
    /// <summary>
    /// Input that uses the webcam's image source.
    /// </summary>
    [AddComponentMenu("Vexpot/CameraInput")]
    public class WebCamInput : IInputSource
    {
        private WebCamTexture _texture;
        private WebCamDevice _device;
        private byte[] rgbaBuffer;
        private byte[] rgbBuffer;

        /// <summary>
        /// Initializes a new instance of the WebCamInput class.
        /// </summary>
        public WebCamInput(int requestedWidth = 640, int requestedHeight = 480, int requestedFPS = 30)
        {
            if (WebCamTexture.devices.Length > 0)
            {
                _device = WebCamTexture.devices[0];
                _texture = new WebCamTexture(_device.name, requestedWidth, requestedHeight, requestedFPS);
                _texture.Play();
            }
            else
            {
                Debug.LogError("No camera device found");
            }
        }

        /// <summary>
        /// Implements <see cref="IInputSource.data"/>
        /// </summary>
        public byte[] data
        {
            get
            {
                if (_texture && _texture.isPlaying)
                    return Color32ArrayToByteArray(_texture.GetPixels32());
                else return null;
            }
        }

        /// <summary>
        /// Implements <see cref="IInputSource.height"/>
        /// </summary>
        public int height
        {
            get
            {
                return _texture.height;
            }
        }

        /// <summary>
        /// Implements <see cref="IInputSource.width"/>
        /// </summary>
        public int width
        {
            get
            {
                return _texture.width;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            if (_texture)
                _texture.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetSize"></param>
        private void ResizeBufferIfNeeded(int targetSize)
        {
            if (rgbaBuffer == null || rgbBuffer == null || rgbBuffer.Length / 3 != targetSize)
            {
                rgbaBuffer = new byte[targetSize * 4];
                rgbBuffer = new byte[targetSize * 3];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        private byte[] Color32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            ResizeBufferIfNeeded(colors.Length);

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, rgbaBuffer, 0, colors.Length * 4);
                RgbaToRgb(rgbBuffer, rgbaBuffer, rgbaBuffer.Length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return rgbBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="srcSize"></param>
        private void RgbaToRgb(byte[] dest, byte[] src, int srcSize)
        {
            int s = 0, d = 0, last = srcSize / 4 * 3;
            while (d < last)
            {
                dest[d++] = src[s++];
                dest[d++] = src[s++];
                dest[d++] = src[s++];
                s++;
            }
        }       
    }
}