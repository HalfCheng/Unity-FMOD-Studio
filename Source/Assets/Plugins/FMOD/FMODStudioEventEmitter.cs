using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using FMOD.Studio;

[AddComponentMenu("FMOD Systems/Studio Event Emitter")]
public class FMODStudioEventEmitter : MonoBehaviour {
	#region Parameter Class
	// ----------------------------------------------------------------------------------------------------
	[System.Serializable]
	public struct Parameter {
		public string name;
		public float value;
	} 
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Constants
	// ----------------------------------------------------------------------------------------------------
	private static bool IsShuttingDown = false; 
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Fields & Properties
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// The instance's logger.
	/// </summary>
	private readonly FMODLogger Logger;

	public FMODAsset asset;
	public string path = "";
	public bool startEventOnAwake = true;

	FMOD.Studio.EventInstance evt;
	bool hasStarted = false;

	Rigidbody cachedRigidBody; 
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Initialization
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Initializes a new instance of the <see cref="FMODStudioEventEmitter"/> class.
	/// </summary>
	public FMODStudioEventEmitter() {
		this.Logger = new FMODLogger("FMOD Studio Event Emitter", this);
	}

	/// <summary>
	/// Starts this instance.
	/// </summary>
	private void Start() {
		CacheEventInstance();

		cachedRigidBody = rigidbody;

		if (startEventOnAwake) {
			StartEvent();
		}
	}

	/// <summary>
	/// Starts the event.
	/// </summary>
	public void StartEvent() {
		if (evt == null || !evt.IsValid()) {
			CacheEventInstance();
		}

		// Attempt to release as oneshot
		if (evt != null && evt.IsValid()) {
			Update3DAttributes();
			Logger.ErrorCheck(evt.Start());
		} else {
			Logger.LogError("Event retrieval failed: " + path);
		}

		hasStarted = true;
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Audio Playing Methods
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Plays the associated audio instance.
	/// </summary>
	public void Play() {
		if (evt != null) {
			Logger.ErrorCheck(evt.Start());
		} else {
			Logger.Log("Tried to play event without a valid instance: " + path);
			return;
		}
	}

	/// <summary>
	/// Stops the associated audio instance.
	/// </summary>
	public void Stop() {
		if (evt != null) {
			Logger.ErrorCheck(evt.Stop(StopNode.Immediate));
		}
	}

	/// <summary>
	/// Gets the parameter.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <returns></returns>
	public FMOD.Studio.ParameterInstance GetParameter(string name) {
		FMOD.Studio.ParameterInstance param = null;
		Logger.ErrorCheck(evt.GetParameter(name, out param));

		return param;
	}

	/// <summary>
	/// Gets the state of the playback.
	/// </summary>
	/// <returns></returns>
	public FMOD.Studio.PlaybackState GetPlaybackState() {
		if (evt == null || !evt.IsValid())
			return FMOD.Studio.PlaybackState.Stopped;

		FMOD.Studio.PlaybackState state = PlaybackState.Idle;

		if (Logger.ErrorCheck(evt.GetPlaybackState(out state))) {
			return state;
		}

		return FMOD.Studio.PlaybackState.Stopped;
	}

	/// <summary>
	/// Caches the event instance.
	/// </summary>
	private void CacheEventInstance() {
		if (asset != null) {
			evt = FMODStudioSystem.Instance.GetEvent(asset.id);
		} else if (!String.IsNullOrEmpty(path)) {
			evt = FMODStudioSystem.Instance.GetEvent(path);
		} else {
			Logger.LogError("No asset or path specified for Event Emitter");
		}
	}

	/// <summary>
	/// Determines whether this instance has finished.
	/// </summary>
	/// <returns></returns>
	public bool HasFinished() {
		if (!hasStarted) {
			return false;
		}
		if (evt == null || !evt.IsValid()) {
			return true;
		}

		return GetPlaybackState() == FMOD.Studio.PlaybackState.Stopped;
	} 
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Updating Methods
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Updates this instance.
	/// </summary>
	private void Update() {
		if (evt != null && evt.IsValid()) {
			Update3DAttributes();
		} else {
			evt = null;
		}
	}

	/// <summary>
	/// Updates the 3D attributes.
	/// </summary>
	private void Update3DAttributes() {
		if (evt != null && evt.IsValid()) {
			FMOD3DAttributes attributes = UnityUtil.To3DAttributes(gameObject, cachedRigidBody);
			Logger.ErrorCheck(evt.Set3DAttributes(attributes));
		}
	} 
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Closing & Destroying
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Called when [application quit].
	/// </summary>
	private void OnApplicationQuit() {
		IsShuttingDown = true;
	}

	/// <summary>
	/// Called when [destroy].
	/// </summary>
	private void OnDestroy() {
		if (IsShuttingDown)
			return;

		Logger.Log("Destroy called");
		if (evt != null && evt.IsValid()) {
			if (GetPlaybackState() != FMOD.Studio.PlaybackState.Stopped) {
				Logger.Log("Release evt: " + path);
				Logger.ErrorCheck(evt.Stop(FMOD.Studio.StopNode.Immediate));
			}

			Logger.ErrorCheck(evt.Release());
			evt = null;
		}
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion
}
