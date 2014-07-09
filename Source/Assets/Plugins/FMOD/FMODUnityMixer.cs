using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("FMOD Systems/Master Mixer")]
public class FMODUnityMixer : MonoBehaviour {
	#region Mixer Property Class
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Manages MixerStrips.
	/// </summary>
	[System.Serializable]
	public class MixerProperty : IComparable<MixerProperty> {
		private const string MasterBusName = "Master";

		#region Fields & Properties
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the mixer strip.
		/// </summary>
		/// <value>
		/// The mixer strip.
		/// </value>
		public FMOD.Studio.MixerStrip MixerStrip {
			get {
				if (this.mixerStrip == null) {
					FMODUnityMixer.Instance.LoadMixerBuses();
				}
				return this.mixerStrip;
			}
			set {
				this.mixerStrip = value;
			}
		}
		[SerializeField]
		private FMOD.Studio.MixerStrip mixerStrip;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is enabled.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool IsEnabled {
			get { return this.isEnabled; }
			set {
				this.isEnabled = value;
				if (this.mixerStrip != null) {
					this.MixerStrip.SetMute(!this.isEnabled);
				}

			}
		}
		[SerializeField]
		private bool isEnabled;

		/// <summary>
		/// Gets the name of the bus.
		/// </summary>
		/// <value>
		/// The name of the bus.
		/// </value>
		public string BusName {
			get { return this.busName; }
		}
		[SerializeField]
		private string busName;

		/// <summary>
		/// Gets or sets the volume.
		/// </summary>
		/// <value>
		/// The volume.
		/// </value>
		public float Volume {
			get { return this.volume; }
			set {
				this.volume = value;
				if (this.mixerStrip != null) {
					this.MixerStrip.SetFaderLevel(this.volume);
				}
			}
		}
		[SerializeField]
		private float volume;
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Initialization
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="MixerProperty"/> class.
		/// </summary>
		/// <param name="mixerStrip">The mixer strip.</param>
		/// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
		/// <param name="busName">Name of the bus.</param>
		/// <param name="volume">The volume.</param>
		public MixerProperty(FMOD.Studio.MixerStrip mixerStrip, bool isEnabled, string busName, float volume) {
			this.mixerStrip = mixerStrip;

			this.isEnabled = isEnabled;
			this.busName = busName;
			this.volume = volume;
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Comparison
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Compares to.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public int CompareTo(MixerProperty other) {
			// Always bring Master on top
			if (this.BusName.Equals(MasterBusName)) { return -1; }
			if (other.BusName.Equals(MasterBusName)) { return 1; }

			return BusName.CompareTo(other.BusName);
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Fields & Properties
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Gets the mixer properties.
	/// </summary>
	/// <value>
	/// The mixer properties.
	/// </value>
	public List<MixerProperty> MixerProperties {
		get {
			return this.mixerProperties;
		}
	}

	[SerializeField]
	private List<MixerProperty> mixerProperties;
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
	public static FMODUnityMixer Instance {
		get {
			if (sInstance == null) {
				sInstance = Component.FindObjectOfType<FMODUnityMixer>();
				if (sInstance == null) {
					GameObject go = new GameObject("FMOD StudioSystem");
					sInstance = go.AddComponent<FMODUnityMixer>();
				}
				sInstance.LoadMixerBuses();
			}
			return sInstance;
		}
	}
	private static FMODUnityMixer sInstance;
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Initialization
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Awakes this instance.
	/// </summary>
	private void Awake() {
		LoadMixerBuses();
		//StartCoroutine(UpdateBusFaders());
	}

	/// <summary>
	/// Loads the mixer buses.
	/// </summary>
	[ContextMenu("Reload Mixer")]
	public void LoadMixerBuses() {
		FMOD.Studio.MixerStrip[] mixerStrips;
		FMOD.Studio.Bank bank;
		FMOD.Studio.MixerStrip mixerStrip;

		FMODStudioSystem studioSystem = FMODStudioSystem.Instance;

		FMOD.GUID guid = new FMOD.GUID();

		if (studioSystem.System == null) {
			Debug.LogError("Cannot find Studio System.");
			return;
		}

		studioSystem.System.LookupID("bank:/Master Bank", out guid);
		studioSystem.System.GetBank(guid, out bank);

		if (bank == null) {
			Debug.LogError("Cannot find Master Bank.");
			return;
		}

		bank.GetMixerStripList(out mixerStrips);

		Dictionary<String, MixerProperty> mixerDictionary = new Dictionary<string, MixerProperty>(mixerStrips.Length);
		if (mixerProperties == null) {
			mixerProperties = new List<MixerProperty>(mixerStrips.Length);
		} else {
			foreach (MixerProperty property in this.mixerProperties) {
				mixerDictionary.Add(property.BusName, property);
			}
		}

		string path;
		bool muteLevel;
		float faderLevel;
		bool mixerListExists = this.mixerProperties != null;
		for (int i = 0; i < mixerStrips.Length; i++) {
			mixerStrips[i].GetPath(out path);
			studioSystem.System.LookupID(path, out guid);
			studioSystem.System.GetMixerStrip(guid, FMOD.Studio.LoadingMode.BeginNow, out mixerStrip);
			mixerStrip.GetMute(out muteLevel);
			mixerStrip.GetFaderLevel(out faderLevel);

			if (path == "bus:/") {
				path = "Master";
			} else {
				path = path.Replace("bus:/", string.Empty);
			}

			if (mixerListExists) {
				MixerProperty property = mixerDictionary[path];
				property.MixerStrip = mixerStrip;
				property.Volume = faderLevel;
				property.IsEnabled = !muteLevel;
			} else {
				this.mixerProperties.Add(new MixerProperty(mixerStrip, !muteLevel, path, faderLevel));
			}
		}

		this.mixerProperties.Sort();
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion
}
