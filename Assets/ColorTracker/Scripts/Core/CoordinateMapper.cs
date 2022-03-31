using UnityEngine;

/// <summary>
/// The Vexpot namespace contains all core functionalities used in the library.
/// </summary>
namespace Vexpot
{
    /// <summary>
    /// Implements the mapper that provides translation operations from one point space to another.
    /// </summary>
    public class CoordinateMapper
    {
        /// <summary>
        /// Maps a point from <see cref="IInputSource"/> coordinate system to Unity screen position.
        /// </summary>
        /// <param name="input">Current <see cref="IInputSource"/> object used by the tracker.</param>
        /// <param name="inputPosition"> The point in image coordinate system.</param>
        /// <param name="screenPosition"> The mapped point in Unity screen coordinates.</param>
        public static void ConvertInputToScreen(IInputSource input, Vector3 inputPosition, ref Vector3 screenPosition)
        {
            int sw = Screen.width;
            int sh = Screen.height;
            float iw = input.width;
            float ih = input.height;

            if (sw != iw || sh != ih)
            {
                float xScale = inputPosition.x / iw;
                float yScale = inputPosition.y / ih;
                screenPosition.Set(sw * xScale, sh * yScale, inputPosition.z);
            }
            else
            {
                screenPosition.Set(inputPosition.x, inputPosition.y, inputPosition.z);
            }
        }

        /// <summary>
        /// Maps a point from <see cref="IInputSource"/> coordinate system to Unity world position.
        /// </summary>
        /// <param name="perspectiveCamera">The perspective camera you are using.</param>
        /// <param name="input"></param>
        /// <param name="inputPosition"></param>
        /// <param name="worldPosition"></param>
        /// <param name="zOffset"></param>
        public static void ConvertInputToWorld(Camera perspectiveCamera, IInputSource input, Vector3 inputPosition, ref Vector3 worldPosition, float zOffset = 0)
        {
            ConvertInputToScreen(input, inputPosition, ref worldPosition);
            worldPosition.z = perspectiveCamera.nearClipPlane + zOffset;
            worldPosition = perspectiveCamera.ScreenToWorldPoint(worldPosition);
        }

        /// <summary>
        /// Maps a point from Unity screen position to UI coordinate system.
        /// </summary>
        /// <param name="screenPosition">The point in screen coordinate system.</param>
        /// <param name="uiPosition">The mapped point in UI space.</param>
        public static void ConvertScreenToUI(Vector2 screenPosition, RectTransform uiPosition)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector2 viewportPoint = Camera.main.ScreenToViewportPoint(screenPosition);
                uiPosition.anchorMin = viewportPoint;
                uiPosition.anchorMax = viewportPoint;
            }
        }
    }
}
