using UnityEngine;

namespace FMOD {
	namespace Studio {
		public static class UnityUtil {
			#region Constants
			// ----------------------------------------------------------------------------------------------------
			/// <summary>
			/// The instance's name as showng in the logger.
			/// </summary>
			private static readonly FMODLogger Logger = new FMODLogger("FMOD Util");
			// ----------------------------------------------------------------------------------------------------
			#endregion

			#region FMOD Vector Extensions
			// ----------------------------------------------------------------------------------------------------
			/// <summary>
			/// Converts a Unity Vector3 to FMOD Vector.
			/// </summary>
			/// <param name="vector">The vector.</param>
			/// <returns></returns>
			public static Vector ToFMODVector(this Vector3 vector) {
				Vector temp;
				temp.x = vector.x;
				temp.y = vector.y;
				temp.z = vector.z;

				return temp;
			}

			/// <summary>
			/// Converts a Unity Vector3 position to FMOD 3D Attributes.
			/// </summary>
			/// <param name="position">The position.</param>
			/// <returns></returns>
			public static FMOD3DAttributes To3DAttributes(this Vector3 position) {
				FMOD.Studio.FMOD3DAttributes attributes = new FMOD.Studio.FMOD3DAttributes();
				attributes.forward = ToFMODVector(Vector3.forward);
				attributes.up = ToFMODVector(Vector3.up);
				attributes.position = ToFMODVector(position);

				return attributes;
			}

			/// <summary>
			/// Converts a Unity GameObject [and rigidbody] to FMOD 3D Attributes.
			/// </summary>
			/// <param name="go">The go.</param>
			/// <param name="rigidbody">The rigidbody.</param>
			/// <returns></returns>
			public static FMOD3DAttributes To3DAttributes(GameObject go, Rigidbody rigidbody = null) {
				FMOD.Studio.FMOD3DAttributes attributes = new FMOD.Studio.FMOD3DAttributes();
				attributes.forward = ToFMODVector(go.transform.forward);
				attributes.up = ToFMODVector(go.transform.up);
				attributes.position = ToFMODVector(go.transform.position);

				if (rigidbody) {
					attributes.velocity = ToFMODVector(rigidbody.velocity);
				}

				return attributes;
			}
			// ----------------------------------------------------------------------------------------------------
			#endregion

			#region Loading Methods
			// ----------------------------------------------------------------------------------------------------
			/// <summary>
			/// Forces the load of the Low Level Binary.
			/// </summary>
			/// <returns></returns>
			public static bool ForceLoadLowLevelBinary() {
				// This is a hack that forces Android to load the .so libraries in the correct order
#if UNITY_ANDROID && !UNITY_EDITOR
				FMOD.Studio.UnityUtil.Log("loading binaries: " + FMOD.Studio.STUDIO_VERSION.dll + " and " + FMOD.VERSION.dll);
				AndroidJavaClass jSystem = new AndroidJavaClass("java.lang.System");
				jSystem.CallStatic("loadLibrary", FMOD.VERSION.dll);
				jSystem.CallStatic("loadLibrary", FMOD.Studio.STUDIO_VERSION.dll);
#endif

				// Hack: force the low level binary to be loaded before accessing Studio API
#if !UNITY_IPHONE || UNITY_EDITOR
				Logger.Log("Loading Low Level Binary");
				int temp1, temp2;
				if (!Logger.ErrorCheck(FMOD.Memory.GetStats(out temp1, out temp2))) {
					Logger.LogError("An error occured while loading Low Level Binary!");
					return false;
				}

				Logger.Log("Low Level Binary successfully loaded!");
#endif

				return true;
			}
			// ----------------------------------------------------------------------------------------------------
			#endregion

			#region Logging Methods
			// ----------------------------------------------------------------------------------------------------
			/// <summary>
			/// Logs the specified MSG.
			/// </summary>
			/// <param name="msg">The MSG.</param>
			public static void Log(string name, string msg, UnityEngine.Object unityObject = null) {
#if FMOD_DEBUG
				Debug.Log(ComposeLogErrorMessage(name, msg), unityObject);
#endif
			}

			/// <summary>
			/// Logs the warning.
			/// </summary>
			/// <param name="msg">The MSG.</param>
			public static void LogWarning(string name, string msg, UnityEngine.Object unityObject = null) {
				Debug.LogWarning(ComposeLogErrorMessage(name, msg), unityObject);
			}

			/// <summary>
			/// Logs the error.
			/// </summary>
			/// <param name="msg">The MSG.</param>
			public static void LogError(string name, string msg, UnityEngine.Object unityObject = null) {
				Debug.LogError(ComposeLogErrorMessage(name, msg), unityObject);
			}

			/// <summary>
			/// Composes the log error message.
			/// </summary>
			/// <param name="name">The name.</param>
			/// <param name="msg">The MSG.</param>
			private static string ComposeLogErrorMessage(string name, string msg) {
				return "[" + name + "] " + msg;
			}
			// ----------------------------------------------------------------------------------------------------
			#endregion

			#region Error Checking
			// ----------------------------------------------------------------------------------------------------
			/// <summary>
			/// Checks an FMOD result for errors.
			/// </summary>
			/// <param name="result">The result.</param>
			/// <returns>Whether or not the FMOD result was positive.</returns>
			public static bool ErrorCheck(string name, FMOD.Result result, UnityEngine.Object unityObject = null) {
				if (result != FMOD.Result.Ok) {
					LogError(name, "FMOD Error (" + result.ToString() + "): " + FMOD.Error.String(result), unityObject);
				}

				return (result == FMOD.Result.Ok);
			}
			// ----------------------------------------------------------------------------------------------------
			#endregion
		}
	}
}