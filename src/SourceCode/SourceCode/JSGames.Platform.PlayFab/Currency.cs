using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;

namespace JSGames.Platform.PlayFab;

public class Currency
{
	public class CurrencyResponse
	{
		public string PlayFabId;

		public string CharacterId;

		public string VirtualCurrency;

		public int Balance;

		public int BalanceChange;
	}

	public enum Action
	{
		Add,
		Subtract
	}

	public void UpdateCharacterVirtualCurrency(int amount, string characterId, string currencyType, Action currencyOperation, EventHandler callback, object userData)
	{
		new ServiceRequest("UPDATE_CHARACTER_CURRENCY", callback, userData);
		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
		{
			FunctionName = "updateCharacterVirtualCurrency",
			FunctionParameter = new
			{
				amount = amount,
				character = characterId,
				currency = currencyType,
				type = (int)currencyOperation
			},
			GeneratePlayStreamEvent = true
		}, OnCurrencyUpdate, OnError);
	}

	public void UpdatePlayerVirtualCurrency(int amount, string currencyType, Action currencyOperation, EventHandler callback, object userData)
	{
		new ServiceRequest("UPDATE_PLAYER_CURRENCY", callback, userData);
		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
		{
			FunctionName = "updatePlayerVirtualCurrency",
			FunctionParameter = new
			{
				amount = amount,
				currency = currencyType,
				type = (int)currencyOperation
			},
			GeneratePlayStreamEvent = true
		}, OnCurrencyUpdate, OnError);
	}

	private void OnCurrencyUpdate(ExecuteCloudScriptResult result)
	{
		ServiceRequest value = ServiceRequest.GetValue("UPDATE_PLAYER_CURRENCY");
		if (value == null)
		{
			value = ServiceRequest.GetValue("UPDATE_CHARACTER_CURRENCY");
		}
		if (value == null)
		{
			return;
		}
		if (result.Error != null && result.Logs.Count > 0)
		{
			string responseObj = string.Empty;
			foreach (LogStatement log in result.Logs)
			{
				if (log.Level == "Error")
				{
					responseObj = log.Message;
					break;
				}
			}
			if (value._EventDelegate != null)
			{
				value._EventDelegate(value._Type, EventType.ERROR, responseObj, value._UserData);
			}
		}
		else
		{
			((JsonObject)result.FunctionResult).TryGetValue("result", out var value2);
			CurrencyResponse responseObj2 = PlayFabSimpleJson.DeserializeObject<CurrencyResponse>(value2.ToString());
			if (value._EventDelegate != null)
			{
				value._EventDelegate(value._Type, EventType.COMPLETE, responseObj2, value._UserData);
			}
		}
		ServiceRequest.RemoveValue(value._Type);
	}

	private void OnError(PlayFabError error)
	{
	}
}
