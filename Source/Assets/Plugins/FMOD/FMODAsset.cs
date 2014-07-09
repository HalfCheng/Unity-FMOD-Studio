using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject containing the path and ID to an FMOD asset.
/// </summary>
public class FMODAsset : ScriptableObject {
	public string path;
	public string id; // Note: Variable name 'guid' is not allowed in Unity [Already uses internal variable named 'GUID' - Visible under "Debug" mode in Inspector]
};
