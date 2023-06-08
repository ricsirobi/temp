using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace JSGames.Platform.PlayFab;

public class AnalyticAgent
{
	public void SendPlayerEvent(string eventName, Dictionary<string, object> body)
	{
		PlayFabClientAPI.WritePlayerEvent(new WriteClientPlayerEventRequest
		{
			EventName = eventName,
			Body = body
		}, delegate
		{
			UtDebug.Log("PlayFab SendPlayerEvent Success");
		}, delegate(PlayFabError error)
		{
			Debug.LogError(error.GenerateErrorReport());
		});
	}

	public void SendTitleEvent(string eventName, Dictionary<string, object> body)
	{
		PlayFabClientAPI.WriteTitleEvent(new WriteTitleEventRequest
		{
			EventName = eventName,
			Body = body
		}, null, null);
	}

	public void SendCharacterEvent(string eventName, string CharacterId, Dictionary<string, object> body)
	{
		PlayFabClientAPI.WriteCharacterEvent(new WriteClientCharacterEventRequest
		{
			EventName = eventName,
			CharacterId = CharacterId,
			Body = body
		}, delegate
		{
			UtDebug.Log("PlayFab SendCharacterEvent Success");
		}, delegate(PlayFabError error)
		{
			Debug.LogError(error.GenerateErrorReport());
		});
	}
}
