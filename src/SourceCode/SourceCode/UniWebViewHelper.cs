using System.IO;
using UnityEngine;

public class UniWebViewHelper
{
	public static string StreamingAssetURLForPath(string path)
	{
		UniWebViewLogger.Instance.Critical("The current build target is not supported.");
		return string.Empty;
	}

	public static string PersistentDataURLForPath(string path)
	{
		return Path.Combine("file://" + Application.persistentDataPath, path);
	}
}
