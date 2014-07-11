using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using CSystem = System;
using System;

[AddComponentMenu("FMOD Systems/Studio System")]
public class FMODStudioSystem : MonoBehaviour {
	#region Constants
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// The instance's logger.
	/// </summary>
	private static readonly FMODLogger Logger = new FMODLogger("FMOD Studio System");
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Enumerations
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Determins FMOD's DSP Buffer size
	/// </summary>
	public enum BufferSizeMode : byte {
		VeryLow = 8,
		Low = 4,
		Normal = 2,
		High = 1
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Fields & Properties
	// ----------------------------------------------------------------------------------------------------
	#region Exposed Variables
	/// <summary>
	/// The DSP's buffer size.
	/// </summary>
	[SerializeField]
	private BufferSizeMode bufferSize = BufferSizeMode.Normal;

	/// <summary>
	/// The plugin paths
	/// </summary>
	[SerializeField]
	private string[] pluginPaths;
	#endregion

	#region Public Properties
	/// <summary>
	/// Gets the FMOD System.
	/// </summary>
	/// <value>
	/// The system.
	/// </value>
	public FMOD.Studio.System System {
		get { return system; }
	}

	/// <summary>
	/// Gets the FMOD Low Level System.
	/// </summary>
	public FMOD.System LowLevelSystem {
		get { return lowLevelSystem; }
	}

	/// <summary>
	/// Gets a list of loaded FMOD Studio Banks.
	/// </summary>
	/// <value>
	/// The loaded banks.
	/// </value>
	public List<FMOD.Studio.Bank> LoadedBanks {
		get {
			if (this.loadedBanks == null) {
				this.LoadAllBanks();
			}
			return this.loadedBanks;
		}
	}
	#endregion

	#region Private Fields
	/// <summary>
	/// Gets or sets the FMOD System.
	/// </summary>
	private FMOD.Studio.System system;

	/// <summary>
	/// Gets or sets FMOD's low level System.
	/// </summary>
	private FMOD.System lowLevelSystem;

	/// <summary>
	/// Gets or sets a list of loaded banks.
	/// </summary>
	private List<FMOD.Studio.Bank> loadedBanks;

	/// <summary>
	/// The event descriptions.
	/// </summary>
	private Dictionary<string, FMOD.Studio.EventDescription> eventDescriptions = new Dictionary<string, FMOD.Studio.EventDescription>();

	/// <summary>
	/// Gets the plugin path.
	/// </summary>
	/// <value>
	/// The plugin path.
	/// </value>
	private string PluginPath {
		get {
			if (Application.platform == RuntimePlatform.WindowsEditor) {
				return Application.dataPath + "/Plugins/x86";
			} else if (Application.platform == RuntimePlatform.WindowsPlayer ||
					   Application.platform == RuntimePlatform.OSXEditor ||
					   Application.platform == RuntimePlatform.OSXPlayer ||
					   Application.platform == RuntimePlatform.OSXDashboardPlayer ||
					   Application.platform == RuntimePlatform.LinuxPlayer
#if PLATFORM_PS4
				     || Application.platform == RuntimePlatform.PS4
#endif
#if UNITY_XBOXONE
				     || Application.platform == RuntimePlatform.XboxOne
#endif
) {
				return Application.dataPath + "/Plugins";
			} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
				Logger.LogError("Plugins not currently supported on iOS, contact support@fmod.org for more information");
				return "";
			} else if (Application.platform == RuntimePlatform.Android) {
				var dirInfo = new System.IO.DirectoryInfo(Application.persistentDataPath);
				string packageName = dirInfo.Parent.Name;
				return "/data/data/" + packageName + "/lib";
			}

			Logger.LogError("Unknown platform!");
			return "";
		}
	}

	/// <summary>
	/// Gets or sets whether or not this System is already initialized.
	/// </summary>
	private bool isInitialized = false;
	#endregion
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
	public static FMODStudioSystem Instance {
		get {
			if (sInstance == null) {
				sInstance = Component.FindObjectOfType<FMODStudioSystem>();
				if (sInstance == null) {
					GameObject go = new GameObject("FMOD StudioSystem");
					sInstance = go.AddComponent<FMODStudioSystem>();
				}
			}
			sInstance.Initialize();
			return sInstance;
		}
	}
	private static FMODStudioSystem sInstance;
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Initialization
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Awakes this instance.
	/// </summary>
	private void Awake() {
		FMODStudioSystem.Instance.Initialize();
	}

	/// <summary>
	/// Initializes this instance.
	/// </summary>
	private void Initialize() {
		if (isInitialized && this.system != null && this.system.IsValid()) {
			return;
		}
		isInitialized = true;

		Logger.LogMessage("Initializing FMOD");

		// Do these hacks before calling ANY fmod functions!
		if (!UnityUtil.ForceLoadLowLevelBinary()) {
			Logger.LogError("Unable to load low level binary!");
		}

		Logger.LogMessage("Creating FMOD System");
		Logger.ErrorCheck(FMOD.Studio.System.Create(out system));

		if (system == null) {
			Logger.LogError("Unable to create FMOD System!");
			return;
		}

		Logger.LogMessage("System.initialize (" + system + ")");
		FMOD.Studio.InitFlags flags = FMOD.Studio.InitFlags.Normal;
#if FMOD_LIVEUPDATE
		flags |= FMOD.Studio.INITFLAGS.LIVEUPDATE;
#endif

		FMOD.Result result = FMOD.Result.Ok;

		system.GetLowLevelSystem(out lowLevelSystem);
		UpdateDSPBufferSize();

		result = system.Initialize(1024, flags, FMOD.InitFlags.Normal, global::System.IntPtr.Zero);

		if (result == FMOD.Result.ErrorNetSocket) {
#if false && FMOD_LIVEUPDATE
			UnityUtil.LogWarning("LiveUpdate disabled: socket in already in use");
			flags &= ~FMOD.Studio.INITFLAGS.LIVEUPDATE;
        	result = system.init(1024, flags, FMOD.INITFLAGS.NORMAL, (System.IntPtr)null);
#else
			Logger.LogError("Unable to initalize with LiveUpdate: socket is already in use");
#endif
		} else if (result == FMOD.Result.ErrorHeaderMismatch) {
			Logger.LogError("Version mismatch between C# script and FMOD binary, restart Unity and reimport the integration package to resolve this issue.");
		} else {
			Logger.ErrorCheck(result);
		}


		LoadPlugins();

		LoadAllBanks();
	}

	/// <summary>
	/// Updates the size of the DSP buffer.
	/// </summary>
	private void UpdateDSPBufferSize() {
		int bufferSizeMode = (int)this.bufferSize;
		uint bufferSize = (uint)(512 / bufferSizeMode);
		int bufferCount = 2 * bufferSizeMode;

		Logger.LogMessage("Changed DSP Buffer Size to " + bufferSize + " ms.");
		lowLevelSystem.SetDSPBufferSize(bufferSize, bufferCount);
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Loading Plugins
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Loads the plugins.
	/// </summary>
	private void LoadPlugins() {
		if (pluginPaths == null || pluginPaths.Length == 0) { return; }

		Logger.LogMessage("Loading Plugins.");
		FMOD.System fmodSystem = null;
		Logger.ErrorCheck(FMODStudioSystem.Instance.System.GetLowLevelSystem(out fmodSystem));

		string dir = PluginPath;
		foreach (var name in pluginPaths) {
			string path = dir + "/" + GetPluginFileName(name);

			Logger.LogMessage("Loading plugin: " + path);
			if (!CSystem.IO.File.Exists(path)) {
				Logger.LogWarning("Plugin not found: " + path);
			}

			uint handle;
			Logger.ErrorCheck(fmodSystem.LoadPlugin(path, out handle));
		}
	}

	/// <summary>
	/// Gets the name of the plugin file.
	/// </summary>
	/// <param name="rawName">Name of the raw.</param>
	/// <returns></returns>
	private string GetPluginFileName(string rawName) {
		if (Application.platform == RuntimePlatform.WindowsEditor ||
			Application.platform == RuntimePlatform.WindowsPlayer
#if UNITY_XBOXONE
		    || Application.platform == RuntimePlatform.XboxOne
#endif
) {
			return rawName + ".dll";
		} else if (Application.platform == RuntimePlatform.OSXEditor ||
				   Application.platform == RuntimePlatform.OSXPlayer ||
				   Application.platform == RuntimePlatform.OSXDashboardPlayer) {
			return rawName + ".dylib";
		} else if (Application.platform == RuntimePlatform.Android ||
				   Application.platform == RuntimePlatform.LinuxPlayer) {
			return "lib" + rawName + ".so";
		}
#if PLATFORM_PS4
		else if (Application.platform == RuntimePlatform.PS4)
		{
			return rawName + ".prx";
		}
#endif

		Logger.LogError("Unknown platform!");
		return "";
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Bank & Asset Methods
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Loads the banks.
	/// </summary>
	public bool LoadAllBanks(bool force = false) {
		FMODBankList bankListAsset = FMODBankList.LoadBankList();

		if (!force) {
			int bankCount;
			FMODStudioSystem.Instance.System.GetBankCount(out bankCount);
			if (bankListAsset != null && bankListAsset.BankList.Count == bankCount) {
				return true;
			}
		}

		if (!bankListAsset) {
			Logger.LogError("FMOD Bank List not found, no banks loaded.");
		} else {
			Logger.LogMessage("Loading " + bankListAsset.BankList.Count + " Banks");

			if (this.loadedBanks == null) {
				this.loadedBanks = new List<Bank>(bankListAsset.BankList.Count);
			} else {
				this.loadedBanks.Clear();
			}

			foreach (var bankName in bankListAsset.BankList) {
				Logger.LogMessage("Loading Bank \"" + bankName + "\"");
				if (!LoadBank(bankName)) {
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Loads the bank.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	public bool LoadBank(string fileName) {
		string bankPath = GetStreamingAsset(fileName);

		FMOD.Studio.Bank bank = null;
		FMOD.Result result = FMODStudioSystem.Instance.System.LoadBankFile(bankPath, LoadBankFlags.Normal, out bank);
		if (result == FMOD.Result.ErrorVersion) {
			Logger.LogError("These banks were built with an incompatible version of FMOD Studio.");
			return false;
		}
		if (result != FMOD.Result.Ok) {
			Logger.LogError("An error occured while loading bank " + fileName + ": " + result.ToString() + "\n  " + FMOD.Error.String(result));
			return false;
		}

		Logger.LogMessage("Bank Load \"" + fileName + "\": " + (bank != null ? "Succeeded!" : "Failed!"));

		this.loadedBanks.Add(bank);

		return true;
	}

	/// <summary>
	/// Unloads all banks.
	/// </summary>
	public void UnloadAllBanks() {
		foreach (var bank in LoadedBanks) {
			Logger.ErrorCheck(bank.Unload());
		}

		eventDescriptions.Clear();
		loadedBanks.Clear();
	}

	/// <summary>
	/// Gets the streaming asset.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns></returns>
	private string GetStreamingAsset(string fileName) {
		string bankPath = "";
		if (Application.platform == RuntimePlatform.WindowsEditor ||
			Application.platform == RuntimePlatform.OSXEditor ||
			Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.LinuxPlayer
#if PLATFORM_PS4
		    || Application.platform == RuntimePlatform.PS4
#endif
#if UNITY_XBOXONE
			|| Application.platform == RuntimePlatform.XboxOne
#endif
) {
			bankPath = Application.dataPath + "/StreamingAssets";
		} else if (Application.platform == RuntimePlatform.OSXPlayer ||
			  Application.platform == RuntimePlatform.OSXDashboardPlayer) {
			bankPath = Application.dataPath + "/Data/StreamingAssets";
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			bankPath = Application.dataPath + "/Raw";
		} else if (Application.platform == RuntimePlatform.Android) {
			bankPath = "jar:file://" + Application.dataPath + "!/assets";
		} else {
			Logger.LogError("Unknown platform!");
			return "";
		}

		string assetPath = bankPath + "/" + fileName;

#if UNITY_ANDROID && !UNITY_EDITOR
		// Unpack the compressed JAR file
		string unpackedJarPath = Application.persistentDataPath + "/" + fileName;
		
		Logger.Log("Unpacking bank from JAR file into:" + unpackedJarPath);
		
		if (File.Exists(unpackedJarPath)) {
			Logger.Log("File already unpacked!");
			File.Delete(unpackedJarPath);
			
			if (File.Exists(unpackedJarPath)) {
				Logger.Log("Could NOT delete!");				
			}
		}
		
		WWW dataStream = new WWW(assetPath);
		
		while(!dataStream.isDone) {} // FIXME: not safe
		
		
		if (!String.IsNullOrEmpty(dataStream.error)) {
			Logger.LogError("WWW Error in Data Stream:" + dataStream.error);
		}
		
		Logger.Log("Android unpacked jar path: " + unpackedJarPath);
		
		File.WriteAllBytes(unpackedJarPath, dataStream.bytes);
		
		//FileInfo fi = new FileInfo(unpackedJarPath);
		//Logger.Log("Unpacked bank size = " + fi.Length);
		
		assetPath = unpackedJarPath;
#endif

		return assetPath;
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Getting Events
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Gets the event.
	/// </summary>
	/// <param name="asset">The asset.</param>
	/// <returns></returns>
	public FMOD.Studio.EventInstance GetEvent(FMODAsset asset) {
		return GetEvent(asset.id);
	}

	/// <summary>
	/// Gets the event.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <returns></returns>
	public FMOD.Studio.EventInstance GetEvent(string path) {
		FMOD.Studio.EventInstance instance = null;

		if (string.IsNullOrEmpty(path)) {
			Logger.LogError("Empty event path!");
			return null;
		}

		if (eventDescriptions.ContainsKey(path)) {
			Logger.ErrorCheck(eventDescriptions[path].CreateInstance(out instance));
		} else {
			FMOD.Studio.EventDescription desc = GetEventDescription(path);
			Logger.ErrorCheck(desc.CreateInstance(out instance));
		}

		if (instance == null) {
			Logger.LogMessage("GetEvent Failed: \"path\"");
		}

		return instance;
	}

	/// <summary>
	/// Gets the event.
	/// </summary>
	/// <param name="eventDescription">The event description.</param>
	/// <returns></returns>
	public FMOD.Studio.EventInstance GetEvent(FMOD.Studio.EventDescription eventDescription) {
		FMOD.Studio.EventInstance instance = null;

		Logger.ErrorCheck(eventDescription.CreateInstance(out instance));

		if (instance == null) {
			Logger.LogMessage("GetEvent Failed: \"path\"");
		}

		return instance;
	}

	/// <summary>
	/// Gets the event description.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <returns></returns>
	public FMOD.Studio.EventDescription GetEventDescription(string path) {
		EventDescription eventDescription;
		if (eventDescriptions.ContainsKey(path)) {
			eventDescription = eventDescriptions[path];
		} else {
			FMOD.GUID id = new FMOD.GUID();

			if (path.StartsWith("{")) {
				Logger.ErrorCheck(FMOD.Studio.Util.ParseID(path, out id));
			} else if (path.StartsWith("event:")) {
				Logger.ErrorCheck(system.LookupID(path, out id));
			} else {
				Logger.LogError("Expected event path to start with 'event:/'");
			}

			Logger.ErrorCheck(system.GetEvent(id, FMOD.Studio.LoadingMode.BeginNow, out eventDescription));

			if (eventDescription != null && eventDescription.IsValid()) {
				eventDescriptions.Add(path, eventDescription);
			} else {
				Logger.LogError("Could not get event " + id + " for " + path);
			}
		}
		return eventDescription;
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Playing Sounds - One Shot
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Plays the one shot.
	/// </summary>
	/// <param name="asset">The asset.</param>
	/// <param name="position">The position.</param>
	public void PlayOneShot(FMODAsset asset, Vector3 position) {
		PlayOneShot(asset.id, position);
	}

	/// <summary>
	/// Plays the one shot.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="position">The position.</param>
	public void PlayOneShot(string path, Vector3 position) {
		PlayOneShot(path, position, 1.0f);
	}

	/// <summary>
	/// Plays the one shot.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="position">The position.</param>
	/// <param name="volume">The volume.</param>
	private void PlayOneShot(FMODAsset asset, Vector3 position, float volume) {
		PlayOneShot(asset.id, position, volume);
	}

	/// <summary>
	/// Plays the one shot.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="position">The position.</param>
	/// <param name="volume">The volume.</param>
	private void PlayOneShot(string path, Vector3 position, float volume) {
		EventInstance instance = GetEvent(path);

		FMOD3DAttributes attributes = UnityUtil.To3DAttributes(position);
		Logger.ErrorCheck(instance.Set3DAttributes(attributes));
		Logger.ErrorCheck(instance.SetVolume(volume));
		Logger.ErrorCheck(instance.Start());
		Logger.ErrorCheck(instance.Release());
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Updating Methods
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Called when application paused.
	/// </summary>
	/// <param name="pauseStatus">if set to <c>true</c> [pause status].</param>
	private void OnApplicationPause(bool pauseStatus) {
		FMOD.System sys;
		Logger.ErrorCheck(system.GetLowLevelSystem(out sys));

		Logger.LogMessage("Pause state changed to: " + pauseStatus);

		if (pauseStatus) {
			Logger.ErrorCheck(sys.MixerSuspend());
		} else {
			Logger.ErrorCheck(sys.MixerResume());
		}
	}

	/// <summary>
	/// Updates this instance.
	/// </summary>
	private void Update() {
		if (isInitialized) {
			Logger.ErrorCheck(system.Update());
		}
	}

	/// <summary>
	/// Called when disabled.
	/// </summary>
	private void OnDisable() {
		if (isInitialized) {
			Logger.LogMessage("Shutting down FMOD System");
			Logger.ErrorCheck(system.Release());
		}
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Unloading & Releasing
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the
	/// <see cref="FMODStudioSystem"/> is reclaimed by garbage collection.
	/// </summary>
	~FMODStudioSystem() {
		//Debug.LogError("Deconstructor: FMODStudioSystem");
		//if (this.system != null) {
		//    Debug.LogError("Releasing FMOD Studio");
		//    this.system.UnloadAll();
		//    this.system.Release();
		//}

		//if (this.lowLevelSystem != null) {
		//    Debug.LogError("Releasing FMOD System");
		//    this.lowLevelSystem.Release();
		//}
	}

	/// <summary>
	/// Unloads this instance.
	/// </summary>
	public void Unload() {
		System.UnloadAll();
		Release();
	}

	/// <summary>
	/// Releases this instance.
	/// </summary>
	public void Release() {
		System.Release();
		this.system = null;
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion
}
