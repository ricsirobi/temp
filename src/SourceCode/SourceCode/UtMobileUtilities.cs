using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

public class UtMobileUtilities
{
	public const double mOneGigaByte = 1073741824.0;

	public const double mOneMegaByte = 1048576.0;

	public const double mOneKiloByte = 1024.0;

	private static List<MonoBehaviour> mPersistentScript = new List<MonoBehaviour>();

	public static bool IsKeyboardVisible()
	{
		return false;
	}

	public static int GetDeviceMemoryInUse()
	{
		return 0;
	}

	public static int GetDeviceFreeMemory()
	{
		return 0;
	}

	public static string GetUniqueID()
	{
		string text = "";
		if (PlayerPrefs.HasKey("UNIQRE_ID"))
		{
			text = PlayerPrefs.GetString("UNIQRE_ID");
			if (text != "")
			{
				return text;
			}
		}
		text = Guid.NewGuid().ToString();
		PlayerPrefs.SetString("UNIQRE_ID", text);
		return text;
	}

	public static bool IsValidDOB(DateTime inDOB, int inMinAge)
	{
		int num = DateTime.Now.Year - inDOB.Year;
		if (DateTime.Now.Month < inDOB.Month || (DateTime.Now.Month == inDOB.Month && DateTime.Now.Day < inDOB.Day))
		{
			num--;
		}
		return num >= inMinAge;
	}

	public static long GetMemoryInUse()
	{
		return 0L;
	}

	public static long GetPhysicalMemory()
	{
		return GetSystemMemory();
	}

	public static long GetGCMemory()
	{
		return GC.GetTotalMemory(forceFullCollection: false);
	}

	public static long GetFreeMemory()
	{
		return 0L;
	}

	public static long GetUnityTotalDeviceMemory()
	{
		long num = (long)SystemInfo.graphicsMemorySize + (long)SystemInfo.systemMemorySize;
		if (num < 0)
		{
			num = 3072L;
		}
		return num;
	}

	public static void PauseGame(bool inPause)
	{
	}

	public static long GetSystemMemory()
	{
		return 1073741824L;
	}

	public static float GetCPUTime()
	{
		return 0f;
	}

	public static bool IsMultiTouch()
	{
		if (UtPlatform.IsMobile() || UtPlatform.IsWSA())
		{
			return KAInput.touchCount > 1;
		}
		return false;
	}

	public static string GetOSVersion()
	{
		return "";
	}

	public static bool IsChartBoostSupported()
	{
		if (Application.isEditor)
		{
			return false;
		}
		return true;
	}

	public static void AddToPersistentScriptList(MonoBehaviour inScript)
	{
		if (!mPersistentScript.Contains(inScript))
		{
			mPersistentScript.Add(inScript);
		}
	}

	public static void RemoveFromPersistentScriptList(MonoBehaviour inScript)
	{
		if (inScript != null)
		{
			mPersistentScript.Remove(inScript);
		}
	}

	public static void DisableAllScripts(bool inAll)
	{
		if (!(Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour)) is MonoBehaviour[] array))
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (inAll || (array[i] != null && !mPersistentScript.Contains(array[i])))
			{
				array[i].enabled = false;
				array[i].StopAllCoroutines();
				UtDebug.Log(" Disableing script : " + array[i].gameObject.name + " Type: " + array[i].GetType());
			}
		}
	}

	public static void SaveToFile(string fileName, Texture2D texture)
	{
		byte[] buffer = texture.EncodeToPNG();
		using BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(fileName));
		binaryWriter.Write(buffer);
		binaryWriter.Close();
	}

	public static Texture2D LoadFromFile(string fileName)
	{
		if (File.Exists(fileName))
		{
			using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(fileName)))
			{
				byte[] data = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
				binaryReader.Close();
				Texture2D texture2D = new Texture2D(256, 256);
				texture2D.LoadImage(data);
				texture2D.name = fileName;
				return texture2D;
			}
		}
		return null;
	}

	public static bool CanLoadInCurrentScene(UiType inUiType, UILoadOptions inOptions)
	{
		return inOptions switch
		{
			UILoadOptions.CURRENT_SCENE => true, 
			UILoadOptions.NEW_SCENE => false, 
			_ => true, 
		};
	}

	public static bool IsWideDisplay()
	{
		if (KAUIManager.pInstance != null && KAUIManager.pInstance.camera != null)
		{
			return Mathf.Round(KAUIManager.pInstance.camera.aspect * 100f) / 100f > 1.5f;
		}
		return false;
	}

	public static int ExtractNumber(string inString)
	{
		return int.Parse(inString.Trim("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()));
	}

	public static string FormatBytes(long bytes)
	{
		string text = " Bytes";
		double num = 0.0;
		if ((double)bytes >= 1073741824.0)
		{
			num = (double)bytes / 1073741824.0;
			text = " GB";
		}
		else if ((double)bytes >= 1048576.0)
		{
			num = (double)bytes / 1048576.0;
			text = " MB";
		}
		else if ((double)bytes >= 1024.0)
		{
			num = (double)bytes / 1024.0;
			text = " KB";
		}
		return num.ToString("0.0") + text;
	}

	public static int GetTextureMemoryInt(Texture inTex)
	{
		if (inTex == null)
		{
			return -1;
		}
		return (int)Profiler.GetRuntimeMemorySizeLong(inTex);
	}

	public static string GetTextureMemory(Texture2D inTex)
	{
		if (inTex == null)
		{
			return "NA";
		}
		return FormatBytes((int)((float)(inTex.width * inTex.height) * GetBpp(inTex.format)));
	}

	public static float GetBpp(TextureFormat texFormat)
	{
		switch (texFormat)
		{
		case TextureFormat.PVRTC_RGB2:
		case TextureFormat.PVRTC_RGBA2:
			return 0.25f;
		case TextureFormat.DXT1:
		case TextureFormat.PVRTC_RGB4:
		case TextureFormat.PVRTC_RGBA4:
			return 0.5f;
		case TextureFormat.Alpha8:
		case TextureFormat.DXT5:
			return 1f;
		case TextureFormat.RGB24:
			return 3f;
		case TextureFormat.RGBA32:
		case TextureFormat.ARGB32:
		case TextureFormat.ETC_RGB4:
			return 4f;
		case TextureFormat.ETC2_RGBA8:
			return 8f;
		default:
			return 1f;
		}
	}

	public static void TrackLog(string inMsg)
	{
	}

	public static Rect GetUiSafeAreaRect()
	{
		Rect safeArea = Screen.safeArea;
		float x = safeArea.min.x;
		float width = safeArea.width;
		float num = safeArea.width * (float)Screen.height / (float)Screen.width;
		float y = ((float)Screen.height - num) * 0.5f;
		float height = num;
		UtDebug.Log(" Safe Area Values: vx- " + x + " vy- " + y + " vw- " + width + " vh- " + height);
		return new Rect(x, y, width, height);
	}

	public static float GetSafeAreaHeightRatio()
	{
		if (UtPlatform.IsMobile() && Screen.safeArea.height != (float)Screen.height)
		{
			return ((float)Screen.height - (Screen.safeArea.height - Screen.safeArea.yMin)) / (float)Screen.height;
		}
		return 0f;
	}
}
