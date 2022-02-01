using UnityEngine;

/// <summary>
/// The Vexpot.Integration namespace contains all scripts used to achieve an easy integration
/// with the Unity 3D editor.
/// </summary>
namespace Vexpot.Integration
{
    /// <summary>
    /// Displays the image from camera device on scene object.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Vexpot/VideoCaptureRenderer")]
    public class VideoCaptureRenderer : MonoBehaviour
    {
        private Material _screenMaterial;
        private GameObject _screen;
        private WebCamTexture _texture;
        private WebCamDevice _device;

        [HideInInspector]
        public int deviceIndex;

        void Start()
        {
            _screen = GameObject.Find("Screen");
            if (_screen == null) return;

            _screenMaterial = GetComponent<Renderer>().material;

            if (WebCamTexture.devices.Length > 0)
            {
                if (Application.isEditor)
                {
                    _device = WebCamTexture.devices[deviceIndex];
                }
                else
                {
                    _device = WebCamTexture.devices[0];
                }

                _texture = new WebCamTexture(_device.name);
                _screenMaterial.mainTexture = _texture;
                _texture.Play();
            }
            else
            {
                Debug.LogError("No camera device found");
            }
        }

        void OnDestroy()
        {
            if (_texture)
                _texture.Stop();          
        }


        void Update()
        {
            if (_texture == null) return;

            if (_device.isFrontFacing)
            {
                _screen.transform.localScale = new Vector3(1.9f, 1, -1.4f);
            }
            else
            {
                _screen.transform.localScale = new Vector3(-1.9f, 1, -1.4f);
            }
        }
    }
}