#if FMOD_LIVEUPDATE
#  define RUN_IN_BACKGROUND
#endif

using FMOD.Studio;
using UnityEngine;

[AddComponentMenu("FMOD Systems/Audio Listener")]
public class FMODListener : MonoBehaviour {
	#region Constants
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// The instance's logger.
	/// </summary>
	private static readonly FMODLogger Logger = new FMODLogger("FMOD Listener");
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Fields & Properites
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// The cached rigid body.
	/// </summary>
	private Rigidbody cachedRigidBody;

	/// <summary>
	/// The studio system.
	/// </summary>
	private FMODStudioSystem studioSystem;
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Singleton Instance
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Gets the instance.
	/// </summary>
	/// <value>
	/// The instance.
	/// </value>
	public static FMODListener Instance {
		get {
			if (sInstance == null) {
				sInstance = Component.FindObjectOfType<FMODListener>();
				if (sInstance == null) {
					sInstance = Camera.main.gameObject.AddComponent<FMODListener>();
				}
				sInstance.Initialize();
			}
			return sInstance;
		}
	}
	private static FMODListener sInstance;
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Initialization
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Awakes this instance.
	/// </summary>
	private void Awake() {
		this.Initialize();
	}

	/// <summary>
	/// Starts this instance.
	/// </summary>
	private void Start() {
#if UNITY_EDITOR && RUN_IN_BACKGROUND
		Application.runInBackground = true; // Prevent execution pausing when editor loses focus
#endif
	}

	/// <summary>
	/// Initializes this instance.
	/// </summary>
	private void Initialize() {
		cachedRigidBody = this.rigidbody;
		studioSystem = FMODStudioSystem.Instance;
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Update Methods
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Updates this instance.
	/// </summary>
	private void Update() {
		Update3DAttributes();
	}

	/// <summary>
	/// Update3s the d attributes.
	/// </summary>
	private void Update3DAttributes() {
		//FMOD.Studio.System studioSystem = FMODStudioSystem.Instance.System;

		//if (studioSystem.System != null && studioSystem.System.IsValid()) {
			FMOD3DAttributes attributes = UnityUtil.To3DAttributes(gameObject, cachedRigidBody);
			Logger.ErrorCheck(studioSystem.System.SetListenerAttributes(attributes));
		//}
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion
}
