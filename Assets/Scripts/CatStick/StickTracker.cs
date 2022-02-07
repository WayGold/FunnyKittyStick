using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Vexpot.Integration namespace contains all scripts used to achieve an easy integration
/// with the Unity 3D editor.
/// </summary>
namespace Vexpot.Integration
{
    public class StickTracker : MonoBehaviour
    {
        public bool MouseControll;

        public GameObject stickModel;
        public ColorTrackerPanel trackerPanel;

        private GameObject m_Model;
        private ColorTracker _tracker;
        private List<GameObject> _graphics;

        public Vector3 screenPos;
        public Vector3 worldPos;

        void Awake()
        {
            screenPos = new Vector3();
            if (MouseControll)
            {
                GameObject plane= GameObject.Instantiate(Resources.Load("Plane"))as GameObject;
                plane.transform.position =new Vector3(0, 11, 0);
            }
                
        }
        private void Update()
        {
            if(MouseControll)
            {
                //worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //Vector3 offset=worldPos;
                //stickModel.transform.SetPositionAndRotation(new Vector3(worldPos.x, worldPos.y, transform.position.z), Quaternion.identity);
                //return;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                bool ishit = Physics.Raycast(ray, out hit);
                if (ishit && hit.collider.gameObject.tag=="Plane")
                {
                    Vector3 spacePos = hit.point;
                    stickModel.transform.SetPositionAndRotation(spacePos, Quaternion.identity);
                    return;
                }
            }
            _tracker = trackerPanel.GetColorTracker();
            List<TrackerResult> result = _tracker.Compute();
            for (var i = 0; i < result.Count; i++)
            {
                TrackerResult target = result[i];
                CoordinateMapper.ConvertInputToScreen(_tracker.input, target.center, ref screenPos);
                //CoordinateMapper.ConvertInputToWorld(Camera.main, _tracker.input, target.center, ref ObjectPosition);
                
            }
            worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            stickModel.transform.SetPositionAndRotation(new Vector3(-worldPos.x,worldPos.y,transform.position.z) , Quaternion.identity);
        }
    }
}
