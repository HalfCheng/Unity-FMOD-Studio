using UnityEngine;
using System.Collections;
using FMOD.Studio;
using System.Runtime.InteropServices;

public class Test : MonoBehaviour {
	//private FMODStudioEventEmitter emitter;
	private EventInstance eventBGM;
	private EventInstance eventFire;
	private EventDescription eventFireDesc;
	private ParameterInstance parameterLoop;

	// Use this for initialization
	private void Start() {
		//this.emitter = this.GetComponent<FMODStudioEventEmitter>();
		this.eventBGM = FMODStudioSystem.Instance.GetEvent("event:/Music/Music");
		this.eventBGM.SetVolume(0.5f);
		this.eventBGM.Start();

		this.eventFireDesc = FMODStudioSystem.Instance.GetEventDescription("event:/Weapons/Full Auto Loop");
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			StartCoroutine("FireSounds");
		}

		if (Input.GetMouseButtonUp(0)) {
			StopCoroutine("FireSounds");
		}

		if (Input.GetMouseButtonDown(1)) {
			this.eventBGM.Stop(StopNode.AllowFadeOut);
		}

		//float outVal;
		//this.parameterLoop.GetValue(out outVal);
	}

	private IEnumerator FireSounds() {
		while (true) {
			this.eventFireDesc.CreateInstance(out this.eventFire);
			this.eventFire.Start();
			this.eventFire.Release();

			yield return new WaitForSeconds(0.05f);
		}
	}
}
