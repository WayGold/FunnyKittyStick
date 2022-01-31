using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The Vexpot.Integration namespace contains all scripts used to achieve an easy integration
/// with the Unity 3D editor.
/// </summary>
namespace Vexpot.Integration
{

    /// <summary>
    /// Enables 3D objects in the scene to listen touchless events emitted by <see cref="TouchlessInputModule"/>
    /// </summary>
    [AddComponentMenu("Vexpot/InteractiveItem")]
    public class InteractiveItem : EventTrigger
    {

        private Renderer render;

        void Start()
        {
            render = GetComponent<Renderer>();
            ExitBehaviour();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public override void OnPointerEnter(PointerEventData f)
        {
            EnterBehaviour();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public override void OnPointerExit(PointerEventData f)
        {
            ExitBehaviour();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public override void OnPointerClick(PointerEventData data)
        {
            Debug.Log("OnPointerClick called.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public override void OnPointerDown(PointerEventData data)
        {
            Debug.Log("PointerDown called.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public override void OnPointerUp(PointerEventData data)
        {
            Debug.Log("OnPointerUp called.");
        }

        private void EnterBehaviour()
        {
            if (render)
                render.material.color = Color.yellow;
        }

        private void ExitBehaviour()
        {
            if (render)
                render.material.color = Color.white;
        }
    }
}