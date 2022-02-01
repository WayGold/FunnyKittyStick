using UnityEngine;


/// <summary>
/// The Vexpot.Integration namespace contains all scripts used to achieve an easy integration
/// with the Unity 3D editor.
/// </summary>
namespace Vexpot.Integration
{
    /// <summary>
    /// Captures everything on the camera view and creates an RGB image to be computed by <see cref="ColorTracker"/>.
    /// </summary>
    [AddComponentMenu("Vexpot/ScreenInput")]
    public class ScreenInput : IInputSource
    {
        protected Texture2D _texture;
        protected Rect _sourceSize;

        /// <summary>
        /// Initializes a new instance of the ScreenInput class.
        /// </summary>
        public ScreenInput()
        {
            _texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            _sourceSize = new Rect(0, 0, 1, 1);
        }

        /// <summary>
        /// Implements <see cref="IInputSource.data"/>
        /// </summary>
        public byte[] data
        {
            get
            {
                int w = Screen.width;
                int h = Screen.height;

                if (_sourceSize.width != w || _sourceSize.height != h)
                {
                    _texture.Resize(w, h);
                    _sourceSize.Set(0, 0, w, h);
                }

                _texture.ReadPixels(_sourceSize, 0, 0);
                _texture.Apply(false);
                return _texture.GetRawTextureData();
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
            //nothing to close here.
        }
    }
}