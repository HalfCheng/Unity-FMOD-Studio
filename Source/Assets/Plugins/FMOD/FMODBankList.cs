using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// ScriptableObject containing a list of paths to FMOD Banks.
/// </summary>
public class FMODBankList : ScriptableObject {
	#region Constants
	// ----------------------------------------------------------------------------------------------------
	private const string AssetName = "FMODBankList";
	public const string AssetPathResource = "FMOD/" + AssetName;
#if UNITY_EDITOR
	public const string AssetPath = "Assets/Resources/" + AssetPathResource + ".asset";
#endif
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Fields & Properties
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Gets the bank list.
	/// </summary>
	/// <value>
	/// The bank list.
	/// </value>
	public List<string> BankList {
		get { return this.bankList; }
		set { this.bankList = value; }
	}
	[SerializeField]
	private List<string> bankList;
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Public Methods
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Loads the bank list and returns it.
	/// </summary>
	/// <returns></returns>
	public static FMODBankList LoadBankList() {
		return Resources.Load<FMODBankList>(AssetPathResource);
	}

	/// <summary>
	/// Generates the bank list.
	/// </summary>
	/// <param name="fileList">The file list.</param>
	public void GenerateBankList(List<System.IO.FileInfo> fileList) {
		if (this.bankList != null) {
			this.bankList.Clear();
		} else {
			this.bankList = new List<string>(fileList.Count);
		}

		foreach (var fileInfo in fileList) {
			//if (Path.GetFileName(fileInfo.FullName.ToLower()).Contains(".strings")) {
			//if (Path.GetFileNameWithoutExtension(fileInfo.FullName.ToLower()).Contains(".strings")) {
			//    continue; // Skip the stale strings bank
			//}

			System.IO.Directory.CreateDirectory(Path.Combine(Application.dataPath, "StreamingAssets"));
			string oldBankPath = Path.Combine(Application.dataPath, Path.Combine("StreamingAssets", fileInfo.Name));
			fileInfo.CopyTo(oldBankPath, true);

			bankList.Add(fileInfo.Name);
		}
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion
}

