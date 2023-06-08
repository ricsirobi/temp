using System;
using PlayFab.Json;

namespace PlayFab.Internal;

public static class OneDsUtility
{
	public const string ONEDS_SERVICE_URL = "https://self.events.data.microsoft.com/OneCollector/1.0/";

	public static void ParseResponse(long httpCode, Func<string> getText, string errorString, Action<object> callback)
	{
		if (!string.IsNullOrEmpty(errorString))
		{
			callback(new PlayFabError
			{
				Error = PlayFabErrorCode.Unknown,
				ErrorMessage = errorString
			});
			return;
		}
		string text;
		try
		{
			text = getText();
		}
		catch (Exception ex)
		{
			PlayFabError playFabError = new PlayFabError();
			playFabError.Error = PlayFabErrorCode.ConnectionError;
			playFabError.ErrorMessage = ex.Message;
			callback?.Invoke(playFabError);
			return;
		}
		if (httpCode >= 200 && httpCode < 300)
		{
			JsonObject jsonObject = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).DeserializeObject(text) as JsonObject;
			try
			{
				if (ulong.Parse(jsonObject["acc"].ToString()) != 0)
				{
					callback?.Invoke(text);
					return;
				}
				PlayFabError playFabError2 = new PlayFabError();
				playFabError2.HttpCode = (int)httpCode;
				playFabError2.HttpStatus = text;
				playFabError2.Error = PlayFabErrorCode.PartialFailure;
				playFabError2.ErrorMessage = "OneDS server did not accept events";
				callback?.Invoke(playFabError2);
				return;
			}
			catch (Exception ex2)
			{
				PlayFabError playFabError3 = new PlayFabError();
				playFabError3.HttpCode = (int)httpCode;
				playFabError3.HttpStatus = text;
				playFabError3.Error = PlayFabErrorCode.JsonParseError;
				playFabError3.ErrorMessage = "Failed to parse response from OneDS server: " + ex2.Message;
				callback?.Invoke(playFabError3);
				return;
			}
		}
		if ((httpCode >= 500 && httpCode != 501 && httpCode != 505) || httpCode == 408 || httpCode == 429)
		{
			PlayFabError playFabError4 = new PlayFabError();
			playFabError4.HttpCode = (int)httpCode;
			playFabError4.HttpStatus = text;
			playFabError4.Error = PlayFabErrorCode.UnknownError;
			playFabError4.ErrorMessage = "Failed to send a batch of events to OneDS";
			callback?.Invoke(playFabError4);
		}
		else
		{
			PlayFabError playFabError5 = new PlayFabError();
			playFabError5.HttpCode = (int)httpCode;
			playFabError5.HttpStatus = text;
			playFabError5.Error = PlayFabErrorCode.UnknownError;
			playFabError5.ErrorMessage = "Failed to send a batch of events to OneDS";
			callback?.Invoke(playFabError5);
		}
	}
}
