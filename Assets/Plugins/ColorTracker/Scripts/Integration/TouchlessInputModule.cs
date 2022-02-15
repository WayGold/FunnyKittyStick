
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The Vexpot.Integration namespace contains all scripts used to achieve an easy integration
/// with the Unity 3D editor.
/// </summary>
namespace Vexpot.Integration
{
    /// <summary>
    /// Converts <see cref="ColorTracker"/> results to Unity native events. This allows to integrate your 
    /// traditional user interfaces with touchless without any changes. 
    /// </summary>
    [AddComponentMenu("Vexpot/TouchlessInputModule")]
    public class TouchlessInputModule : BaseInputModule
    {
        /// <summary>
        /// The amount of seconds to wait before emit a touch down event.
        /// </summary>
        public float touchDelay = 0.5f;
        /// <summary>
        /// A reference to the GameObject containing the tracker panel.
        /// </summary>
        public ColorTrackerPanel colorTrackerPanel;

        private List<PointerEventData> _pointerData;
        private Vector3 _reusablePosition;
        private Dictionary<int, GameObject> _selectionHash;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            _reusablePosition = new Vector3();
            _pointerData = new List<PointerEventData>();
            _selectionHash = new Dictionary<int, GameObject>();
        }

        /// <inheritdoc />
        public override bool ShouldActivateModule()
        {
            return colorTrackerPanel != null && colorTrackerPanel.GetColorTracker() != null;
        }

        /// <inheritdoc />
        public override void DeactivateModule()
        {
            base.DeactivateModule();
            for (var i = 0; i < _pointerData.Count; i++)
            {
                PointerEventData p = _pointerData[i];
                HandlePointerExitAndEnter(p, null);
                p.pointerPress = null;
                p = null;
            }
            eventSystem.SetSelectedGameObject(null, GetBaseEventData());
            _pointerData.Clear();
            _selectionHash.Clear();
        }

        /// <inheritdoc />
        public override void Process()
        {
            UpdateEventsAndRaycast();
        }

        private void AllocatePointerDataIfNeeded(List<TrackerResult> tracks)
        {
            if (tracks.Count != _pointerData.Count)
            {
                _pointerData.Clear();
                for (var t = 0; t < tracks.Count; t++)
                {
                    _pointerData.Add(new PointerEventData(eventSystem) { pointerId = t });
                    _selectionHash[t] = null;
                }
            }
        }

        private void UpdateEventsAndRaycast()
        {
            if (colorTrackerPanel == null) return;
            ColorTracker tracker = colorTrackerPanel.GetColorTracker();
            if (tracker == null) return;

            List<TrackerResult> colorTrackerResults = tracker.GetLatestResult();
            AllocatePointerDataIfNeeded(colorTrackerResults);

            for (var t = 0; t < colorTrackerResults.Count; t++)
            {
                PointerEventData current = _pointerData[t];
                current.Reset();
                TrackerResult r = colorTrackerResults[t];

                if (r.state == TrackingState.Tracked)
                {
                    CoordinateMapper.ConvertInputToScreen(tracker.input, r.center, ref _reusablePosition);
                }
                else
                {
                    _reusablePosition.Set(-150, -150, 0);
                }

                current.position = _reusablePosition;
                current.delta = r.linearVelocity;
                eventSystem.RaycastAll(current, m_RaycastResultCache);
                current.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
                UpdateHoverAndExit(current);
                HandleTrigger(current);
                m_RaycastResultCache.Clear();
            }
        }

        private void UpdateHoverAndExit(PointerEventData data)
        {
            var currentGameObject = data.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(data, currentGameObject);

            if (currentGameObject != null)
            {
                ExecuteEvents.Execute(currentGameObject, data, ExecuteEvents.updateSelectedHandler);
            }
            else
            {
                _selectionHash[data.pointerId] = null;
                data.selectedObject = null;
                data.eligibleForClick = false;
            }
        }

        private void HandleTrigger(PointerEventData pointerData)
        {
            var currentGameObject = pointerData.pointerCurrentRaycast.gameObject;

            if (_selectionHash[pointerData.pointerId] != currentGameObject && currentGameObject != null)
            {
                _selectionHash[pointerData.pointerId] = currentGameObject;
                pointerData.selectedObject = currentGameObject;
                pointerData.eligibleForClick = true;
                StartCoroutine(DispatchPressEvent(currentGameObject, pointerData));
            }
        }

        private IEnumerator DispatchPressEvent(GameObject currentGameObject, PointerEventData pointerData)
        {
            yield return new WaitForSeconds(touchDelay);
            if (pointerData.eligibleForClick)
            {
                var evtHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentGameObject);
                pointerData.pressPosition = pointerData.position;
                pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
                pointerData.pointerPress = ExecuteEvents.ExecuteHierarchy(currentGameObject, pointerData, ExecuteEvents.pointerDownHandler) ?? evtHandler;
                StartCoroutine(DispatchUpEvent(currentGameObject, pointerData));
            }
        }

        private IEnumerator DispatchUpEvent(GameObject currentGameObject, PointerEventData pointerData)
        {
            yield return new WaitForSeconds(0.2f);
            var evtHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentGameObject);
            pointerData.pointerPress = ExecuteEvents.ExecuteHierarchy(currentGameObject, pointerData, ExecuteEvents.pointerUpHandler) ?? evtHandler;

            if (pointerData.eligibleForClick)
            {
                pointerData.pointerPress = ExecuteEvents.ExecuteHierarchy(currentGameObject, pointerData, ExecuteEvents.pointerClickHandler) ?? evtHandler;
            }
        }
    }
}
