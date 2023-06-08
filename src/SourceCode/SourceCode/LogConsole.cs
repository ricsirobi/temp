using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogConsole : MonoBehaviour
{
	public string _FileName = "DeviceLog";

	public List<string> _ManifestWarningList = new List<string>();

	public bool _EnableLog = true;

	public bool _ShowLog;

	public bool _ShowWarning = true;

	public bool _ShowError = true;

	public bool _ShowManifestWarning;

	public bool _UniqueFile = true;

	public bool _ShowStackTrace = true;

	public float _WriteFrequency = 1f;

	private float mElapsedTime;

	private bool mCanFlush;

	private StreamWriter mWriter;

	private string mFullPath;

	private static int mNumExceptions;

	public string pFullPath
	{
		get
		{
			return mFullPath;
		}
		set
		{
			mFullPath = value;
		}
	}

	public static int pNumExceptions => mNumExceptions;

	private void Awake()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (mCanFlush)
		{
			mElapsedTime += Time.deltaTime;
			if (mElapsedTime >= _WriteFrequency)
			{
				mCanFlush = false;
				mElapsedTime = 0f;
				mWriter.Flush();
			}
		}
	}

	private void OnDestroy()
	{
		if (mWriter != null)
		{
			mWriter.Close();
		}
		Application.logMessageReceived -= OnDebugLogCallbackHandler;
	}

	public string CreateLogFile()
	{
		_UniqueFile = false;
		string text = "";
		if (_UniqueFile)
		{
			DateTime now = DateTime.Now;
			text = "-" + now.Day + "-" + now.Month + "-" + now.Year + "(" + now.Hour + "-" + now.Minute + ").txt";
		}
		else
		{
			text = ".txt";
		}
		if (mWriter != null)
		{
			mWriter.Close();
		}
		mFullPath = Application.persistentDataPath + "/" + _FileName + text;
		if (!_UniqueFile)
		{
			string destFileName = Application.persistentDataPath + "/" + _FileName + "_Prev" + text;
			if (File.Exists(mFullPath))
			{
				File.Copy(mFullPath, destFileName, overwrite: true);
			}
		}
		Debug.Log("Logging to file : " + mFullPath);
		mWriter = new StreamWriter(mFullPath);
		mWriter.WriteLine("Unity Version : " + Application.unityVersion);
		mWriter.WriteLine("Platform : " + Application.platform);
		mWriter.WriteLine("OS Version : " + UtMobileUtilities.GetOSVersion());
		mWriter.WriteLine("Device Name : " + SystemInfo.deviceName);
		mWriter.WriteLine("Device Model : " + SystemInfo.deviceModel);
		mWriter.WriteLine("System Memory(Unity API) : " + SystemInfo.systemMemorySize + " MB");
		mWriter.WriteLine("System Memory(Native API) : " + UtMobileUtilities.FormatBytes(UtMobileUtilities.GetSystemMemory()));
		mWriter.WriteLine("Graphics Memory(Unity API) : " + SystemInfo.graphicsMemorySize + " MB");
		mWriter.WriteLine("Operating System : " + SystemInfo.operatingSystem);
		mWriter.WriteLine("Build Type : CDN");
		mWriter.WriteLine("---------------------------------------------------\n");
		mWriter.AutoFlush = true;
		return _FileName;
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if ((type != LogType.Log || _ShowLog) && (type != LogType.Warning || _ShowWarning) && ((type != 0 && type != LogType.Exception && type != LogType.Assert) || _ShowError) && mWriter != null)
		{
			if (logString.Contains("Exception") && !logString.Contains("SocketException") && !logString.Contains("FileNotFoundException"))
			{
				mNumExceptions++;
			}
			mWriter.WriteLine(logString);
			if (_ShowStackTrace)
			{
				mWriter.WriteLine("");
				mWriter.WriteLine(stackTrace);
			}
			mWriter.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
		}
	}

	private void OnUnresolvedExceptionHandler(object sender, UnhandledExceptionEventArgs args)
	{
		if (args == null || args.ExceptionObject == null || args.ExceptionObject.GetType() != typeof(Exception))
		{
			return;
		}
		mNumExceptions++;
		try
		{
			Exception ex = (Exception)args.ExceptionObject;
			mWriter.WriteLine("UnhandledException");
			mWriter.WriteLine(ex.Source);
			mWriter.WriteLine(ex.Message);
			mWriter.WriteLine(ex.StackTrace);
			mWriter.Flush();
		}
		catch (Exception ex2)
		{
			mWriter.WriteLine(ex2.Message);
		}
	}

	private void OnDebugLogCallbackHandler(string inName, string inStack, LogType inType)
	{
		if (!_EnableLog || (inType == LogType.Log && !_ShowLog))
		{
			return;
		}
		if (inType == LogType.Warning)
		{
			if (!_ShowWarning)
			{
				return;
			}
			if (!_ShowManifestWarning)
			{
				foreach (string manifestWarning in _ManifestWarningList)
				{
					if (inName.Contains(manifestWarning))
					{
						return;
					}
				}
			}
		}
		if (((inType != 0 && inType != LogType.Exception && inType != LogType.Assert) || _ShowError) && mWriter != null)
		{
			if (inName.Contains("Exception"))
			{
				mNumExceptions++;
			}
			mWriter.WriteLine(inName);
			if (_ShowStackTrace)
			{
				mWriter.WriteLine("");
				mWriter.WriteLine(inStack);
			}
			mWriter.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
		}
	}

	public bool ClearLog()
	{
		if (File.Exists(mFullPath))
		{
			mWriter.Close();
			File.Delete(mFullPath);
			CreateLogFile();
			return true;
		}
		return false;
	}
}
