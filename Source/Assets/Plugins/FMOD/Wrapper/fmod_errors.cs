/* ============================================================================================= = */
/* FMOD Ex - Error string header file. Copyright (c), Firelight Technologies Pty, Ltd. 2004-2014.  */
/* Modified by Xane (https://github.com/XaneFeather), Jul 2014                                     */
/*                                                                                                 */
/* Use this header if you want to store or display a string version / english explanation of       */
/* the FMOD error codes.                                                                           */
/*                                                                                                 */
/* =============================================================================================== */

namespace FMOD {
	public class Error {
		public static string String(FMOD.Result errcode) {
			switch (errcode) {
				case FMOD.Result.Ok:
					return "No errors.";
				case FMOD.Result.ErrorAlreadyLocked:
					return "Tried to call lock a second time before unlock was called.";
				case FMOD.Result.ErrorBadCommand:
					return "Tried to call a function on a data type that does not allow this type of functionality (ie calling Sound::lock on a streaming sound).";
				case FMOD.Result.ErrorChannelAllocation:
					return "Error trying to allocate a channel.";
				case FMOD.Result.ErrorChannelStolen:
					return "The specified channel has been reused to play another sound.";
				case FMOD.Result.ErrorCOM:
					return "A Win32 COM related error occured. COM failed to initialize or a QueryInterface failed meaning a Windows codec or driver was not installed properly.";
				case FMOD.Result.ErrorDMA:
					return "DMA Failure.  See debug output for more information.";
				case FMOD.Result.ErrorDSPConnection:
					return "DSP connection error.  Connection possibly caused a cyclic dependancy or connected dsps with incompatibile buffer counts.";
				case FMOD.Result.ErrorDSPDontProcess:
					return "DSP return code from a DSP process query callback.  Tells mixer not to call the process callback and therefore not consume CPU.  Use this to optimize the DSP graph.";
				case FMOD.Result.ErrorDSPFormat:
					return "DSP Format error.  A DSP unit may have attempted to connect to this network with the wrong format, or a matrix may have been set with the wrong size if the target unit has a specified channel map.";
				case FMOD.Result.ErrorDSPInUse:
					return "DSP is already in the mixer's DSP network. It must be removed before being reinserted or released.";
				case FMOD.Result.ErrorDSPNotFound:
					return "DSP connection error.  Couldn't find the DSP unit specified.";
				case FMOD.Result.ErrorDSPReserved:
					return "DSP operation error.  Cannot perform operation on this DSP as it is reserved by the system.";
				case FMOD.Result.ErrorDSPSilence:
					return "DSP return code from a DSP process query callback.  Tells mixer silence would be produced from read, so go idle and not consume CPU.  Use this to optimize the DSP graph.";
				case FMOD.Result.ErrorDSPType:
					return "DSP operation cannot be performed on a DSP of this type.";
				case FMOD.Result.ErrorFileBad:
					return "Error loading file.";
				case FMOD.Result.ErrorFileCouldNotSeek:
					return "Couldn't perform seek operation.  This is a limitation of the medium (ie netstreams) or the file format.";
				case FMOD.Result.ErrorFileDiskEjected:
					return "Media was ejected while reading.";
				case FMOD.Result.ErrorFileEOF:
					return "End of file unexpectedly reached while trying to read essential data (truncated?).";
				case FMOD.Result.ErrorFileEndOfData:
					return "End of current chunk reached while trying to read data.";
				case FMOD.Result.ErrorFileNotFound:
					return "File not found.";
				case FMOD.Result.ErrorFileUnwanted:
					return "Unwanted file access occured.";
				case FMOD.Result.ErrorFormat:
					return "Unsupported file or audio format.";
				case FMOD.Result.ErrorHeaderMismatch:
					return "There is a version mismatch between the FMOD header and either the FMOD Studio library or the FMOD Low Level library.";
				case FMOD.Result.ErrorHTTP:
					return "A HTTP error occurred. This is a catch-all for HTTP errors not listed elsewhere.";
				case FMOD.Result.ErrorHTTPAccess:
					return "The specified resource requires authentication or is forbidden.";
				case FMOD.Result.ErrorHTTPProxyAuth:
					return "Proxy authentication is required to access the specified resource.";
				case FMOD.Result.ErrorHTTPServerError:
					return "A HTTP server error occurred.";
				case FMOD.Result.ErrorHTTPTimeout:
					return "The HTTP request timed out.";
				case FMOD.Result.ErrorInitialization:
					return "FMOD was not initialized correctly to support this function.";
				case FMOD.Result.ErrorInitialized:
					return "Cannot call this command after System::init.";
				case FMOD.Result.ErrorInternal:
					return "An error occured that wasn't supposed to.  Contact support.";
				case FMOD.Result.ErrorInvalidAddress:
					return "On Xbox 360, this memory address passed to FMOD must be physical, (ie allocated with XPhysicalAlloc.)";
				case FMOD.Result.ErrorInvalidFloat:
					return "Value passed in was a NaN, Inf or denormalized float.";
				case FMOD.Result.ErrorInvalidHandle:
					return "An invalid object handle was used.";
				case FMOD.Result.ErrorInvalidParameter:
					return "An invalid parameter was passed to this function.";
				case FMOD.Result.ErrorInvalidPosition:
					return "An invalid seek position was passed to this function.";
				case FMOD.Result.ErrorInvalidSpeaker:
					return "An invalid speaker was passed to this function based on the current speaker mode.";
				case FMOD.Result.ErrorInvalidSyncPoint:
					return "The syncpoint did not come from this sound handle.";
				case FMOD.Result.ErrorInvalidThread:
					return "Tried to call a function on a thread that is not supported.";
				case FMOD.Result.ErrorInvalidVector:
					return "The vectors passed in are not unit length, or perpendicular.";
				case FMOD.Result.ErrorMaxAudible:
					return "Reached maximum audible playback count for this sound's soundgroup.";
				case FMOD.Result.ErrorMemory:
					return "Not enough memory or resources.";
				case FMOD.Result.ErrorMemoryCannotPoint:
					return "Can't use FMOD_OPENMEMORY_POINT on non PCM source data, or non mp3/xma/adpcm data if FMOD_CREATECOMPRESSEDSAMPLE was used.";
				case FMOD.Result.ErrorMemorySRAM:
					return "Not enough memory or resources on console sound ram.";
				case FMOD.Result.ErrorNeeds2D:
					return "Tried to call a command on a 3d sound when the command was meant for 2d sound.";
				case FMOD.Result.ErrorNeeds3D:
					return "Tried to call a command on a 2d sound when the command was meant for 3d sound.";
				case FMOD.Result.ErrorNeedsHardware:
					return "Tried to use a feature that requires hardware support.  (ie trying to play a GCADPCM compressed sound in software on Wii).";
				case FMOD.Result.ErrorNeedsSoftware:
					return "Tried to use a feature that requires the software engine.  Software engine has either been turned off, or command was executed on a hardware channel which does not support this feature.";
				case FMOD.Result.ErrorNetConnect:
					return "Couldn't connect to the specified host.";
				case FMOD.Result.ErrorNetSocket:
					return "A socket error occurred.  This is a catch-all for socket-related errors not listed elsewhere.";
				case FMOD.Result.ErrorNetURL:
					return "The specified URL couldn't be resolved.";
				case FMOD.Result.ErrorNetWouldBlock:
					return "Operation on a non-blocking socket could not complete immediately.";
				case FMOD.Result.ErrorNotReady:
					return "Operation could not be performed because specified sound/DSP connection is not ready.";
				case FMOD.Result.ErrorOutputAllocated:
					return "Error initializing output device, but more specifically, the output device is already in use and cannot be reused.";
				case FMOD.Result.ErrorOutputCreateBuffer:
					return "Error creating hardware sound buffer.";
				case FMOD.Result.ErrorOutputDriverCall:
					return "A call to a standard soundcard driver failed, which could possibly mean a bug in the driver or resources were missing or exhausted.";
				case FMOD.Result.ErrorOutputFormat:
					return "Soundcard does not support the minimum features needed for this soundsystem (16bit stereo output).";
				case FMOD.Result.ErrorOutputInit:
					return "Error initializing output device.";
				case FMOD.Result.ErrorOutputNoDrivers:
					return "The output device has no drivers installed, so FMOD_OUTPUT_NOSOUND is selected as the output mode.";
				case FMOD.Result.ErrorOutputNoSoftware:
					return "Attempted to create a software sound but no software channels were specified in System::init.";
				case FMOD.Result.ErrorPlugin:
					return "An unspecified error has been returned from a plugin.";
				case FMOD.Result.ErrorPluginInstances:
					return "The number of allowed instances of a plugin has been exceeded.";
				case FMOD.Result.ErrorPluginMissing:
					return "A requested output, dsp unit type or codec was not available.";
				case FMOD.Result.ErrorPluginResource:
					return "A resource that the plugin requires cannot be found. (ie the DLS file for MIDI playback)";
				case FMOD.Result.ErrorPluginVersion:
					return "A plugin was built with an unsupported SDK version.";
				case FMOD.Result.ErrorPreloaded:
					return "The specified sound is still in use by the event system, call EventSystem::unloadFSB before trying to release it.";
				case FMOD.Result.ErrorProgrammerSound:
					return "The specified sound is still in use by the event system, wait for the event which is using it finish with it.";
				case FMOD.Result.ErrorRecord:
					return "An error occured trying to initialize the recording device.";
				case FMOD.Result.ErrorReverbChannelGroup:
					return "Reverb properties cannot be set on this channel because a parent channelgroup owns the reverb connection.";
				case FMOD.Result.ErrorReverbInstance:
					return "Specified instance in FMOD_REVERB_PROPERTIES couldn't be set. Most likely because it is an invalid instance number or the reverb doesnt exist.";
				case FMOD.Result.ErrorSubsounds:
					return "The error occured because the sound referenced contains subsounds when it shouldn't have, or it doesn't contain subsounds when it should have.  The operation may also not be able to be performed on a parent sound.";
				case FMOD.Result.ErrorSubsoundAllocated:
					return "This subsound is already being used by another sound, you cannot have more than one parent to a sound.  Null out the other parent's entry first.";
				case FMOD.Result.ErrorSubsoundCannotMove:
					return "Shared subsounds cannot be replaced or moved from their parent stream, such as when the parent stream is an FSB file.";
				case FMOD.Result.ErrorSubsoundMode:
					return "The subsound's mode bits do not match with the parent sound's mode bits.  See documentation for function that it was called with.";
				case FMOD.Result.ErrorTagNotFound:
					return "The specified tag could not be found or there are no tags.";
				case FMOD.Result.ErrorTooManyChannels:
					return "The sound created exceeds the allowable input channel count.  This can be increased using the 'maxinputchannels' parameter in System::setSoftwareFormat.";
				case FMOD.Result.ErrorTruncated:
					return "The retrieved string is too long to fit in the supplied buffer and has been truncated.";
				case FMOD.Result.ErrorUnimplemented:
					return "Something in FMOD hasn't been implemented when it should be! contact support!";
				case FMOD.Result.ErrorUninitialized:
					return "This command failed because System::init or System::setDriver was not called.";
				case FMOD.Result.ErrorUnsupported:
					return "A command issued was not supported by this object.  Possibly a plugin without certain callbacks specified.";
				case FMOD.Result.ErrorUpdate:
					return "An error caused by System::update occured.";
				case FMOD.Result.ErrorVersion:
					return "The version number of this file format is not supported.";
				case FMOD.Result.ErrorEventAlreadyLoaded:
					return "The specified project or bank has already been loaded. Having multiple copies of the same project loaded simultaneously is forbidden.";
				case FMOD.Result.ErrorEventFailed:
					return "An Event failed to be retrieved, most likely due to 'just fail' being specified as the max playbacks behavior.";
				case FMOD.Result.ErrorEventGUIDConflict:
					return "An event with the same GUID already exists.";
				case FMOD.Result.ErrorEventInfoOnly:
					return "Can't execute this command on an EVENT_INFOONLY event.";
				case FMOD.Result.ErrorEventInternal:
					return "An error occured that wasn't supposed to.  See debug log for reason.";
				case FMOD.Result.ErrorEventLiveUpdateBusy:
					return "The live update connection failed due to the game already being connected.";
				case FMOD.Result.ErrorEventLiveUpdateMismatch:
					return "The live update connection failed due to the game data being out of sync with the tool.";
				case FMOD.Result.ErrorEventLiveUpdateTimeout:
					return "The live update connection timed out.";
				case FMOD.Result.ErrorEventMaxStreams:
					return "Event failed because 'Max streams' was hit when FMOD_EVENT_INIT_FAIL_ON_MAXSTREAMS was specified.";
				case FMOD.Result.ErrorEventMismatch:
					return "FSB mismatches the FEV it was compiled with, the stream/sample mode it was meant to be created with was different, or the FEV was built for a different platform.";
				case FMOD.Result.ErrorEventNameConflict:
					return "A category with the same name already exists.";
				case FMOD.Result.ErrorEventNeedsSimple:
					return "Tried to call a function on a complex event that's only supported by simple events.";
				case FMOD.Result.ErrorEventNotFound:
					return "The requested event, bus or vca could not be found.";
				case FMOD.Result.ErrorEventWontStop:
					return "The event cannot be released because it will not terminate, call stop to allow releasing of this event.";
				case FMOD.Result.ErrorMusicNoCallback:
					return "The music callback is required, but it has not been set.";
				case FMOD.Result.ErrorMusicNotFound:
					return "The requested music entity could not be found.";
				case FMOD.Result.ErrorMusicUninitialized:
					return "Music system is not initialized probably because no music data is loaded.";
				case FMOD.Result.ErrorStudioUninitialized:
					return "The Studio::System object is not yet initialized.";
				case FMOD.Result.ErrorStudioNotLoaded:
					return "The specified resource is not loaded, so it can't be unloaded.";
				default:
					return "Unknown error.";
			}
		}
	}
}
