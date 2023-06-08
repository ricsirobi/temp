using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Applications.Events;
using UnityEngine.Networking;

namespace PlayFab.Internal;

public class OneDsUnityHttpPlugin : IOneDSTransportPlugin, IPlayFabPlugin
{
	public void DoPost(object request, Dictionary<string, string> extraHeaders, Action<object> callback)
	{
		SingletonMonoBehaviour<PlayFabHttp>.instance.InjectInUnityThread(Post(request, extraHeaders, callback));
	}

	public IEnumerator Post(object request, Dictionary<string, string> extraHeaders, Action<object> callback)
	{
		UnityWebRequest webRequest = new UnityWebRequest("https://self.events.data.microsoft.com/OneCollector/1.0/", "POST");
		webRequest.uploadHandler = new UploadHandlerRaw(request as byte[]);
		webRequest.downloadHandler = new DownloadHandlerBuffer();
		string value = Utils.MsFrom1970().ToString();
		extraHeaders.Add("sdk-version", "OCT_C#-0.11.1.0");
		extraHeaders.Add("Content-Encoding", "gzip");
		extraHeaders.Add("Content-Type", "application/bond-compact-binary");
		extraHeaders.Add("Upload-Time", value);
		extraHeaders.Add("client-time-epoch-millis", value);
		extraHeaders.Add("Client-Id", "NO_AUTH");
		foreach (KeyValuePair<string, string> extraHeader in extraHeaders)
		{
			webRequest.SetRequestHeader(extraHeader.Key, extraHeader.Value);
		}
		webRequest.chunkedTransfer = false;
		yield return webRequest.SendWebRequest();
		OneDsUtility.ParseResponse(webRequest.responseCode, () => webRequest.downloadHandler.text, webRequest.error, callback);
	}
}
