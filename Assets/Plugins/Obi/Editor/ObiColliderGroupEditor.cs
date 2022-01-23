using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi{
	
	/**
	 * Custom inspector for ObiColliderGroup. 
	 */
	
	[CustomEditor(typeof(ObiColliderGroup))] 
	public class ObiColliderGroupEditor : Editor
	{
		
		ObiColliderGroup group;
		
		public void OnEnable(){
			group = (ObiColliderGroup)target;
		}
		
		public override void OnInspectorGUI() {
			
			serializedObject.UpdateIfRequiredOrScript();
			
			Editor.DrawPropertiesExcluding(serializedObject,"m_Script");
			
			// Apply changes to the serializedProperty
			if (GUI.changed){
				
				serializedObject.ApplyModifiedProperties();
		
			}
			
		}
		
	}
}

