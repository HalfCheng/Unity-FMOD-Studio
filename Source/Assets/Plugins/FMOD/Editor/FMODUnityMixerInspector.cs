#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FMODUnityMixer))]
public class FMODUnityMixerInspector : Editor {
	private FMODUnityMixer unityMixer;

	private void OnEnable() {
		unityMixer = (FMODUnityMixer)target;
		unityMixer.LoadMixerBuses();
	}

	/// <summary>
	/// Called when [inspector GUI].
	/// </summary>
	public override void OnInspectorGUI() {
		serializedObject.Update();

		List<FMODUnityMixer.MixerProperty> mixedProperties = unityMixer.MixerProperties;

		if (mixedProperties.Count > 0) {
			EditorGUILayout.Space();
			foreach (FMODUnityMixer.MixerProperty mixedProperty in mixedProperties) {
				mixedProperty.IsEnabled = GUILayout.Toggle(mixedProperty.IsEnabled, mixedProperty.BusName);
				mixedProperty.Volume = EditorGUILayout.Slider(mixedProperty.Volume, 0, 2);
			}
			EditorGUILayout.Space();
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
