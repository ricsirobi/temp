using System;

namespace KA.Framework;

[Serializable]
public class EnvironmentDetails
{
	public Environment _Environment;

	public string _MainXMLPath;

	public string _EditorMainXMLPath;

	public string _Secret;

	public int _DownloadTimeOutInSecs = 30;

	public int _PostTimeOutInSecs = 30;
}
