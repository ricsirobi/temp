using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace Xsolla;

public class XsollaJsonGenerator
{
	public struct User
	{
		public string id;

		public string name;

		public string email;

		public string country;
	}

	public struct Settings
	{
		public long id;

		public string languge;

		public string currency;

		public string mode;

		public string secretKey;

		public Ui ui;
	}

	public struct Ui
	{
		public string theme;
	}

	public User user;

	public Settings settings;

	public XsollaJsonGenerator(string userId, long projectId)
	{
		user = default(User);
		settings = default(Settings);
		user.id = userId;
		settings.id = projectId;
	}

	public string GetPrepared()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{").Append("\"user\":{").Append("\"id\":{")
			.Append("\"value\":\"")
			.Append(user.id)
			.Append("\"}")
			.Append(",");
		if (user.name != null)
		{
			stringBuilder.Append("\"name\":{").Append("\"value\":\"").Append(user.name)
				.Append("\"}")
				.Append(",");
		}
		if (user.email != null)
		{
			stringBuilder.Append("\"email\":{").Append("\"value\":\"").Append(user.email)
				.Append("\"}")
				.Append(",");
		}
		if (user.country != null)
		{
			stringBuilder.Append("\"country\":{").Append("\"value\":\"").Append(user.country)
				.Append("\"")
				.Append(",")
				.Append("\"allow_modify\":")
				.Append(value: true)
				.Append("}")
				.Append(",");
		}
		stringBuilder.Length--;
		stringBuilder.Append("}").Append(",");
		stringBuilder.Append("\"settings\":{").Append("\"project_id\":").Append(settings.id)
			.Append(",");
		if (settings.languge != null)
		{
			stringBuilder.Append("\"language\":\"").Append(settings.languge).Append("\"")
				.Append(",");
		}
		if (settings.currency != null)
		{
			stringBuilder.Append("\"currency\":\"").Append(settings.currency).Append("\"")
				.Append(",");
		}
		if (settings.mode == "sandbox")
		{
			stringBuilder.Append("\"mode\":\"sandbox\",");
		}
		if (settings.secretKey != null)
		{
			stringBuilder.Append("\"secretKey\":\"").Append(settings.secretKey).Append("\"")
				.Append(",");
		}
		if (settings.ui.theme != null)
		{
			stringBuilder.Append("\"ui\":{\"theme\":\"").Append(settings.ui.theme).Append("\"}}");
		}
		stringBuilder.Length--;
		stringBuilder.Append("}").Append("}");
		return stringBuilder.ToString();
	}

	public static IEnumerator FreshToken(Action<string> tokenCallback)
	{
		Logger.isLogRequired = true;
		string prepared = new XsollaJsonGenerator("user_1", 14004L)
		{
			user = 
			{
				name = "John Smith",
				email = "support@xsolla.com",
				country = "US"
			},
			settings = 
			{
				currency = "USD",
				languge = "en",
				ui = 
				{
					theme = "default"
				}
			}
		}.GetPrepared();
		string url = "https://livedemo.xsolla.com/sdk/token/";
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("data", prepared);
		WWW www = new WWW(url, wWWForm);
		yield return www;
		if (www.error == null)
		{
			Logger.Log("DEBUG: Last section" + www.text);
			JSONNode jSONNode = JSON.Parse(www.text);
			tokenCallback(jSONNode["token"].Value);
		}
		else
		{
			tokenCallback(null);
		}
	}

	public static string Base64Encode(string plainText)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
	}

	private static byte[] GetBytes(string str)
	{
		byte[] array = new byte[str.Length * 2];
		Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
		return array;
	}

	private static string GetAppPlatformPath()
	{
		string text = "ExecConnect.dll";
		Logger.Log("ApplicationDataPath --> " + Application.dataPath);
		if (Application.platform == RuntimePlatform.Android)
		{
			return "";
		}
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			return Application.dataPath + "/Plugins/" + text;
		}
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			return "Assets/Plugins/" + text;
		}
		if (Application.platform == RuntimePlatform.WindowsPlayer)
		{
			return Application.dataPath + "/Plugins/ExecConnectWin.dll";
		}
		return "";
	}

	private static void CaptureConsoleAppOutput(string exeName, string arguments, int timeoutMilliseconds, out int exitCode, out string output)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = exeName;
		processStartInfo.Arguments = arguments;
		processStartInfo.UseShellExecute = false;
		processStartInfo.RedirectStandardOutput = true;
		processStartInfo.CreateNoWindow = true;
		try
		{
			Process process = Process.Start(processStartInfo);
			using (StreamReader streamReader = process.StandardOutput)
			{
				string text = streamReader.ReadToEnd();
				output = text;
			}
			if (process.WaitForExit(timeoutMilliseconds))
			{
				exitCode = process.ExitCode;
			}
			else
			{
				exitCode = -1;
			}
		}
		catch (Exception ex)
		{
			Logger.Log(ex.Message);
			exitCode = -1;
			output = "";
		}
	}
}
