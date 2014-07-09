using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMOD {
	namespace Studio {
		public class FMODLogger {
			#region Fields & Properties
			// ----------------------------------------------------------------------------------------------------
			/// <summary>
			/// The logger name.
			/// </summary>
			private readonly string loggerName;

			/// <summary>
			/// The unity object.
			/// </summary>
			private readonly UnityEngine.Object unityObject;
			// ----------------------------------------------------------------------------------------------------
			#endregion

			#region Initialization
			// ----------------------------------------------------------------------------------------------------
			/// <summary>
			/// Initializes a new instance of the <see cref="FMODLogger"/> class.
			/// </summary>
			/// <param name="loggerName">Name of the logger.</param>
			public FMODLogger(string loggerName) {
				this.loggerName = loggerName;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="FMODLogger"/> class.
			/// </summary>
			/// <param name="loggerName">Name of the logger.</param>
			/// <param name="unityObject">The unity object.</param>
			public FMODLogger(string loggerName, UnityEngine.Object unityObject) {
				this.loggerName = loggerName;
				this.unityObject = unityObject;
			} 
			// ----------------------------------------------------------------------------------------------------
			#endregion

			#region Error Checking & Logging
			// ----------------------------------------------------------------------------------------------------
			/// <summary>
			/// Logs the specified message.
			/// </summary>
			/// <param name="message">The message.</param>
			public void Log(string message) {
				this.LogMessage(message);
			}

			/// <summary>
			/// Logs the specified message.
			/// </summary>
			/// <param name="msg">The message.</param>
			public void LogMessage(string message) {
				UnityUtil.Log(loggerName, message, unityObject);
			}

			/// <summary>
			/// Logs the warning.
			/// </summary>
			/// <param name="msg">The MSG.</param>
			public void LogWarning(string message) {
				UnityUtil.LogWarning(loggerName, message, unityObject);
			}

			/// <summary>
			/// Logs the error.
			/// </summary>
			/// <param name="msg">The MSG.</param>
			public void LogError(string message) {
				UnityUtil.LogError(loggerName, message, unityObject);
			}

			/// <summary>
			/// Errors the check.
			/// </summary>
			/// <param name="result">The result.</param>
			public bool ErrorCheck(FMOD.Result result) {
				return UnityUtil.ErrorCheck(loggerName, result, unityObject);
			}
			// ----------------------------------------------------------------------------------------------------
			#endregion
		}
	}
}
