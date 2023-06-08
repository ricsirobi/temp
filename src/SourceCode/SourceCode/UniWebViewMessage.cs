using System;
using System.Collections.Generic;
using UnityEngine.Networking;

public struct UniWebViewMessage
{
	public string RawMessage { get; private set; }

	public string Scheme { get; private set; }

	public string Path { get; private set; }

	public Dictionary<string, string> Args { get; private set; }

	public UniWebViewMessage(string rawMessage)
	{
		this = default(UniWebViewMessage);
		UniWebViewLogger.Instance.Debug("Try to parse raw message: " + rawMessage);
		RawMessage = rawMessage;
		string[] array = rawMessage.Split(new string[1] { "://" }, StringSplitOptions.None);
		if (array.Length >= 2)
		{
			Scheme = array[0];
			UniWebViewLogger.Instance.Debug("Get scheme: " + Scheme);
			string text = "";
			for (int i = 1; i < array.Length; i++)
			{
				text += array[i];
			}
			UniWebViewLogger.Instance.Verbose("Build path and args string: " + text);
			string[] array2 = text.Split("?"[0]);
			Path = UnityWebRequest.UnEscapeURL(array2[0].TrimEnd('/'));
			Args = new Dictionary<string, string>();
			if (array2.Length <= 1)
			{
				return;
			}
			string[] array3 = array2[1].Split("&"[0]);
			for (int j = 0; j < array3.Length; j++)
			{
				string[] array4 = array3[j].Split("="[0]);
				if (array4.Length > 1)
				{
					string text2 = UnityWebRequest.UnEscapeURL(array4[0]);
					if (Args.ContainsKey(text2))
					{
						string text3 = Args[text2];
						Args[text2] = text3 + "," + UnityWebRequest.UnEscapeURL(array4[1]);
					}
					else
					{
						Args[text2] = UnityWebRequest.UnEscapeURL(array4[1]);
					}
					UniWebViewLogger.Instance.Debug("Get arg, key: " + text2 + " value: " + Args[text2]);
				}
			}
		}
		else
		{
			UniWebViewLogger.Instance.Critical("Bad url scheme. Can not be parsed to UniWebViewMessage: " + rawMessage);
		}
	}
}
