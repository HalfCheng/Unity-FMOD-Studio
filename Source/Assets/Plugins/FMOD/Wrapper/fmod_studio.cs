/* ========================================================================================== */
/* FMOD System - C# Wrapper . Copyright (c), Firelight Technologies Pty, Ltd. 2004-2014.      */
/* Modified by Xane (https://github.com/XaneFeather), Jul 2014                                */
/*                                                                                            */
/*                                                                                            */
/* ========================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace FMOD {
	namespace Studio {
		public class StudioVersion {
#if UNITY_IPHONE && !UNITY_EDITOR
        public const string dll    = "__Internal";
#elif (UNITY_PS4) && !UNITY_EDITOR
		public const string dll    = "libfmodstudio";
#else
			public const string dll = "fmodstudio";
#endif
		}

		public enum LoadingMode {
			BeginNow,
			Prohibited
		}

		public enum StopNode {
			AllowFadeOut,
			Immediate
		}

		public enum LoadingState {
			Unloading,
			Unloaded,
			Loading,
			Loaded
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FMOD3DAttributes {
			public Vector position;
			public Vector velocity;
			public Vector forward;
			public Vector up;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ProgrammerSoundProperties {
			public string name;
			public IntPtr sound;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct AdvancedSettings {
			public int cbSize;               /* [w]   Size of this structure.  NOTE: For C# wrapper, users can leave this at 0. ! */
			public int commandQueueSize;     /* [r/w] Optional. Specify 0 to ignore. Specify the command queue size for studio async processing.  Default 4096 (4kb) */
			public int handleInitialSize;    /* [r/w] Optional. Specify 0 to ignore. Specify the initial size to allocate for handles.  Memory for handles will grow as needed in pages. */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CpuUsage {
			public float dspUsage;            /* Returns the % CPU time taken by DSP processing on the low level mixer thread. */
			public float streamUsage;         /* Returns the % CPU time taken by stream processing on the low level stream thread. */
			public float geometryUsage;       /* Returns the % CPU time taken by geometry processing on the low level geometry thread. */
			public float updateUsage;         /* Returns the % CPU time taken by low level update, called as part of the studio update. */
			public float studioUsage;         /* Returns the % CPU time taken by studio update, called from the studio thread. Does not include low level update time. */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BufferInfo {
			public int currentUsage;          /* Current buffer usage in bytes. */
			public int peakUsage;             /* Peak buffer usage in bytes. */
			public int capacity;              /* Buffer capacity in bytes. */
			public int stallCount;            /* Number of stalls due to buffer overflow. */
			public float stallTime;           /* Amount of time stalled due to buffer overflow, in seconds. */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BufferUsage {
			public BufferInfo studioCommandQueue;      /* Information for the Studio Async Command buffer, controlled by FMOD_STUDIO_ADVANCEDSETTINGS commandQueueSize. */
			public BufferInfo studioHandle;            /* Information for the Studio handle table, controlled by FMOD_STUDIO_ADVANCEDSETTINGS handleInitialSize. */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BankInfo {
			public int size;                           /* The size of this struct (for binary compatibility) */
			public IntPtr userData;                    /* User data to be passed to the file callbacks */
			public int userDataLength;                 /* If this is non-zero, userData will be copied internally */
			public FileOpenCallback openCallback;      /* Callback for opening this file. */
			public FileCloseCallback closeCallback;    /* Callback for closing this file. */
			public FileReadCallback readCallback;      /* Callback for reading from this file. */
			public FileSeekCallback seekCallback;      /* Callback for seeking within this file. */
		}

		[Flags]
		public enum SystemCallbackType : int {
			PREUPDATE = 0x00000001,  /* Called before Studio main update. */
			POSTUPDATE = 0x00000002,  /* Called after Studio main update. */
		}

		public delegate Result SYSTEM_CALLBACK(IntPtr systemraw, SystemCallbackType type, IntPtr parameters, IntPtr userdata);

		public enum ParameterType {
			GAME_CONTROLLED,                  /* Controlled via the API using Studio::ParameterInstance::setValue. */
			AUTOMATIC_DISTANCE,               /* Distance between the event and the listener. */
			AUTOMATIC_EVENT_CONE_ANGLE,       /* Angle between the event's forward vector and the vector pointing from the event to the listener (0 to 180 degrees). */
			AUTOMATIC_EVENT_ORIENTATION,      /* Horizontal angle between the event's forward vector and listener's forward vector (-180 to 180 degrees). */
			AUTOMATIC_DIRECTION,              /* Horizontal angle between the listener's forward vector and the vector pointing from the listener to the event (-180 to 180 degrees). */
			AUTOMATIC_ELEVATION,              /* Angle between the listener's XZ plane and the vector pointing from the listener to the event (-90 to 90 degrees). */
			AUTOMATIC_LISTENER_ORIENTATION,   /* Horizontal angle between the listener's forward vector and the global positive Z axis (-180 to 180 degrees). */
		}

		public struct ParameterDescription {
			public string name;               /* Name of the parameter. */
			public float minimum;             /* Minimum parameter value. */
			public float maximum;             /* Maximum parameter value. */
			public ParameterType type;        /* Type of the parameter */
		}

		#region Wrapper Internal Methods

		// The above structure has an issue with getting a const char* back from game code so we use this special marshalling struct instead
		[StructLayout(LayoutKind.Sequential)]
		struct ParameterDescriptionInternal {
			public IntPtr name;               /* Name of the parameter. */
			public float minimum;             /* Minimum parameter value. */
			public float maximum;             /* Maximum parameter value. */
			public ParameterType type;        /* Type of the parameter */

			// Helper functions
			public void Assign(out ParameterDescription publicDesc) {
				publicDesc.name = MarshallingHelper.StringFromNativeUtf8(name);
				publicDesc.minimum = minimum;
				publicDesc.maximum = maximum;
				publicDesc.type = type;
			}
		}

		// This is only need for loading memory and given our C# wrapper. LoadMemoryPoint isn't feasible anyway.
		enum LoadMemoryMode {
			LoadMemory,
			LoadMemoryPoint
		}

		#endregion

		public class SoundInfo {
			public byte[] name_or_data;         /* The filename or memory buffer that contains the sound. */
			public Mode mode;                   /* Mode flags required for loading the sound. */
			public CreateSoundExInfo exinfo;    /* Extra information required for loading the sound. */
			public int subsoundIndex;           /* Subsound index for loading the sound. */

			// For informational purposes - returns null if the sound will be loaded from memory
			public string name {
				get {
					if (((mode & (Mode.OpenMemory | Mode.OpenMemoryPoint)) == 0) && (name_or_data != null)) {
						return Encoding.UTF8.GetString(name_or_data);
					} else {
						return null;
					}
				}
			}

			#region Wrapper Internal Methods

			~SoundInfo() {
				if (exinfo.inclusionlist != IntPtr.Zero) {
					// Allocated in SOUND_INFO_INTERNAL::assign()
					Marshal.FreeHGlobal(exinfo.inclusionlist);
				}
			}

			#endregion
		}

		#region Wrapper Internal Methods

		// The SOUND_INFO class has issues with getting pointers back from game code so we use this special marshalling struct instead
		[StructLayout(LayoutKind.Sequential)]
		public struct SoundInfoInternal {
			IntPtr name_or_data;
			Mode mode;
			CreateSoundExInfo exinfo;
			int subsoundIndex;

			// Helper functions
			public void assign(out SoundInfo publicInfo) {
				publicInfo = new SoundInfo();

				publicInfo.mode = mode;
				publicInfo.exinfo = exinfo;

				// Somewhat hacky: we know the inclusion list always points to subsoundIndex, so recreate it here
				publicInfo.exinfo.inclusionlist = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
				Marshal.WriteInt32(publicInfo.exinfo.inclusionlist, subsoundIndex);
				publicInfo.exinfo.inclusionlistnum = 1;

				publicInfo.subsoundIndex = subsoundIndex;

				if (name_or_data != IntPtr.Zero) {
					int offset;
					int length;

					if ((mode & (Mode.OpenMemory | Mode.OpenMemoryPoint)) != 0) {
						// OPENMEMORY_POINT won't work, so force it to OPENMEMORY
						publicInfo.mode = (publicInfo.mode & ~Mode.OpenMemoryPoint) | Mode.OpenMemory;

						// We want the data from (name_or_data + offset) to (name_or_data + offset + length)
						offset = (int)exinfo.fileoffset;

						// We'll copy the data taking fileoffset into account, so reset it to 0
						publicInfo.exinfo.fileoffset = 0;

						length = (int)exinfo.length;
					} else {
						offset = 0;
						length = MarshallingHelper.StringLengthUtf8(name_or_data) + 1;
					}

					publicInfo.name_or_data = new byte[length];
					Marshal.Copy(new IntPtr(name_or_data.ToInt64() + offset), publicInfo.name_or_data, 0, length);
				} else {
					publicInfo.name_or_data = null;
				}
			}
		}

		#endregion

		public enum UserPropertyType {
			Integer,         /* Integer property */
			Boolean,         /* Boolean property */
			Float,           /* Float property */
			String,          /* String property */
		}

		public struct UserProperty {
			public string name;           /* Name of the user property. */
			public UserPropertyType type; /* Type of the user property. Use this to select one of the following values. */

			public int intValue;          /* Value of the user property. Only valid when type is USER_PROPERTY_TYPE.INTEGER. */
			public bool boolValue;        /* Value of the user property. Only valid when type is USER_PROPERTY_TYPE.BOOLEAN. */
			public float floatValue;      /* Value of the user property. Only valid when type is USER_PROPERTY_TYPE.FLOAT. */
			public string stringValue;    /* Value of the user property. Only valid when type is USER_PROPERTY_TYPE.STRING. */
		};

		#region Wrapper Internal Methods

		// The above structure has issues with strings and unions so we use this special marshalling struct instead
		[StructLayout(LayoutKind.Sequential)]
		struct UserPropertyInternal {
			IntPtr name;                /* Name of the user property. */
			UserPropertyType type;    /* Type of the user property. Use this to select one of the following values. */

			Union_IntBoolFloatString value;

			// Helper functions
			public UserProperty createPublic() {
				UserProperty publicProperty = new UserProperty();
				publicProperty.name = MarshallingHelper.StringFromNativeUtf8(name);
				publicProperty.type = type;

				switch (type) {
					case UserPropertyType.Integer:
						publicProperty.intValue = value.intValue;
						break;
					case UserPropertyType.Boolean:
						publicProperty.boolValue = value.boolValue;
						break;
					case UserPropertyType.Float:
						publicProperty.floatValue = value.floatValue;
						break;
					case UserPropertyType.String:
						publicProperty.stringValue = MarshallingHelper.StringFromNativeUtf8(value.stringValue);
						break;
				}

				return publicProperty;
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Union_IntBoolFloatString {
			[FieldOffset(0)]
			public int intValue;
			[FieldOffset(0)]
			public bool boolValue;
			[FieldOffset(0)]
			public float floatValue;
			[FieldOffset(0)]
			public IntPtr stringValue;
		}

		#endregion

		[Flags]
		public enum InitFlags {
			Normal = 0x00000000,              /* Initialize normally. */
			LiveUpdate = 0x00000001,          /* Enable live update. */
			AllowMissingPlugins = 0x00000002, /* Load banks even if they reference plugins that have not been loaded. */
			SynchronousUpdate = 0x00000004,   /* Disable asynchronous processing and perform all processing on the calling thread instead. */
		}

		[Flags]
		public enum LoadBankFlags {
			Normal = 0x00000000,      /* Standard behaviour. */
			NonBlocking = 0x00000001, /* Bank loading occurs asynchronously rather than occurring immediately. */
		}

		[Flags]
		public enum RecordCommandsFlags {
			Normal = 0x00000000,      /* Standard behaviour. */
			FileFlush = 0x00000001,   /* Call file flush on every command. */
		}

		public enum PlaybackState {
			Playing,
			Idle,
			Sustaining,
			Stopped,
			Starting,
			Stopping
		}

		public enum EventProperty {
			ChannelPriority,            /* Priority to set on low-level channels created by this event instance (-1 to 256). */
		};

		public enum EventCallbackType {
			Started,                    /* Called when an instance starts. Parameters = unused. */
			Stopped,                    /* Called when an instance stops. Parameters = unused. */
			Idle,                       /* Called when an instance enters the idle state. Parameters = unused. */
			CreateProgrammerSound,      /* Called when a programmer sound needs to be created in order to play a programmer instrument. Parameters = FMOD_STUDIO_PROGRAMMER_SOUND_PROPERTIES. */
			DestroyProgrammerSound,     /* Called when a programmer sound needs to be destroyed. Parameters = FMOD_STUDIO_PROGRAMMER_SOUND_PROPERTIES. */
			Restarted                   /* Called when an instance is restarted due to a repeated start command. Parameters = unused. */
		}

		public delegate Result EventCallback(EventCallbackType type, IntPtr eventInstance, IntPtr parameters);

		public class Util {
			public static Result ParseID(string idString, out GUID id) {
				return FMOD_Studio_ParseID(Encoding.UTF8.GetBytes(idString + Char.MinValue), out id);
			}

			#region Imported Methods
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_ParseID(byte[] idString, out GUID id);
			#endregion
		}

		public class HandleBase {
			protected IntPtr rawPtr;

			public HandleBase(IntPtr newPtr) {
				rawPtr = newPtr;
			}

			public bool IsValid() {
				return rawPtr != IntPtr.Zero;
			}

			public IntPtr GetRaw() {
				return rawPtr;
			}

			#region Equality

			public override bool Equals(Object obj) {
				return Equals(obj as HandleBase);
			}
			public bool Equals(HandleBase p) {
				// Equals if p not null and handle is the same
				return ((object)p != null && rawPtr == p.rawPtr);
			}
			public override int GetHashCode() {
				return rawPtr.ToInt32();
			}
			public static bool operator ==(HandleBase a, HandleBase b) {
				// If both are null, or both are same instance, return true.
				if (Object.ReferenceEquals(a, b)) {
					return true;
				}
				// If one is null, but not both, return false.
				if (((object)a == null) || ((object)b == null)) {
					return false;
				}
				// Return true if the handle matches
				return (a.rawPtr == b.rawPtr);
			}
			public static bool operator !=(HandleBase a, HandleBase b) {
				return !(a == b);
			}
			#endregion
		}

		public class System : HandleBase {
			private static System previousSystem;

			// Initialization / system functions.
			public static Result Create(out System studioSystem) {
				if (previousSystem != null) {
					UnityEngine.Debug.LogWarning("Previous System found! Releasing resources.");
					FMOD.System lowLevel;
					previousSystem.GetLowLevelSystem(out lowLevel);

					if (lowLevel != null) {
						lowLevel.Dispose();
					}

					previousSystem.Dispose();
				}

				Result result = Result.Ok;
				IntPtr rawPtr;
				studioSystem = null;

				result = FMOD_Studio_System_Create(out rawPtr, Version.Number);
				if (result != Result.Ok) {
					return result;
				}

				studioSystem = new System(rawPtr);

				previousSystem = studioSystem;

				return result;
			}

			public void Dispose() {
				UnityEngine.Debug.LogError("Releasing FMOD Studio");
				this.UnloadAll();
				this.Release();
			}

			public Result SetAdvancedSettings(AdvancedSettings settings) {
				settings.cbSize = Marshal.SizeOf(typeof(AdvancedSettings));
				return FMOD_Studio_System_SetAdvancedSettings(rawPtr, ref settings);
			}

			public Result GetAdvancedSettings(out AdvancedSettings settings) {
				settings.cbSize = Marshal.SizeOf(typeof(AdvancedSettings));
				return FMOD_Studio_System_GetAdvancedSettings(rawPtr, out settings);
			}

			public Result Initialize(int maxchannels, InitFlags studioFlags, FMOD.InitFlags flags, IntPtr extradriverdata) {
				return FMOD_Studio_System_Initialize(rawPtr, maxchannels, studioFlags, flags, extradriverdata);
			}

			public Result Release() {
				return FMOD_Studio_System_Release(rawPtr);
			}

			public Result Update() {
				return FMOD_Studio_System_Update(rawPtr);
			}

			public Result GetLowLevelSystem(out FMOD.System system) {
				system = null;

				IntPtr systemraw = new IntPtr();
				Result result = FMOD_Studio_System_GetLowLevelSystem(rawPtr, out systemraw);
				if (result != Result.Ok) {
					return result;
				}

				system = new FMOD.System(systemraw);

				return result;
			}

			public Result GetEvent(GUID guid, LoadingMode mode, out EventDescription _event) {
				_event = null;

				IntPtr eventraw = new IntPtr();
				Result result = FMOD_Studio_System_GetEvent(rawPtr, ref guid, mode, out eventraw);
				if (result != Result.Ok) {
					return result;
				}

				_event = new EventDescription(eventraw);
				return result;
			}

			public Result GetMixerStrip(GUID guid, LoadingMode mode, out MixerStrip strip) {
				strip = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_System_GetMixerStrip(rawPtr, ref guid, mode, out newPtr);
				if (result != Result.Ok) {
					return result;
				}

				strip = new MixerStrip(newPtr);
				return result;
			}

			public Result GetBank(GUID guid, out Bank bank) {
				bank = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_System_GetBank(rawPtr, ref guid, out newPtr);
				if (result != Result.Ok) {
					return result;
				}

				bank = new Bank(newPtr);
				return result;
			}

			public Result GetSoundInfo(string key, out SoundInfo info) {
				SoundInfoInternal internalInfo;

				Result result = FMOD_Studio_System_GetSoundInfo(rawPtr, Encoding.UTF8.GetBytes(key + Char.MinValue), out internalInfo);
				if (result != Result.Ok) {
					info = new SoundInfo();
					return result;
				}

				internalInfo.assign(out info);

				return result;
			}

			public Result LookupID(string path, out GUID guid) {
				return FMOD_Studio_System_LookupID(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), out guid);
			}

			public Result LookupPath(GUID guid, out string path) {
				path = null;

				byte[] buffer = new byte[256];
				int retrieved = 0;
				Result result = FMOD_Studio_System_LookupPath(rawPtr, ref guid, buffer, buffer.Length, out retrieved);

				if (result == Result.ErrorTruncated) {
					buffer = new byte[retrieved];
					result = FMOD_Studio_System_LookupPath(rawPtr, ref guid, buffer, buffer.Length, out retrieved);
				}

				if (result == Result.Ok) {
					path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
				}

				return result;
			}
			public Result GetListenerAttributes(out FMOD3DAttributes attributes) {
				return FMOD_Studio_System_GetListenerAttributes(rawPtr, out attributes);
			}
			public Result SetListenerAttributes(FMOD3DAttributes attributes) {
				return FMOD_Studio_System_SetListenerAttributes(rawPtr, ref attributes);
			}
			public Result LoadBankFile(string name, LoadBankFlags flags, out Bank bank) {
				bank = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_System_LoadBankFile(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), flags, out newPtr);
				if (result != Result.Ok) {
					return result;
				}

				bank = new Bank(newPtr);
				return result;
			}
			public Result LoadBankMemory(byte[] buffer, LoadBankFlags flags, out Bank bank) {
				bank = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_System_LoadBankMemory(rawPtr, buffer, buffer.Length, LoadMemoryMode.LoadMemory, flags, out newPtr);
				if (result != Result.Ok) {
					return result;
				}

				bank = new Bank(newPtr);
				return result;
			}
			public Result LoadBankCustom(BankInfo info, LoadBankFlags flags, out Bank bank) {
				bank = null;

				info.size = Marshal.SizeOf(info);

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_System_LoadBankCustom(rawPtr, ref info, flags, out newPtr);
				if (result != Result.Ok) {
					return result;
				}

				bank = new Bank(newPtr);
				return result;
			}
			public Result UnloadAll() {
				return FMOD_Studio_System_UnloadAll(rawPtr);
			}
			public Result FlushCommands() {
				return FMOD_Studio_System_FlushCommands(rawPtr);
			}
			public Result StartRecordCommands(string path, RecordCommandsFlags flags) {
				return FMOD_Studio_System_StartRecordCommands(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue), flags);
			}
			public Result StopRecordCommands() {
				return FMOD_Studio_System_StopRecordCommands(rawPtr);
			}
			public Result PlaybackCommands(string path) {
				return FMOD_Studio_System_PlaybackCommands(rawPtr, Encoding.UTF8.GetBytes(path + Char.MinValue));
			}
			public Result GetBankCount(out int count) {
				return FMOD_Studio_System_GetBankCount(rawPtr, out count);
			}
			public Result GetBankList(out Bank[] array) {
				array = null;

				Result result;
				int capacity;
				result = FMOD_Studio_System_GetBankCount(rawPtr, out capacity);
				if (result != Result.Ok) {
					return result;
				}
				if (capacity == 0) {
					array = new Bank[0];
					return result;
				}

				IntPtr[] rawArray = new IntPtr[capacity];
				int actualCount;
				result = FMOD_Studio_System_GetBankList(rawPtr, rawArray, capacity, out actualCount);
				if (result != Result.Ok) {
					return result;
				}
				// More items added since we queried just now?
				if (actualCount > capacity) {
					actualCount = capacity;
				}
				array = new Bank[actualCount];
				for (int i = 0; i < actualCount; ++i) {
					array[i] = new Bank(rawArray[i]);
				}
				return Result.Ok;
			}
			public Result GetCPUUsage(out CpuUsage usage) {
				return FMOD_Studio_System_GetCPUUsage(rawPtr, out usage);
			}
			public Result GetBufferUsage(out BufferUsage usage) {
				return FMOD_Studio_System_GetBufferUsage(rawPtr, out usage);
			}
			public Result ResetBufferUsage() {
				return FMOD_Studio_System_ResetBufferUsage(rawPtr);
			}

			public Result SetCallback(SYSTEM_CALLBACK callback, SystemCallbackType callbackmask) {
				return FMOD_Studio_System_SetCallback(rawPtr, callback, callbackmask);
			}

			public Result GetUserData(out IntPtr userData) {
				return FMOD_Studio_System_GetUserData(rawPtr, out userData);
			}

			public Result SetUserData(IntPtr userData) {
				return FMOD_Studio_System_SetUserData(rawPtr, userData);
			}

			#region Imported Methods
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_Create(out IntPtr studiosystem, uint headerversion);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_SetAdvancedSettings(IntPtr studiosystem, ref AdvancedSettings settings);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetAdvancedSettings(IntPtr studiosystem, out AdvancedSettings settings);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_Initialize(IntPtr studiosystem, int maxchannels, InitFlags studioFlags, FMOD.InitFlags flags, IntPtr extradriverdata);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_Release(IntPtr studiosystem);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_Update(IntPtr studiosystem);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetLowLevelSystem(IntPtr studiosystem, out IntPtr system);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetEvent(IntPtr studiosystem, ref GUID guid, LoadingMode mode, out IntPtr description);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetMixerStrip(IntPtr studiosystem, ref GUID guid, LoadingMode mode, out IntPtr mixerStrip);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetBank(IntPtr studiosystem, ref GUID guid, out IntPtr bank);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetSoundInfo(IntPtr studiosystem, byte[] key, out SoundInfoInternal info);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_LookupID(IntPtr studiosystem, byte[] path, out GUID guid);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_LookupPath(IntPtr studiosystem, ref GUID guid, [Out] byte[] path, int size, out int retrieved);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetListenerAttributes(IntPtr studiosystem, out FMOD3DAttributes attributes);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_SetListenerAttributes(IntPtr studiosystem, ref FMOD3DAttributes attributes);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_LoadBankFile(IntPtr studiosystem, byte[] filename, LoadBankFlags flags, out IntPtr bank);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_LoadBankMemory(IntPtr studiosystem, byte[] buffer, int length, LoadMemoryMode mode, LoadBankFlags flags, out IntPtr bank);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_LoadBankCustom(IntPtr studiosystem, ref BankInfo info, LoadBankFlags flags, out IntPtr bank);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_UnloadAll(IntPtr studiosystem);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_FlushCommands(IntPtr studiosystem);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_StartRecordCommands(IntPtr studiosystem, byte[] path, RecordCommandsFlags flags);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_StopRecordCommands(IntPtr studiosystem);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_PlaybackCommands(IntPtr studiosystem, byte[] path);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetBankCount(IntPtr studiosystem, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetBankList(IntPtr studiosystem, IntPtr[] array, int capacity, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetCPUUsage(IntPtr studiosystem, out CpuUsage usage);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetBufferUsage(IntPtr studiosystem, out BufferUsage usage);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_ResetBufferUsage(IntPtr studiosystem);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_SetCallback(IntPtr studiosystem, SYSTEM_CALLBACK callback, SystemCallbackType callbackmask);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_GetUserData(IntPtr studiosystem, out IntPtr userData);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_System_SetUserData(IntPtr studiosystem, IntPtr userData);
			#endregion

			#region Wrapper Internal Methods

			public System(IntPtr raw)
				: base(raw) {
			}

			#endregion
		}

		public class EventDescription : HandleBase {
			public Result GetID(out GUID id) {
				return FMOD_Studio_EventDescription_GetID(rawPtr, out id);
			}
			public Result GetPath(out string path) {
				path = null;

				byte[] buffer = new byte[256];
				int retrieved = 0;
				Result result = FMOD_Studio_EventDescription_GetPath(rawPtr, buffer, buffer.Length, out retrieved);

				if (result == Result.ErrorTruncated) {
					buffer = new byte[retrieved];
					result = FMOD_Studio_EventDescription_GetPath(rawPtr, buffer, buffer.Length, out retrieved);
				}

				if (result == Result.Ok) {
					path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
				}

				return result;
			}
			public Result GetParameterCount(out int count) {
				return FMOD_Studio_EventDescription_GetParameterCount(rawPtr, out count);
			}
			public Result GetParameterByIndex(int index, out ParameterDescription parameter) {
				parameter = new ParameterDescription();

				ParameterDescriptionInternal paramInternal;
				Result result = FMOD_Studio_EventDescription_GetParameterByIndex(rawPtr, index, out paramInternal);
				if (result != Result.Ok) {
					return result;
				}
				paramInternal.Assign(out parameter);
				return result;
			}
			public Result GetParameter(string name, out ParameterDescription parameter) {
				parameter = new ParameterDescription();

				ParameterDescriptionInternal paramInternal;
				Result result = FMOD_Studio_EventDescription_GetParameter(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out paramInternal);
				if (result != Result.Ok) {
					return result;
				}
				paramInternal.Assign(out parameter);
				return result;
			}
			public Result GetUserPropertyCount(out int count) {
				return FMOD_Studio_EventDescription_GetUserPropertyCount(rawPtr, out count);
			}
			public Result GetUserPropertyByIndex(int index, out UserProperty property) {
				UserPropertyInternal propertyInternal;

				Result result = FMOD_Studio_EventDescription_GetUserPropertyByIndex(rawPtr, index, out propertyInternal);
				if (result != Result.Ok) {
					property = new UserProperty();
					return result;
				}

				property = propertyInternal.createPublic();

				return Result.Ok;
			}
			public Result GetUserProperty(string name, out UserProperty property) {
				UserPropertyInternal propertyInternal;

				Result result = FMOD_Studio_EventDescription_GetUserProperty(
					rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out propertyInternal);
				if (result != Result.Ok) {
					property = new UserProperty();
					return result;
				}

				property = propertyInternal.createPublic();

				return Result.Ok;
			}
			public Result GetLength(out int length) {
				return FMOD_Studio_EventDescription_GetLength(rawPtr, out length);
			}
			public Result GetMinimumDistance(out float distance) {
				return FMOD_Studio_EventDescription_GetMinimumDistance(rawPtr, out distance);
			}
			public Result GetMaximumDistance(out float distance) {
				return FMOD_Studio_EventDescription_GetMaximumDistance(rawPtr, out distance);
			}
			public Result IsOneshot(out bool oneshot) {
				return FMOD_Studio_EventDescription_IsOneshot(rawPtr, out oneshot);
			}
			public Result IsStream(out bool isStream) {
				return FMOD_Studio_EventDescription_IsStream(rawPtr, out isStream);
			}
			public Result Is3D(out bool is3D) {
				return FMOD_Studio_EventDescription_Is3D(rawPtr, out is3D);
			}

			public Result CreateInstance(out EventInstance instance) {
				instance = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_EventDescription_CreateInstance(rawPtr, out newPtr);
				if (result != Result.Ok) {
					return result;
				}
				instance = new EventInstance(newPtr);
				return result;
			}

			public Result GetInstanceCount(out int count) {
				return FMOD_Studio_EventDescription_GetInstanceCount(rawPtr, out count);
			}
			public Result GetInstanceList(out EventInstance[] array) {
				array = null;

				Result result;
				int capacity;
				result = FMOD_Studio_EventDescription_GetInstanceCount(rawPtr, out capacity);
				if (result != Result.Ok) {
					return result;
				}
				if (capacity == 0) {
					array = new EventInstance[0];
					return result;
				}

				IntPtr[] rawArray = new IntPtr[capacity];
				int actualCount;
				result = FMOD_Studio_EventDescription_GetInstanceList(rawPtr, rawArray, capacity, out actualCount);
				if (result != Result.Ok) {
					return result;
				}
				// More items added since we queried just now?
				if (actualCount > capacity) {
					actualCount = capacity;
				}
				array = new EventInstance[actualCount];
				for (int i = 0; i < actualCount; ++i) {
					array[i] = new EventInstance(rawArray[i]);
				}
				return Result.Ok;
			}

			public Result LoadSampleData() {
				return FMOD_Studio_EventDescription_LoadSampleData(rawPtr);
			}

			public Result UnloadSampleData() {
				return FMOD_Studio_EventDescription_UnloadSampleData(rawPtr);
			}

			public Result GetSampleLoadingState(out LoadingState state) {
				return FMOD_Studio_EventDescription_GetSampleLoadingState(rawPtr, out state);
			}

			public Result ReleaseAllInstances() {
				return FMOD_Studio_EventDescription_ReleaseAllInstances(rawPtr);
			}
			public Result SetCallback(EventCallback callback) {
				return FMOD_Studio_EventDescription_SetCallback(rawPtr, callback);
			}

			public Result GetUserData(out IntPtr userData) {
				return FMOD_Studio_EventDescription_GetUserData(rawPtr, out userData);
			}

			public Result SetUserData(IntPtr userData) {
				return FMOD_Studio_EventDescription_SetUserData(rawPtr, userData);
			}

			#region Imported Methods
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetID(IntPtr eventdescription, out GUID id);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetPath(IntPtr eventdescription, [Out] byte[] path, int size, out int retrieved);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetParameterCount(IntPtr eventdescription, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetParameterByIndex(IntPtr eventdescription, int index, out ParameterDescriptionInternal parameter);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetParameter(IntPtr eventdescription, byte[] name, out ParameterDescriptionInternal parameter);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetUserPropertyCount(IntPtr eventdescription, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetUserPropertyByIndex(IntPtr eventdescription, int index, out UserPropertyInternal property);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetUserProperty(IntPtr eventdescription, byte[] name, out UserPropertyInternal property);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetLength(IntPtr eventdescription, out int length);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetMinimumDistance(IntPtr eventdescription, out float distance);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetMaximumDistance(IntPtr eventdescription, out float distance);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_IsOneshot(IntPtr eventdescription, out bool oneshot);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_IsStream(IntPtr eventdescription, out bool isStream);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_Is3D(IntPtr eventdescription, out bool is3D);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_CreateInstance(IntPtr eventdescription, out IntPtr instance);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetInstanceCount(IntPtr eventdescription, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetInstanceList(IntPtr eventdescription, IntPtr[] array, int capacity, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_LoadSampleData(IntPtr eventdescription);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_UnloadSampleData(IntPtr eventdescription);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetSampleLoadingState(IntPtr eventdescription, out LoadingState state);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_ReleaseAllInstances(IntPtr eventdescription);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_SetCallback(IntPtr eventdescription, EventCallback callback);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_GetUserData(IntPtr eventdescription, out IntPtr userData);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventDescription_SetUserData(IntPtr eventdescription, IntPtr userData);
			#endregion
			#region Wrapper Internal Methods

			public EventDescription(IntPtr raw)
				: base(raw) {
			}

			#endregion
		}

		public class EventInstance : HandleBase {
			public Result GetDescription(out EventDescription description) {
				description = null;

				IntPtr newPtr;
				Result result = FMOD_Studio_EventInstance_GetDescription(rawPtr, out newPtr);
				if (result != Result.Ok) {
					return result;
				}
				description = new EventDescription(newPtr);
				return result;
			}
			public Result GetVolume(out float volume) {
				return FMOD_Studio_EventInstance_GetVolume(rawPtr, out volume);
			}
			public Result SetVolume(float volume) {
				return FMOD_Studio_EventInstance_SetVolume(rawPtr, volume);
			}
			public Result GetPitch(out float pitch) {
				return FMOD_Studio_EventInstance_GetPitch(rawPtr, out pitch);
			}
			public Result SetPitch(float pitch) {
				return FMOD_Studio_EventInstance_SetPitch(rawPtr, pitch);
			}
			public Result Get3DAttributes(out FMOD3DAttributes attributes) {
				return FMOD_Studio_EventInstance_Get3DAttributes(rawPtr, out attributes);
			}
			public Result Set3DAttributes(FMOD3DAttributes attributes) {
				return FMOD_Studio_EventInstance_Set3DAttributes(rawPtr, ref attributes);
			}
			public Result GetProperty(EventProperty index, out float value) {
				return FMOD_Studio_EventInstance_GetProperty(rawPtr, index, out value);
			}
			public Result SetProperty(EventProperty index, float value) {
				return FMOD_Studio_EventInstance_SetProperty(rawPtr, index, value);
			}
			public Result GetPaused(out bool paused) {
				return FMOD_Studio_EventInstance_GetPaused(rawPtr, out paused);
			}
			public Result SetPaused(bool paused) {
				return FMOD_Studio_EventInstance_SetPaused(rawPtr, paused);
			}
			public Result Start() {
				return FMOD_Studio_EventInstance_Start(rawPtr);
			}
			public Result Stop(StopNode mode) {
				return FMOD_Studio_EventInstance_Stop(rawPtr, mode);
			}
			public Result GetTimelinePosition(out int position) {
				return FMOD_Studio_EventInstance_GetTimelinePosition(rawPtr, out position);
			}
			public Result SetTimelinePosition(int position) {
				return FMOD_Studio_EventInstance_SetTimelinePosition(rawPtr, position);
			}
			public Result GetPlaybackState(out PlaybackState state) {
				return FMOD_Studio_EventInstance_GetPlaybackState(rawPtr, out state);
			}
			public Result GetChannelGroup(out FMOD.ChannelGroup group) {
				group = null;

				IntPtr groupraw = new IntPtr();
				Result result = FMOD_Studio_EventInstance_GetChannelGroup(rawPtr, out groupraw);
				if (result != Result.Ok) {
					return result;
				}

				group = new FMOD.ChannelGroup(groupraw);

				return result;
			}
			public Result Release() {
				return FMOD_Studio_EventInstance_Release(rawPtr);
			}
			public Result IsVirtual(out bool virtualState) {
				return FMOD_Studio_EventInstance_IsVirtual(rawPtr, out virtualState);
			}
			public Result GetParameter(string name, out ParameterInstance instance) {
				instance = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_EventInstance_GetParameter(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out newPtr);
				if (result != Result.Ok) {
					return result;
				}
				instance = new ParameterInstance(newPtr);

				return result;
			}
			public Result GetParameterCount(out int count) {
				return FMOD_Studio_EventInstance_GetParameterCount(rawPtr, out count);
			}
			public Result GetParameterByIndex(int index, out ParameterInstance instance) {
				instance = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_EventInstance_GetParameterByIndex(rawPtr, index, out newPtr);
				if (result != Result.Ok) {
					return result;
				}
				instance = new ParameterInstance(newPtr);

				return result;
			}
			public Result SetParameterValue(string name, float value) {
				return FMOD_Studio_EventInstance_SetParameterValue(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), value);
			}
			public Result SetParameterValueByIndex(int index, float value) {
				return FMOD_Studio_EventInstance_SetParameterValueByIndex(rawPtr, index, value);
			}
			public Result GetCue(string name, out CueInstance instance) {
				instance = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_EventInstance_GetCue(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out newPtr);
				if (result != Result.Ok) {
					return result;
				}
				instance = new CueInstance(newPtr);

				return result;
			}
			public Result GetCueByIndex(int index, out CueInstance instance) {
				instance = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_EventInstance_GetCueByIndex(rawPtr, index, out newPtr);
				if (result != Result.Ok) {
					return result;
				}
				instance = new CueInstance(newPtr);

				return result;
			}
			public Result GetCueCount(out int count) {
				return FMOD_Studio_EventInstance_GetCueCount(rawPtr, out count);
			}
			public Result CreateSubEvent(string name, out EventInstance instance) {
				instance = null;

				IntPtr newPtr = new IntPtr();
				Result result = FMOD_Studio_EventInstance_CreateSubEvent(rawPtr, Encoding.UTF8.GetBytes(name + Char.MinValue), out newPtr);
				if (result != Result.Ok) {
					return result;
				}
				instance = new EventInstance(newPtr);

				return result;
			}
			public Result SetCallback(EventCallback callback) {
				return FMOD_Studio_EventInstance_SetCallback(rawPtr, callback);
			}
			public Result GetUserData(out IntPtr userData) {
				return FMOD_Studio_EventInstance_GetUserData(rawPtr, out userData);
			}
			public Result SetUserData(IntPtr userData) {
				return FMOD_Studio_EventInstance_SetUserData(rawPtr, userData);
			}

			#region Imported Methods
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetDescription(IntPtr _event, out IntPtr description);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetVolume(IntPtr _event, out float volume);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetVolume(IntPtr _event, float volume);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetPitch(IntPtr _event, out float pitch);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetPitch(IntPtr _event, float pitch);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_Get3DAttributes(IntPtr _event, out FMOD3DAttributes attributes);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_Set3DAttributes(IntPtr _event, ref FMOD3DAttributes attributes);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetProperty(IntPtr _event, EventProperty index, out float value);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetProperty(IntPtr _event, EventProperty index, float value);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetPaused(IntPtr _event, out bool paused);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetPaused(IntPtr _event, bool paused);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_Start(IntPtr _event);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_Stop(IntPtr _event, StopNode mode);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetTimelinePosition(IntPtr _event, out int position);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetTimelinePosition(IntPtr _event, int position);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetPlaybackState(IntPtr _event, out PlaybackState state);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetChannelGroup(IntPtr _event, out IntPtr group);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_Release(IntPtr _event);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_IsVirtual(IntPtr _event, out bool virtualState);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetParameter(IntPtr _event, byte[] name, out IntPtr parameter);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetParameterByIndex(IntPtr _event, int index, out IntPtr parameter);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetParameterCount(IntPtr _event, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetParameterValue(IntPtr _event, byte[] name, float value);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetParameterValueByIndex(IntPtr _event, int index, float value);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetCue(IntPtr _event, byte[] name, out IntPtr cue);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetCueByIndex(IntPtr _event, int index, out IntPtr cue);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetCueCount(IntPtr _event, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_CreateSubEvent(IntPtr _event, byte[] name, out IntPtr _instance);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetCallback(IntPtr _event, EventCallback callback);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_GetUserData(IntPtr _event, out IntPtr userData);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_EventInstance_SetUserData(IntPtr _event, IntPtr userData);
			#endregion

			#region Wrapper Internal Methods

			public EventInstance(IntPtr raw)
				: base(raw) {
			}

			#endregion
		}

		public class CueInstance : HandleBase {
			public Result Trigger() {
				return FMOD_Studio_CueInstance_Trigger(rawPtr);
			}

			#region Imported Methods
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_CueInstance_Trigger(IntPtr cue);
			#endregion

			#region Wrapper Internal Methods

			public CueInstance(IntPtr raw)
				: base(raw) {
			}

			#endregion
		}

		public class ParameterInstance : HandleBase {
			public Result GetDescription(out ParameterDescription description) {
				description = new ParameterDescription();

				ParameterDescriptionInternal paramInternal;
				Result result = FMOD_Studio_ParameterInstance_GetDescription(rawPtr, out paramInternal);
				if (result != Result.Ok) {
					return result;
				}
				paramInternal.Assign(out description);
				return result;
			}

			public Result GetValue(out float value) {
				return FMOD_Studio_ParameterInstance_GetValue(rawPtr, out value);
			}
			public Result SetValue(float value) {
				return FMOD_Studio_ParameterInstance_SetValue(rawPtr, value);
			}

			#region Import Methods
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_ParameterInstance_GetDescription(IntPtr parameter, out ParameterDescriptionInternal description);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_ParameterInstance_GetValue(IntPtr parameter, out float value);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_ParameterInstance_SetValue(IntPtr parameter, float value);
			#endregion

			#region Wrapper Internal Methods

			public ParameterInstance(IntPtr raw)
				: base(raw) {
			}

			#endregion
		}

		public class MixerStrip : HandleBase {
			public Result GetID(out GUID id) {
				return FMOD_Studio_MixerStrip_GetID(rawPtr, out id);
			}
			public Result GetPath(out string path) {
				path = null;

				byte[] buffer = new byte[256];
				int retrieved = 0;
				Result result = FMOD_Studio_MixerStrip_GetPath(rawPtr, buffer, buffer.Length, out retrieved);

				if (result == Result.ErrorTruncated) {
					buffer = new byte[retrieved];
					result = FMOD_Studio_MixerStrip_GetPath(rawPtr, buffer, buffer.Length, out retrieved);
				}

				if (result == Result.Ok) {
					path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
				}

				return result;
			}
			public Result GetFaderLevel(out float volume) {
				return FMOD_Studio_MixerStrip_GetFaderLevel(rawPtr, out volume);
			}
			public Result SetFaderLevel(float volume) {
				return FMOD_Studio_MixerStrip_SetFaderLevel(rawPtr, volume);
			}
			public Result GetPaused(out bool paused) {
				return FMOD_Studio_MixerStrip_GetPaused(rawPtr, out paused);
			}
			public Result SetPaused(bool paused) {
				return FMOD_Studio_MixerStrip_SetPaused(rawPtr, paused);
			}
			public Result GetMute(out bool mute) {
				return FMOD_Studio_MixerStrip_GetMute(rawPtr, out mute);
			}
			public Result SetMute(bool mute) {
				return FMOD_Studio_MixerStrip_SetMute(rawPtr, mute);
			}
			public Result StopAllEvents(StopNode mode) {
				return FMOD_Studio_MixerStrip_StopAllEvents(rawPtr, mode);
			}
			public Result GetChannelGroup(out FMOD.ChannelGroup group) {
				group = null;

				IntPtr groupraw = new IntPtr();
				Result result = FMOD_Studio_MixerStrip_GetChannelGroup(rawPtr, out groupraw);
				if (result != Result.Ok) {
					return result;
				}

				group = new FMOD.ChannelGroup(groupraw);

				return result;
			}
			public Result GetLoadingState(out LoadingState state) {
				return FMOD_Studio_MixerStrip_GetLoadingState(rawPtr, out state);
			}
			public Result Release() {
				return FMOD_Studio_MixerStrip_Release(rawPtr);
			}

			#region Imported Methods
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_GetID(IntPtr strip, out GUID id);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_GetPath(IntPtr strip, [Out] byte[] path, int size, out int retrieved);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_GetFaderLevel(IntPtr strip, out float value);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_SetFaderLevel(IntPtr strip, float value);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_GetPaused(IntPtr strip, out bool paused);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_SetPaused(IntPtr strip, bool paused);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_GetMute(IntPtr strip, out bool mute);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_SetMute(IntPtr strip, bool mute);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_StopAllEvents(IntPtr strip, StopNode mode);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_GetChannelGroup(IntPtr strip, out IntPtr group);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_GetLoadingState(IntPtr strip, out LoadingState state);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_MixerStrip_Release(IntPtr strip);
			#endregion

			#region Wrapper Internal Methods

			public MixerStrip(IntPtr raw)
				: base(raw) {
			}

			#endregion
		}

		public class Bank : HandleBase {
			// Property access
			public Result GetID(out GUID id) {
				return FMOD_Studio_Bank_GetID(rawPtr, out id);
			}
			public Result GetPath(out string path) {
				path = null;

				byte[] buffer = new byte[256];
				int retrieved = 0;
				Result result = FMOD_Studio_Bank_GetPath(rawPtr, buffer, buffer.Length, out retrieved);

				if (result == Result.ErrorTruncated) {
					buffer = new byte[retrieved];
					result = FMOD_Studio_Bank_GetPath(rawPtr, buffer, buffer.Length, out retrieved);
				}

				if (result == Result.Ok) {
					path = Encoding.UTF8.GetString(buffer, 0, retrieved - 1);
				}

				return result;
			}
			public Result Unload() {
				Result result = FMOD_Studio_Bank_Unload(rawPtr);

				if (result != Result.Ok) {
					return result;
				}

				rawPtr = IntPtr.Zero;

				return Result.Ok;
			}
			public Result LoadSampleData() {
				return FMOD_Studio_Bank_LoadSampleData(rawPtr);
			}
			public Result UnloadSampleData() {
				return FMOD_Studio_Bank_UnloadSampleData(rawPtr);
			}
			public Result GetLoadingState(out LoadingState state) {
				return FMOD_Studio_Bank_GetLoadingState(rawPtr, out state);
			}
			public Result GetSampleLoadingState(out LoadingState state) {
				return FMOD_Studio_Bank_GetSampleLoadingState(rawPtr, out state);
			}

			// Enumeration
			public Result GetEventCount(out int count) {
				return FMOD_Studio_Bank_GetEventCount(rawPtr, out count);
			}
			public Result GetEventList(out EventDescription[] array) {
				array = null;

				Result result;
				int capacity;
				result = FMOD_Studio_Bank_GetEventCount(rawPtr, out capacity);
				if (result != Result.Ok) {
					return result;
				}
				if (capacity == 0) {
					array = new EventDescription[0];
					return result;
				}

				IntPtr[] rawArray = new IntPtr[capacity];
				int actualCount;
				result = FMOD_Studio_Bank_GetEventList(rawPtr, rawArray, capacity, out actualCount);
				if (result != Result.Ok) {
					return result;
				}
				// More items added since we queried just now?
				if (actualCount > capacity) {
					actualCount = capacity;
				}
				array = new EventDescription[actualCount];
				for (int i = 0; i < actualCount; ++i) {
					array[i] = new EventDescription(rawArray[i]);
				}
				return Result.Ok;
			}
			public Result GetMixerStripCount(out int count) {
				return FMOD_Studio_Bank_GetMixerStripCount(rawPtr, out count);
			}
			public Result GetMixerStripList(out MixerStrip[] array) {
				array = null;

				Result result;
				int capacity;
				result = FMOD_Studio_Bank_GetMixerStripCount(rawPtr, out capacity);
				if (result != Result.Ok) {
					return result;
				}
				if (capacity == 0) {
					array = new MixerStrip[0];
					return result;
				}

				IntPtr[] rawArray = new IntPtr[capacity];
				int actualCount;
				result = FMOD_Studio_Bank_GetMixerStripList(rawPtr, rawArray, capacity, out actualCount);
				if (result != Result.Ok) {
					return result;
				}
				// More items added since we queried just now?
				if (actualCount > capacity) {
					actualCount = capacity;
				}
				array = new MixerStrip[actualCount];
				for (int i = 0; i < actualCount; ++i) {
					array[i] = new MixerStrip(rawArray[i]);
				}
				return Result.Ok;
			}

			#region Imported Methods
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_GetID(IntPtr bank, out GUID id);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_GetPath(IntPtr bank, [Out] byte[] path, int size, out int retrieved);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_Unload(IntPtr bank);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_LoadSampleData(IntPtr bank);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_UnloadSampleData(IntPtr bank);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_GetLoadingState(IntPtr bank, out LoadingState state);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_GetSampleLoadingState(IntPtr bank, out LoadingState state);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_GetEventCount(IntPtr bank, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_GetEventList(IntPtr bank, IntPtr[] array, int capacity, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_GetMixerStripCount(IntPtr bank, out int count);
			[DllImport(StudioVersion.dll)]
			private static extern Result FMOD_Studio_Bank_GetMixerStripList(IntPtr bank, IntPtr[] array, int capacity, out int count);
			#endregion

			#region Wrapper Internal Methods

			public Bank(IntPtr raw)
				: base(raw) {
			}

			#endregion
		}

		#region Wrapper Internal Methods

		// Helper functions
		class MarshallingHelper {
			public static int StringLengthUtf8(IntPtr nativeUtf8) {
				int len = 0;
				while (Marshal.ReadByte(nativeUtf8, len) != 0)
					++len;

				return len;
			}

			public static string StringFromNativeUtf8(IntPtr nativeUtf8) {
				// There is no one line marshal IntPtr->string for UTF8
				int len = StringLengthUtf8(nativeUtf8);
				if (len == 0)
					return string.Empty;
				byte[] buffer = new byte[len];
				Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
				return Encoding.UTF8.GetString(buffer);
			}
		}

		#endregion
	} // System

} // FMOD
