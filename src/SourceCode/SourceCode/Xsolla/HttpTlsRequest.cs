using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Xsolla;

public class HttpTlsRequest : MonoBehaviour
{
	private static string outputFileName = "requestResult.x";

	public static string loaderGameObjName = "HttpRequestLoader";

	public IEnumerator Request(string pUrl, Dictionary<string, object> pDataDic, Action<RequestClass> onReturnRes)
	{
		Logger.isLogRequired = true;
		RequestClass res = new RequestClass();
		string text = "";
		foreach (KeyValuePair<string, object> item in pDataDic)
		{
			text = text + item.Key + "=" + Uri.EscapeDataString((item.Value == null) ? "" : item.Value.ToString()) + "&";
		}
		text = "?" + text.Substring(0, text.Length - 1);
		string text2 = pUrl + text;
		Logger.Log(GetType().Name + " -> " + text2);
		Logger.Log("ApplicationPlatform -> " + Application.platform);
		Logger.Log("ApplicationDataPath -> " + Application.dataPath);
		switch (Application.platform)
		{
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.LinuxPlayer:
			CaptureConsoleCmdOutput("curl", text2, out res);
			break;
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
			CaptureConsoleCmdOutputWin(Application.dataPath + "/Plugins/ExecConnectWin.dll", text2, out res);
			break;
		case RuntimePlatform.IPhonePlayer:
		case RuntimePlatform.Android:
		case RuntimePlatform.WebGLPlayer:
			Logger.Log("StartCoroutine");
			StartCoroutine(GetWWWFormRequest(pUrl, pDataDic, delegate(RequestClass value)
			{
				onReturnRes(value);
			}));
			break;
		}
		yield return res;
		onReturnRes(res);
	}

	private static IEnumerator GetWWWFormRequest(string pUrl, Dictionary<string, object> pDataDic, Action<RequestClass> onComplite)
	{
		WWWForm wWWForm = new WWWForm();
		foreach (KeyValuePair<string, object> item in pDataDic)
		{
			wWWForm.AddField(item.Key, item.Value.ToString());
		}
		WWW www = new WWW(pUrl, wWWForm);
		yield return www;
		if (www.error == null)
		{
			if (onComplite != null)
			{
				Logger.Log("www.text -> " + www.text);
				onComplite(new RequestClass(www.text, pUrl));
			}
		}
		else if (onComplite != null)
		{
			Logger.Log("www.text.error -> " + www.text);
			onComplite(new RequestClass(www.text, pUrl, pError: true, www.error));
		}
	}

	private static void CaptureConsoleCmdOutputWin(string pExeName, string pArgs, out RequestClass pRes)
	{
		string pRequest = "";
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = pExeName;
		processStartInfo.Arguments = pArgs;
		processStartInfo.UseShellExecute = false;
		processStartInfo.RedirectStandardOutput = true;
		processStartInfo.CreateNoWindow = true;
		try
		{
			Process process = Process.Start(processStartInfo);
			using (StreamReader streamReader = process.StandardOutput)
			{
				pRequest = streamReader.ReadToEnd();
			}
			process.WaitForExit();
			pRes = new RequestClass(pRequest, pArgs);
		}
		catch (Exception ex)
		{
			Logger.Log(ex.Message);
			pRes = new RequestClass("", pArgs, pError: true, ex.Message.ToString());
		}
	}

	private static void CaptureConsoleCmdOutput(string pExeName, string pArgs, out RequestClass pRes)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = pExeName;
		processStartInfo.Arguments = "--globoff --output " + outputFileName + " " + pArgs;
		processStartInfo.UseShellExecute = false;
		processStartInfo.RedirectStandardOutput = true;
		processStartInfo.CreateNoWindow = true;
		try
		{
			string pRequest = "";
			Process process = Process.Start(processStartInfo);
			process.WaitForExit();
			_ = process.StandardOutput;
			try
			{
				using StreamReader streamReader = new StreamReader(outputFileName);
				pRequest = streamReader.ReadToEnd();
			}
			catch (Exception ex)
			{
				Console.WriteLine("The file could not be read:");
				Console.WriteLine(ex.Message);
			}
			pRes = new RequestClass(pRequest, pArgs);
		}
		catch (Exception ex2)
		{
			Logger.Log(ex2.Message);
			pRes = new RequestClass("", pArgs, pError: true, ex2.Message.ToString());
		}
	}
}
