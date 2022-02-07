using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HeadTracking))]
public class HeadTrackingEditor : Editor
{
    #region Variables.

    private HeadTracking _target;
    private string[] _typeOptions = {"Custom", "Animation Rigging"};

    #endregion

    #region Methods.

    

    #endregion

    #region Life-cycle Callbacks.

    public override void OnInspectorGUI()
    {
        _target = (HeadTracking) target;
        
        serializedObject.Update();
        {
            _target.type = EditorGUILayout.Popup("Tracking Type", _target.type, _typeOptions);
            if (_target.type == 1)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimConstraint"));
            }
            else if (_target.type == 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("headBone"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAngle"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("trackingSpeed"));
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    #endregion
}
