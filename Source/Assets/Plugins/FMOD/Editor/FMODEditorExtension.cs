#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FMOD.Studio;

[InitializeOnLoad]
public class FMODEditorExtension : MonoBehaviour {
	#region Constants
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// The instance's logger.
	/// </summary>
	private static readonly FMODLogger Logger = new FMODLogger("FMOD Studio Importer");

	public static FMODStudioSystem StudioSystem {
		get { return FMODStudioSystem.Instance; }
	}
	private static FMOD.Studio.EventInstance currentInstance = null;

	private const string AssetFolder = "FMODAssets"; 
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Initialization
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Initializes the <see cref="FMODEditorExtension"/> class.
	/// </summary>
	static FMODEditorExtension() {
		EditorApplication.update += Update;
		EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
	} 
	// ----------------------------------------------------------------------------------------------------
	#endregion

	/// <summary>
	/// Handles the on play mode changed.
	/// </summary>
	private static void HandleOnPlayModeChanged() {
		if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPaused) {
			//StudioSystem.Unload();
			//StudioSystem.Release();
		}

		if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPaused) {
			//StudioSystem.Unload();
			//StudioSystem.Release();
		}
	}

	/// <summary>
	/// Updates this instance.
	/// </summary>
	private static void Update() {
		if (StudioSystem != null && StudioSystem.System != null && StudioSystem.System.IsValid()) {
			Logger.ErrorCheck(StudioSystem.System.Update());
		}
	}

	public static void AuditionEvent(FMODAsset asset) {
		StopEvent();

		var desc = GetEventDescription(asset.id);
		if (desc == null) {
			Logger.LogError("Failed to retrieve EventDescription for event: " + asset.path);
		}

		if (!Logger.ErrorCheck(desc.CreateInstance(out currentInstance))) {
			return;
		}

		Logger.ErrorCheck(currentInstance.Start());
	}

	public static void StopEvent() {
		if (currentInstance != null && currentInstance.IsValid()) {
			Logger.ErrorCheck(currentInstance.Stop(FMOD.Studio.StopNode.Immediate));
			currentInstance = null;
		}
	}

	public static void SetEventParameterValue(int index, float val) {
		if (currentInstance != null && currentInstance.IsValid()) {
			FMOD.Studio.ParameterInstance param;
			currentInstance.GetParameterByIndex(index, out param);
			param.SetValue(val);
		}
	}

	public static FMOD.Studio.EventDescription GetEventDescription(string idString) {
		return StudioSystem.GetEventDescription(idString);
	}

	[MenuItem("FMOD/Import Banks")]
	private static void ImportBanks() {
		PrepareIntegration();

		string filePath = "";
		if (!LocateProject(ref filePath)) {
			return;
		}

		ImportAndRefresh(filePath);
	}


	[MenuItem("FMOD/Refresh Event List", true)]
	private static bool CheckRefreshEventList() {
		if (EditorPrefs.HasKey(GuidPathKey)) {
			string filePath = GetDefaultPath();
			return LocateProjectPath(ref filePath);
		}
		return false;
	}

	[MenuItem("FMOD/Refresh Event List")]
	private static void RefreshEventList() {
		string filePath = GetDefaultPath();
		if (LocateProjectPath(ref filePath)) {
			ImportAndRefresh(filePath);
		} else {
			ImportBanks();
		}
	}

	private static bool CopyBanks(string path) {
		UnloadAllBanks();

		var info = new System.IO.DirectoryInfo(path);

		int bankCount = 0;
		string copyBanksString = "";
		var banksToCopy = new List<System.IO.FileInfo>();

		foreach (var fileInfo in info.GetFiles()) {
			var ex = fileInfo.Extension;
			if (!ex.Equals(".bank", System.StringComparison.CurrentCultureIgnoreCase) &&
				!ex.Equals(".strings", System.StringComparison.CurrentCultureIgnoreCase)) {
					Logger.LogWarning("Ignoring unexpected file: \"" + fileInfo.Name + "\": unknown file type: \"" + fileInfo.Extension + "\"");
				continue;
			}

			bankCount++;

			string bankMessage = "(added)";

			var oldBankPath = Path.Combine(Application.dataPath, Path.Combine("StreamingAssets", fileInfo.Name));
			if (System.IO.File.Exists(oldBankPath)) {
				var oldFileInfo = new System.IO.FileInfo(oldBankPath);
				if (oldFileInfo.LastWriteTime == fileInfo.LastWriteTime) {
					bankMessage = "(same)";
				} else if (oldFileInfo.LastWriteTime < fileInfo.LastWriteTime) {
					bankMessage = "(newer)";
				} else {
					bankMessage = "(older)";
				}
			}

			copyBanksString += fileInfo.Name + " " + bankMessage + "\n";
			banksToCopy.Add(fileInfo);
		}

		if (bankCount == 0) {
			EditorUtility.DisplayDialog("FMOD Studio Importer", "No .bank files found in the directory:\n" + path, "OK");
			return false;
		}

		if (!EditorUtility.DisplayDialog("FMOD Studio Importer", "The import will modify the following files:\n" + copyBanksString, "Continue", "Cancel")) {
			return false;
		}

		FMODBankList fmodBankList = ScriptableObject.CreateInstance<FMODBankList>();
		fmodBankList.GenerateBankList(banksToCopy);

		CreateDirectories(FMODBankList.AssetPath);
		AssetDatabase.CreateAsset(fmodBankList, FMODBankList.AssetPath);

		return true;
	}

	private static bool ImportAndRefresh(string filePath) {
		Logger.LogMessage("Importing from path \"" + filePath + "\"");

		CopyBanks(filePath);

		if (!LoadAllBanks()) {
			return false;
		}

		List<FMODAsset> existingAssets = new List<FMODAsset>();
		GatherExistingAssets(existingAssets);

		List<FMODAsset> newAssets = new List<FMODAsset>();
		GatherNewAssets(filePath, newAssets);

		var assetsToDelete = existingAssets.Except(newAssets, new FMODAssetGUIDComparer());
		var assetsToAdd = newAssets.Except(existingAssets, new FMODAssetGUIDComparer());

		var assetsToMoveFrom = existingAssets.Intersect(newAssets, new FMODAssetGUIDComparer());
		var assetsToMoveTo = newAssets.Intersect(existingAssets, new FMODAssetGUIDComparer());

		var assetsToMove = assetsToMoveFrom.Except(assetsToMoveTo, new FMODAssetPathComparer());

		if (!assetsToDelete.Any() && !assetsToAdd.Any() && !assetsToMove.Any()) {
			Logger.LogMessage("Banks updated. Events list unchanged " + System.DateTime.Now.ToString(@"[hh:mm tt]"));
		} else {
			string assetsToDeleteFormatted = "";
			foreach (var asset in assetsToDelete) {
				assetsToDeleteFormatted += eventToAssetPath(asset.path) + "\n";
			}

			string assetsToAddFormatted = "";
			foreach (var asset in assetsToAdd) {
				assetsToAddFormatted += eventToAssetPath(asset.path) + "\n";
			}

			string assetsToMoveFormatted = "";
			foreach (var asset in assetsToMove) {
				var fromPath = assetsToMoveFrom.First(a => a.id == asset.id).path;
				var toPath = assetsToMoveTo.First(a => a.id == asset.id).path;
				assetsToMoveFormatted += fromPath + "  moved to  " + toPath + "\n";
			}

			string deletionMessage =
					(assetsToDelete.Count() == 0 ? "No assets removed" : "Removed assets: " + assetsToDelete.Count()) + "\n" +
					(assetsToAdd.Count() == 0 ? "No assets added" : "Added assets: " + assetsToAdd.Count()) + "\n" +
					(assetsToMove.Count() == 0 ? "No assets moved" : "Moved assets: " + assetsToMove.Count()) + "\n" +
					((assetsToDelete.Count() != 0 || assetsToAdd.Count() != 0 || assetsToMove.Count() != 0) ? "\nSee console for details" : "");

			Logger.LogMessage("Details " + System.DateTime.Now.ToString(@"[hh:mm tt]") + "\n\n" +
				(assetsToDelete.Count() == 0 ? "No assets removed" : "Removed Assets:\n" + assetsToDeleteFormatted) + "\n" +
				(assetsToAdd.Count() == 0 ? "No assets added" : "Added Assets:\n" + assetsToAddFormatted) + "\n" +
				(assetsToMove.Count() == 0 ? "No assets moved" : "Moved Assets:\n" + assetsToMoveFormatted) + "\n" +
				"________________________________");

			if (!EditorUtility.DisplayDialog("FMOD Studio Importer", deletionMessage, "Continue", "Cancel")) {
				return false; // User clicked cancel
			}
		}

		ImportAssets(assetsToAdd);
		DeleteMissingAssets(assetsToDelete);
		MoveExistingAssets(assetsToMove, assetsToMoveFrom, assetsToMoveTo);

		AssetDatabase.Refresh();

		return true;
	}

	private static void CreateDirectories(string assetPath) {
		Debug.Log("CreateDirectories: " + assetPath);

		const string root = "Assets";
		var currentDir = System.IO.Directory.GetParent(assetPath);
		Stack<string> directories = new Stack<string>();
		while (!currentDir.Name.Equals(root)) {
			directories.Push(currentDir.Name);
			currentDir = currentDir.Parent;
		}

		string path = root;
		while (directories.Any()) {
			var d = directories.Pop();

			if (!System.IO.Directory.Exists(Application.dataPath + "/../" + path + "/" + d)) {
				Logger.LogMessage("Creating folder \"" + path + "/" + d + "\"");
				AssetDatabase.CreateFolder(path, d);
			}
			path += "/" + d;
		}
	}

	private static void MoveExistingAssets(IEnumerable<FMODAsset> assetsToMove, IEnumerable<FMODAsset> assetsToMoveFrom, IEnumerable<FMODAsset> assetsToMoveTo) {
		foreach (var asset in assetsToMove) {
			var fromAsset = assetsToMoveFrom.First(a => a.id == asset.id);
			var toAsset = assetsToMoveTo.First(a => a.id == asset.id);
			var fromPath = "Assets/" + AssetFolder + eventToAssetPath(fromAsset.path) + ".asset";
			var toPath = "Assets/" + AssetFolder + eventToAssetPath(toAsset.path) + ".asset";

			CreateDirectories(toPath);

			if (!AssetDatabase.Contains(fromAsset)) {
				Logger.LogMessage("Importing Asset " + fromPath);
				AssetDatabase.ImportAsset(fromPath);
			}

			string result = AssetDatabase.MoveAsset(fromPath, toPath);
			if (!result.Equals(string.Empty)) {
				Logger.LogError("Asset move failed: " + result);
			} else {
				var dir = new System.IO.FileInfo(fromPath).Directory;
				DeleteDirectoryIfEmpty(dir);
			}

			fromAsset.path = toAsset.path;
		}
	}

	[MenuItem("FMOD/About Integration")]
	private static void AboutIntegration() {
		PrepareIntegration();

		if (StudioSystem.System == null || !StudioSystem.System.IsValid()) {
			EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "Unable to retrieve version, check the version number in fmod.cs", "OK");
		}

		uint version;
		if (!Logger.ErrorCheck(StudioSystem.LowLevelSystem.GetVersion(out version))) {
			return;
		}

		EditorUtility.DisplayDialog("FMOD Studio Unity Integration", "Version: " + GetVersionString(version), "OK");
	}

	/// <summary>
	/// Gets the version string.
	/// </summary>
	/// <param name="version">The version.</param>
	/// <returns></returns>
	private static string GetVersionString(uint version) {
		uint major = (version & 0x00FF0000) >> 16;
		uint minor = (version & 0x0000FF00) >> 8;
		uint patch = (version & 0x000000FF);

		return major.ToString("X1") + "." +
			minor.ToString("X2") + "." +
				patch.ToString("X2");
	}

	/// <summary>
	/// Gets the unique identifier path key.
	/// </summary>
	/// <value>
	/// The unique identifier path key.
	/// </value>
	private static string GuidPathKey {
		get { return "FMODStudioProjectPath_" + Application.dataPath; }
	}

	/// <summary>
	/// Gets the default path.
	/// </summary>
	/// <returns></returns>
	private static string GetDefaultPath() {
		return EditorPrefs.GetString(GuidPathKey, Application.dataPath);
	}

	/// <summary>
	/// Locates the FMOD 1project directory.
	/// </summary>
	/// <param name="bankPath">The bank path.</param>
	/// <returns></returns>
	private static bool LocateProject(ref string bankPath) {
		var defaultPath = GetDefaultPath();

		{
			string workDir = System.Environment.CurrentDirectory;
			bankPath = EditorUtility.OpenFolderPanel("Locate build directory", defaultPath, "Build");
			System.Environment.CurrentDirectory = workDir; // HACK: fixes weird Unity bug that causes random crashes after using OpenFolderPanel 
		}

		if (System.String.IsNullOrEmpty(bankPath)) {
			Logger.LogWarning("No directory selected");
			return false;
		}

		if (!LocateProjectPath(ref bankPath)) {
			EditorUtility.DisplayDialog("Incorrect directory", "Incorrect directory selected. Select the project directory of your FMOD Studio project", "OK");
			return false;
		}

		DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(bankPath);
		EditorPrefs.SetString(GuidPathKey, directoryInfo.Parent.Parent.FullName);

		if (directoryInfo.GetFiles().Count() == 0) {
			EditorUtility.DisplayDialog("FMOD Studio Importer", "No bank files found in directory: " + bankPath + "\nYou must build the FMOD Studio project before importing", "OK");
			return false;
		}

		return true;
	}

	private static bool LocateProjectPath(ref string searchPath) {
		string path = CleanPath(searchPath);
		if (!System.IO.Directory.Exists(path)) {
			return false;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(searchPath);

		// Inside Build/Desktop Folder?
		if (CleanPath(directoryInfo.FullName.ToLower()).Contains("/build/desktop")) {
			return true;
		}

		// FMOD Studio Project Root?
		if (directoryInfo.GetFiles("*.fspro").Length > 0) {
			path = CleanPath(Path.Combine(searchPath, Path.Combine("Build", "Desktop")));
			searchPath = path;
			return true;
		}

		// Inside Build Folder?
		path = CleanPath(Path.Combine(searchPath, "Desktop"));
		if (System.IO.Directory.Exists(path)) {
			searchPath = path;
			return true;
		}

		return false;
	}

	private static void GatherExistingAssets(List<FMODAsset> existingAssets) {
		var assetRoot = Application.dataPath + "/" + AssetFolder;
		if (System.IO.Directory.Exists(assetRoot)) {
			GatherAssetsFromDirectory(assetRoot, existingAssets);
		}
	}

	private static void GatherAssetsFromDirectory(string directory, List<FMODAsset> existingAssets) {
		var info = new System.IO.DirectoryInfo(directory);
		foreach (var file in info.GetFiles()) {
			var relativePath = new System.Uri(Application.dataPath).MakeRelativeUri(new System.Uri(file.FullName)).ToString();
			var asset = (FMODAsset)AssetDatabase.LoadAssetAtPath(relativePath, typeof(FMODAsset));
			if (asset != null) {
				existingAssets.Add(asset);
			}
		}

		foreach (var dir in info.GetDirectories()) {
			GatherAssetsFromDirectory(dir.FullName, existingAssets);
		}
	}

	private static void GatherNewAssets(string filePath, List<FMODAsset> newAssets) {
		if (System.String.IsNullOrEmpty(filePath)) {
			Logger.LogError("No build folder specified");
			return;
		}

		foreach (var bank in StudioSystem.LoadedBanks) {
			int count = 0;
			Logger.ErrorCheck(bank.GetEventCount(out count));

			FMOD.Studio.EventDescription[] descriptions = new FMOD.Studio.EventDescription[count];
			Logger.ErrorCheck(bank.GetEventList(out descriptions));

			foreach (var desc in descriptions) {
				string path;
				FMOD.Result result = desc.GetPath(out path);

				if (result == FMOD.Result.ErrorEventNotFound || desc == null || !desc.IsValid() || !Logger.ErrorCheck(result)) {
					continue;
				}
				Logger.ErrorCheck(result);

				FMOD.GUID id;
				Logger.ErrorCheck(desc.GetID(out id));

				FMODAsset asset = ScriptableObject.CreateInstance<FMODAsset>();
				//asset.name = path.Substring(path.LastIndexOf('/') + 1);
				asset.name = path.Replace('/', '-');
				asset.path = path;
				asset.id = new System.Guid((int)id.Data1, (short)id.Data2, (short)id.Data3, id.Data4).ToString("B");

				newAssets.Add(asset);
			}
		}
	}

	private static string eventToAssetPath(string eventPath) {
		if (eventPath.StartsWith("event:")) {
			return eventPath.Substring(6); // Trim "event:" from the start of the path
		} else if (eventPath.StartsWith("snapshot:")) {
			return eventPath.Substring(9); // Trim "snapshot:" from the start of the path
		} else if (eventPath.StartsWith("/")) {
			// Assume 1.2 style paths
			return eventPath;
		}

		throw new UnityException("Incorrectly formatted FMOD Studio event path: " + eventPath);
	}

	private static void ImportAssets(IEnumerable<FMODAsset> assetsToAdd) {
		foreach (var asset in assetsToAdd) {
			var path = "Assets/" + AssetFolder + eventToAssetPath(asset.path) + ".asset";
			CreateDirectories(path);

			AssetDatabase.CreateAsset(asset, path);
		}
	}

	private static void DeleteMissingAssets(IEnumerable<FMODAsset> assetsToDelete) {
		foreach (var asset in assetsToDelete) {
			var path = AssetDatabase.GetAssetPath(asset);
			AssetDatabase.DeleteAsset(path);

			var dir = new System.IO.FileInfo(path).Directory;
			DeleteDirectoryIfEmpty(dir);
		}
	}

	private static void DeleteDirectoryIfEmpty(System.IO.DirectoryInfo dir) {
		Logger.LogMessage("Attempt delete directory: " + dir.FullName);

		if (dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0 && dir.Name != AssetFolder) {
			dir.Delete();
			DeleteDirectoryIfEmpty(dir.Parent);
		}
	}

	private static void UnloadAllBanks() {
		if (StudioSystem.System != null) {
			StudioSystem.UnloadAllBanks();
		} else if (StudioSystem.LoadedBanks.Count != 0) {
			Logger.LogError("Banks not unloaded!");
		}
	}

	private static bool LoadAllBanks() {
		UnloadAllBanks();

		return StudioSystem.LoadAllBanks(true);
	}

	private static void PrepareIntegration() {
		if (!UnityEditorInternal.InternalEditorUtility.HasPro()) {
			Logger.LogMessage("Unity basic license detected: running integration in Basic compatible mode");

			// Copy the FMOD binaries to the root directory of the project
			if (Application.platform == RuntimePlatform.WindowsEditor) {
				var pluginPath = Application.dataPath + "/Plugins/x86/";
				var projectRoot = new System.IO.DirectoryInfo(Application.dataPath).Parent;

				var fmodFile = new System.IO.FileInfo(pluginPath + "fmod.dll");
				if (fmodFile.Exists) {
					var dest = projectRoot.FullName + "/fmod.dll";

					DeleteBinaryFile(dest);
					fmodFile.MoveTo(dest);
				}

				var studioFile = new System.IO.FileInfo(pluginPath + "fmodstudio.dll");
				if (studioFile.Exists) {
					var dest = projectRoot.FullName + "/fmodstudio.dll";

					DeleteBinaryFile(dest);
					studioFile.MoveTo(dest);
				}
			} else if (Application.platform == RuntimePlatform.OSXEditor) {
				var pluginPath = Application.dataPath + "/Plugins/";
				var projectRoot = new System.IO.DirectoryInfo(Application.dataPath).Parent;

				var fmodFile = new System.IO.FileInfo(pluginPath + "fmod.bundle/Contents/MacOS/fmod");
				if (fmodFile.Exists) {
					var dest = projectRoot.FullName + "/fmod.dylib";

					DeleteBinaryFile(dest);
					fmodFile.MoveTo(dest);
				}

				var studioFile = new System.IO.FileInfo(pluginPath + "fmodstudio.bundle/Contents/MacOS/fmodstudio");
				if (studioFile.Exists) {
					var dest = projectRoot.FullName + "/fmodstudio.dylib";

					DeleteBinaryFile(dest);
					studioFile.MoveTo(dest);
				}
			}
		}
	}

	private static void DeleteBinaryFile(string path) {
		if (System.IO.File.Exists(path)) {
			try {
				System.IO.File.Delete(path);
			} catch (System.UnauthorizedAccessException e) {
				EditorUtility.DisplayDialog("Restart Unity",
					"The following file is in use and cannot be overwritten, restart Unity and try again\n" + path, "OK");

				throw e;
			}
		}
	}

	private static string CleanPath(string path) {
		if (path.Contains('\\')) {
			return CleanPath(path.Replace('\\', '/'));
		}
		if (path.Contains("//")) {
			return CleanPath(path.Replace("//", "/"));
		}
		return path;
	}
}

public class FMODAssetGUIDComparer : IEqualityComparer<FMODAsset> {
	public bool Equals(FMODAsset lhs, FMODAsset rhs) {
		return lhs.id.Equals(rhs.id, System.StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode(FMODAsset asset) {
		return asset.id.GetHashCode();
	}
}

public class FMODAssetPathComparer : IEqualityComparer<FMODAsset> {
	public bool Equals(FMODAsset lhs, FMODAsset rhs) {
		return lhs.path.Equals(rhs.path, System.StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode(FMODAsset asset) {
		return asset.path.GetHashCode();
	}
}

#endif
