using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ionic.Zlib;
using Microsoft.Applications.Events;
using Microsoft.Applications.Events.DataModels;
using PlayFab.EventsModels;
using PlayFab.Internal;
using PlayFab.SharedModels;

namespace PlayFab;

public class OneDSEventsAPI
{
	private string oneDSProjectIdIkey;

	private string oneDSIngestionKey;

	private string oneDSJwtToken;

	private string oneDSHeaderJwtTicketKey;

	private string oneDSHeaderJwtTicketPrefix;

	public bool IsOneDSAuthenticated { get; private set; }

	public void SetCredentials(string projectIdIkey, string ingestionKey, string jwtToken, string headerJwtTicketKey, string headerJwtTicketPrefix)
	{
		oneDSProjectIdIkey = projectIdIkey;
		oneDSIngestionKey = ingestionKey;
		oneDSJwtToken = jwtToken;
		oneDSHeaderJwtTicketKey = headerJwtTicketKey;
		oneDSHeaderJwtTicketPrefix = headerJwtTicketPrefix;
		IsOneDSAuthenticated = true;
	}

	public void ForgetAllCredentials()
	{
		IsOneDSAuthenticated = false;
		oneDSProjectIdIkey = string.Empty;
		oneDSIngestionKey = string.Empty;
		oneDSJwtToken = string.Empty;
		oneDSHeaderJwtTicketKey = string.Empty;
		oneDSHeaderJwtTicketPrefix = string.Empty;
	}

	public async Task<PlayFabResult<WriteEventsResponse>> WriteTelemetryEventsAsync(WriteEventsRequest request, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		if (request.Events.Count == 0)
		{
			return new PlayFabResult<WriteEventsResponse>
			{
				Error = new PlayFabError
				{
					Error = PlayFabErrorCode.ContentNotFound,
					ErrorMessage = "No events found in request. Please make sure to provide at least one event in the batch.",
					HttpStatus = "OneDSError"
				},
				CustomData = customData
			};
		}
		if (!IsOneDSAuthenticated)
		{
			return new PlayFabResult<WriteEventsResponse>
			{
				Error = new PlayFabError
				{
					Error = PlayFabErrorCode.AuthTokenDoesNotExist,
					ErrorMessage = "OneDS API client is not authenticated. Please make sure OneDS credentials are set.",
					HttpStatus = "OneDSError"
				},
				CustomData = customData
			};
		}
		IOneDSTransportPlugin plugin = PluginManager.GetPlugin<IOneDSTransportPlugin>(PluginContract.PlayFab_Transport, "PLUGIN_TRANSPORT_ONEDS");
		byte[] request2;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			foreach (EventContents @event in request.Events)
			{
				CsEvent csEvent = new CsEvent();
				csEvent.data = new List<Data>();
				if (@event.Payload is Data item)
				{
					csEvent.data.Add(item);
				}
				csEvent.name = @event.Name;
				csEvent.iKey = oneDSProjectIdIkey;
				csEvent.time = DateTime.UtcNow.Ticks;
				csEvent.ver = "3.0";
				csEvent.baseType = string.Empty;
				csEvent.flags = 1L;
				BondHelper.Serialize(csEvent, memoryStream);
			}
			memoryStream.Position = 0L;
			byte[] array = memoryStream.ToArray();
			memoryStream.SetLength(0L);
			using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
			{
				gZipStream.Write(array, 0, array.Length);
			}
			request2 = memoryStream.ToArray();
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["APIKey"] = oneDSIngestionKey;
		dictionary["Tickets"] = "\"" + oneDSHeaderJwtTicketKey + "\": \"" + oneDSHeaderJwtTicketPrefix + ":" + oneDSJwtToken + "\"";
		if (extraHeaders != null)
		{
			foreach (KeyValuePair<string, string> extraHeader in extraHeaders)
			{
				dictionary.Add(extraHeader.Key, extraHeader.Value);
			}
		}
		PlayFabResult<WriteEventsResponse> result = null;
		plugin.DoPost(request2, dictionary, delegate(object httpResult)
		{
			if (httpResult is PlayFabError)
			{
				PlayFabError error = (PlayFabError)httpResult;
				result = new PlayFabResult<WriteEventsResponse>
				{
					Error = error,
					CustomData = customData
				};
			}
			else
			{
				result = new PlayFabResult<WriteEventsResponse>
				{
					Result = new WriteEventsResponse(),
					CustomData = customData
				};
			}
		});
		await WaitWhile(() => result == null, 100);
		return result;
	}

	internal static async Task<PlayFabResult<TelemetryIngestionConfigResponse>> GetTelemetryIngestionConfigAsync(TelemetryIngestionConfigRequest request, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		if (PlayFabSettings.staticPlayer.EntityToken == null)
		{
			throw new PlayFabException(PlayFabExceptionCode.EntityTokenNotSet, "Must call GetEntityToken before calling this method");
		}
		PlayFabResult<TelemetryIngestionConfigResponse> result = null;
		SingletonMonoBehaviour<PlayFabHttp>.instance.InjectInUnityThread(delegate
		{
			PlayFabHttp.MakeApiCall("/Event/GetTelemetryIngestionConfig", request, AuthType.EntityToken, delegate(TelemetryIngestionConfigResponse callback)
			{
				result = new PlayFabResult<TelemetryIngestionConfigResponse>
				{
					Result = callback,
					CustomData = customData
				};
			}, delegate(PlayFabError error)
			{
				result = new PlayFabResult<TelemetryIngestionConfigResponse>
				{
					Error = error,
					CustomData = customData
				};
			}, null, null, PlayFabSettings.staticPlayer);
		});
		await WaitWhile(() => result == null, 100);
		return result;
	}

	public static async System.Threading.Tasks.Task WaitWhile(Func<bool> condition, int frequency = 25, int timeout = -1)
	{
		System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(async delegate
		{
			while (condition())
			{
				await System.Threading.Tasks.Task.Delay(frequency);
			}
		});
		object obj = task;
		if (obj != await System.Threading.Tasks.Task.WhenAny(task, System.Threading.Tasks.Task.Delay(timeout)))
		{
			throw new TimeoutException();
		}
	}
}
