using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Vexpot;
using Vexpot.Integration;
using UnityEditorInternal;

/// <summary>
/// Custom ColorTracker Unity inspector.
/// </summary>
[CustomEditor(typeof(ColorTrackerPanel))]
public class ColorTrackerPanelEditor : Editor
{
    private ReorderableList list;

    private ColorTrackerPanel _content;
    private List<ColorTarget> _colorTargets;
    private int _targetsChanged = 0;

    private static GUIContent enableColorTrackDescription = new GUIContent("Enable Color Track", "Enables/Disables color tracking");
    private static GUIContent pointSpacingDescription = new GUIContent("Point Spacing", "The amount of pixels to increase during color segmentation");
    private static GUIContent enableColorMapDescription = new GUIContent("Enable Color Map", "By enabling this option the tracker will also compute a color map");
    private static GUIContent playOnAwakeDescription = new GUIContent("Play on Awake", "By enabling this option the tracker will automatically start");
    private static GUIContent kalmanDescription = new GUIContent("Use Kalman Filter", "By enabling this option the tracker will reduce the jitter in the targets tracks");
    private static GUIContent colorToleranceDescription = new GUIContent("Tolerance", "Defines the allowed color variance during the color tracking against multiple lighting conditions and other factors like motion blur");

    private string colorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    private void OnEnable()
    {
        list = new ReorderableList(serializedObject,
               serializedObject.FindProperty("colorTargets"),
               false, true, true, true);

        list.drawHeaderCallback = HeaderCallbackDelegate;
        list.drawElementCallback = DrawElementDelegate;
        list.onChangedCallback = OnChangedHandler;
    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        _targetsChanged = 0;
        _content = (ColorTrackerPanel)target;        
        _colorTargets = _content.colorTargets;
      
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        GUI.enabled = !Application.isPlaying;
        _content.inputType = (InputType)EditorGUILayout.EnumPopup("Input", _content.inputType);
        _content.accuracy = (TrackerAccuracy) EditorGUILayout.EnumPopup("Accuracy", _content.accuracy);

        GUI.enabled = true;
        _content.enableColorTrack = EditorGUILayout.Toggle(enableColorTrackDescription, _content.enableColorTrack);

        if(_content.enableColorTrack)
        _content.useKalmanFilter = EditorGUILayout.Toggle(kalmanDescription, _content.useKalmanFilter);

        _content.enableColorMap = EditorGUILayout.Toggle(enableColorMapDescription, _content.enableColorMap);

        if(_content.enableColorMap)
        _content.colorMapPointSpacing = EditorGUILayout.IntField(pointSpacingDescription, _content.colorMapPointSpacing);

        GUI.enabled = (_content.enableColorTrack || _content.enableColorMap);
        _content.playOnAwake = EditorGUILayout.Toggle(playOnAwakeDescription, _content.playOnAwake);
       
        EditorGUILayout.Separator();

        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();     

        EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);
       
        GUILayout.BeginHorizontal();

        ColorTracker tracker = _content.GetColorTracker();
        bool controlsEnabled = tracker != null;

        GUI.enabled = controlsEnabled && !tracker.isRunning;
        if (GUILayout.Button("Start", GUILayout.Height(30)))
        {
            _content.StartColorTracker();
        }

        GUI.enabled = controlsEnabled && tracker.isRunning;
        if (GUILayout.Button("Stop", GUILayout.Height(30)))
        {
            _content.StopColorTracker();
        }

        GUI.enabled = _colorTargets.Count > 0;
        if (GUILayout.Button("Remove all targets", GUILayout.Height(30)))
        {
            _colorTargets.Clear();
            OnChangedHandler(null);
        }

        if(_targetsChanged > 0 && Application.isPlaying)
        {
           _content.UpdateColorTargets();
        }

        GUILayout.EndHorizontal();
        GUI.enabled = true;
        EditorGUILayout.Separator();

        if (_colorTargets.Count == 0)
        {
            EditorGUILayout.HelpBox("You must add at least one target!", MessageType.Error);
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(target);
    }

    void OnChangedHandler(ReorderableList list)
    {
        _targetsChanged++;       
    }

    void DrawElementDelegate(Rect rect, int index, bool isActive, bool isFocused)
    {
        Color32 targetObject = _colorTargets[index].color;

        rect.height = EditorGUIUtility.singleLineHeight;
        float spacing = rect.width * 0.01f;
        float colorWidth = rect.width * 0.20f;
        float toleranceWidth = rect.width * 0.40f;

        EditorGUI.BeginChangeCheck();
        rect.width = colorWidth;
        EditorGUI.LabelField(rect, "#" + colorToHex(targetObject));
        rect.x += colorWidth;
        rect.width = colorWidth;
        _colorTargets[index].color = EditorGUI.ColorField(rect, new GUIContent(""), targetObject, true, false, false, null);
        rect.x += colorWidth + spacing;
        rect.width = colorWidth;
        EditorGUI.LabelField(rect, colorToleranceDescription);
        rect.x += colorWidth - spacing;
        rect.width = toleranceWidth;
        _colorTargets[index].tolerance = EditorGUI.Slider(rect,"", _colorTargets[index].tolerance, 10.0f, 50.0f);

        if (EditorGUI.EndChangeCheck())
        {
            OnChangedHandler(null);
        }
    }

    void HeaderCallbackDelegate(Rect rect)
    {
        EditorGUI.LabelField(rect, "Color Targets (" + _colorTargets.Count + ")", EditorStyles.boldLabel);
    }
}
