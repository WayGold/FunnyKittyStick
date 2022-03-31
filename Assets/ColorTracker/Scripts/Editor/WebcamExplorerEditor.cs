using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Vexpot.Integration;

[CustomEditor(typeof(VideoCaptureRenderer))]
public class WebcamExplorerEditor : Editor {

    private List<string> deviceNames;

    private VideoCaptureRenderer content;

    public override void OnInspectorGUI()
    {
        content = (VideoCaptureRenderer)target; 
        
        if (deviceNames == null)
            deviceNames = new List<string>();

        deviceNames.Clear();
      
       // DrawDefaultInspector();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Device Name");

        foreach (var d in WebCamTexture.devices) {
            deviceNames.Add(d.name);
        }

        if (WebCamTexture.devices.Length == 1)
            content.deviceIndex = 0;

        EditorGUI.BeginChangeCheck();

        content.deviceIndex = EditorGUILayout.Popup(content.deviceIndex, deviceNames.ToArray());
                   
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(target);
    } 
	
}
