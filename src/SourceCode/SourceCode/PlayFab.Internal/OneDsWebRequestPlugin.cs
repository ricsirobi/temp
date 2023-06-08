using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Applications.Events;

namespace PlayFab.Internal;

public class OneDsWebRequestPlugin : IOneDSTransportPlugin, IPlayFabPlugin
{
	public void DoPost(object request, Dictionary<string, string> extraHeaders, Action<object> callback)
	{
		new Thread((ThreadStart)delegate
		{
			string value = Utils.MsFrom1970().ToString();
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://self.events.data.microsoft.com/OneCollector/1.0/");
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/bond-compact-binary";
			httpWebRequest.Headers.Add("sdk-version", "OCT_C#-0.11.1.0");
			httpWebRequest.Headers.Add("Content-Encoding", "gzip");
			httpWebRequest.Headers.Add("Upload-Time", value);
			httpWebRequest.Headers.Add("client-time-epoch-millis", value);
			httpWebRequest.Headers.Add("Client-Id", "NO_AUTH");
			foreach (KeyValuePair<string, string> extraHeader in extraHeaders)
			{
				httpWebRequest.Headers.Add(extraHeader.Key, extraHeader.Value);
			}
			if (request is byte[] array)
			{
				httpWebRequest.ContentLength = array.Length;
				using Stream stream = httpWebRequest.GetRequestStream();
				stream.Write(array, 0, array.Length);
			}
			try
			{
				HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
				OneDsUtility.ParseResponse((long)response.StatusCode, delegate
				{
					using StreamReader streamReader2 = new StreamReader(response.GetResponseStream());
					return streamReader2.ReadToEnd();
				}, null, callback);
			}
			catch (WebException ex)
			{
				try
				{
					using Stream stream2 = ex.Response.GetResponseStream();
					if (stream2 != null)
					{
						using (StreamReader streamReader = new StreamReader(stream2))
						{
							callback?.Invoke(new PlayFabError
							{
								Error = PlayFabErrorCode.Unknown,
								ErrorMessage = streamReader.ReadToEnd()
							});
							return;
						}
					}
				}
				catch (Exception ex2)
				{
					callback?.Invoke(new PlayFabError
					{
						Error = PlayFabErrorCode.Unknown,
						ErrorMessage = ex2.Message
					});
				}
			}
			catch (Exception ex3)
			{
				callback?.Invoke(new PlayFabError
				{
					Error = PlayFabErrorCode.Unknown,
					ErrorMessage = ex3.Message
				});
			}
		}).Start();
	}
}
