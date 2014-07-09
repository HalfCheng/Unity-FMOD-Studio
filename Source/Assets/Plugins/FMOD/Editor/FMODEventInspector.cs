using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FMOD.Studio;

[CustomEditor(typeof(FMODAsset))]
public class FMODEventInspector : Editor {
	FMODAsset currentAsset; // Make an easy shortcut to the Dialogue your editing

	private class Param {
		public ParameterDescription description;
		public float value;
	}

	List<Param> parameters = new List<Param>(10);

	private void Awake() {
		currentAsset = (FMODAsset)target;
		FMODEditorExtension.StopEvent();

		// Seting up parameters
		FMOD.Studio.EventDescription eventDescription = FMODEditorExtension.GetEventDescription(currentAsset.id);

		if (eventDescription == null) {
			return;
		}

		int count;
		eventDescription.GetParameterCount(out count);

		// Fetch parameters
		parameters.Clear();
		for (int i = 0; i < count; ++i) {
			Param parameter = new Param();
			eventDescription.GetParameterByIndex(i, out parameter.description);
			parameter.value = parameter.description.minimum;
		}
	}

	private void OnDestroy() {
		FMODEditorExtension.StopEvent();
	}

	public override void OnInspectorGUI() {
		EditorGUILayout.LabelField("Path:", currentAsset.path);
		//EditorGUILayout.LabelField("GUID:", currentAsset.id);
		EditorGUILayout.Space();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Play", new GUILayoutOption[0])) {
			FMODEditorExtension.AuditionEvent(currentAsset);
		}
		if (GUILayout.Button("Stop", new GUILayoutOption[0])) {
			FMODEditorExtension.StopEvent();
		}
		GUILayout.EndHorizontal();



		// Display Parameters
		if (parameters.Count > 0) { EditorGUILayout.Space(); }
		foreach (Param param in parameters) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(param.description.name);
			param.value = GUILayout.HorizontalSlider(param.value, param.description.minimum, param.description.maximum, new GUILayoutOption[0]);
			//FMODEditorExtension.SetEventParameterValue(i, param.value);
			GUILayout.EndHorizontal();
		}
	}
}
