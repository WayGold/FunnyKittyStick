

using UnityEngine;
/// <summary>
/// The Vexpot namespace contains all core functionalities used in the library.
/// </summary>
namespace Vexpot
{
    /// <summary>
    /// Defines an abstract input for the color tracker.
    /// You can build your own custom input by implementing this class.
    /// </summary>
    public interface IInputSource
    {
        /// <summary>
        /// Only read property. Defines the frame width.
        /// </summary>
        int width { get; }
        
        /// <summary>
        /// Only read property. Defines the frame height.
        /// </summary>
        int height { get; }
       
        /// <summary>
        /// Get the input raw pixels in RGB format.
        /// </summary>
        byte[] data { get; }

        /// <summary>
        /// Explicitly closes the source
        /// </summary>
        void Close();
    }  
}
