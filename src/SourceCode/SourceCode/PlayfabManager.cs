using System;
using System.Collections.Generic;
using JSGames.Platform;
using JSGames.Platform.PlayFab;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using UnityEngine;

public class PlayfabManager<T> where T : PlayfabManager<T>, new()
{
	public class PlayfabCharacterInfo
	{
		public string CharacterName;

		public string CharacterId;

		public string CharacterType;

		public string UserID;

		public string Deleted;
	}

	protected List<StatisticValue> mParentStatistics;

	protected Dictionary<string, int> mCurrentAvatarStatistics;

	protected bool mCurrentCharacterSelected;

	private static T mInstance;

	protected Login mPlayFabLogin;

	protected List<string> mPlayfabCharacterList = new List<string>();

	protected List<PlayfabCharacterInfo> mCharacters;

	protected PlayfabCharacterInfo mCurrentCharacter;

	public static bool EnablePlayfab { get; set; }

	public static T Instance
	{
		get
		{
			if (!EnablePlayfab)
			{
				return null;
			}
			if (mInstance == null)
			{
				mInstance = new T();
			}
			return mInstance;
		}
	}

	public string ParentToken { get; set; }

	public void LoginUser(string UserID, bool guest, object userData)
	{
		Reset();
		mPlayFabLogin = new Login();
		mPlayFabLogin.LoginUser(UserID, guest, OnPlayFabLogin, userData);
	}

	public void OnPlayFabLogin(string type, JSGames.Platform.EventType eventType, object result, object userData)
	{
		if (!(type == "LOGIN_USER"))
		{
			if (type == "LOGIN_GUEST")
			{
				UtDebug.Log("Playfab Login Guest :: " + eventType);
				UpdatePlayerStatistics("Guest", 1);
			}
		}
		else
		{
			UtDebug.Log("PlayFab Login User :: " + eventType);
			UpdatePlayerStatistics("Guest", 0);
		}
		if (eventType == JSGames.Platform.EventType.COMPLETE)
		{
			GetCharacters();
			GetPlayerStatistics();
		}
	}

	public virtual void RegisterChild(RegisterChildRequest request)
	{
		if (request != null && !string.IsNullOrEmpty(request.Name) && !string.IsNullOrEmpty(request.Type) && mPlayFabLogin != null)
		{
			mPlayFabLogin.RegisterChild(request, OnPlayFabRegisterChild, null);
			mCurrentCharacterSelected = false;
		}
	}

	private void OnPlayFabRegisterChild(string type, JSGames.Platform.EventType eventType, object result, object userData)
	{
		UtDebug.Log("PlayFab Register Child :: " + eventType);
		if (eventType == JSGames.Platform.EventType.COMPLETE && type == "REGISTER_CHILD")
		{
			mPlayfabCharacterList.Add((string)result);
			UpdatePlayerData();
		}
	}

	public void UpdatePlayerData()
	{
		string value = PlayFabSimpleJson.SerializeObject(mPlayfabCharacterList);
		PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
		{
			Data = new Dictionary<string, string> { { "CharacterIDs", value } }
		}, delegate
		{
			GetCharacters();
			Debug.Log("Successfully updated user data");
		}, delegate(PlayFabError error)
		{
			Debug.Log("Got error setting user data Ancestor to Arthur");
			Debug.Log(error.GenerateErrorReport());
		});
	}

	protected bool ChildCharacterExist(string childUserID)
	{
		if (mCharacters != null && mCharacters.Count > 0 && mCharacters.Find((PlayfabCharacterInfo x) => x.UserID == childUserID) != null)
		{
			return true;
		}
		return false;
	}

	public string GetCurrentCharacterID()
	{
		if (mCurrentCharacter != null)
		{
			return mCurrentCharacter.CharacterId;
		}
		return null;
	}

	protected void GetCharacters()
	{
		GetCharactersForPlayer();
	}

	protected void GetCharactersForPlayer()
	{
		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
		{
			FunctionName = "GetCharactersForPlayer",
			GeneratePlayStreamEvent = true
		}, OnGetCharactersSuccess, OnGetCharactersFail);
	}

	private void OnGetCharactersFail(PlayFabError obj)
	{
		Debug.LogError("PlayFab " + obj.GenerateErrorReport());
	}

	private void OnGetCharactersSuccess(ExecuteCloudScriptResult obj)
	{
		if (obj != null)
		{
			if (obj.Error != null)
			{
				Debug.LogError("PlayFab OnGetCharacters " + obj.Error.Error + " :: " + obj.Error.Message);
			}
			else
			{
				if (obj.FunctionResult == null)
				{
					return;
				}
				UtDebug.Log("PlayFab OnGetCharacters Success :: " + obj.FunctionResult.ToString());
				JsonObject jsonObject = (JsonObject)obj.FunctionResult;
				if (jsonObject == null || !jsonObject.ContainsKey("charactersInfo"))
				{
					return;
				}
				if (mCharacters != null)
				{
					mCharacters.Clear();
				}
				else
				{
					mCharacters = new List<PlayfabCharacterInfo>();
				}
				JsonObject jsonObject2 = (JsonObject)jsonObject["charactersInfo"];
				if (jsonObject2 != null)
				{
					for (int i = 0; i < jsonObject2.Keys.Count; i++)
					{
						mCharacters.Add(PlayFabSimpleJson.DeserializeObject<PlayfabCharacterInfo>(jsonObject2[i].ToString()));
					}
					if (!mCurrentCharacterSelected && mPlayFabLogin.JSPlayer.ChildUserID != null)
					{
						UpdateCurrentCharacter();
					}
				}
			}
		}
		else
		{
			Debug.LogError("PlayFab OnGetCharacters Fail");
		}
	}

	protected void UpdateCurrentCharacter()
	{
		PlayfabCharacterInfo playfabCharacterInfo = mCharacters.Find((PlayfabCharacterInfo x) => x.UserID == mPlayFabLogin.JSPlayer.ChildUserID);
		if (playfabCharacterInfo != null)
		{
			mCurrentCharacter = playfabCharacterInfo;
			GetCharacterStatistics();
		}
		else
		{
			mCurrentCharacter = null;
		}
		mCurrentCharacterSelected = true;
	}

	protected void GetCharacterStatistics()
	{
		if (mCurrentCharacter != null)
		{
			PlayFabClientAPI.GetCharacterStatistics(new GetCharacterStatisticsRequest
			{
				CharacterId = mCurrentCharacter.CharacterId
			}, OnGetCharacterStatistics, delegate(PlayFabError error)
			{
				Debug.LogError("PlayFab " + error.GenerateErrorReport());
			});
		}
	}

	private void OnGetCharacterStatistics(GetCharacterStatisticsResult result)
	{
		if (result != null && result.CharacterStatistics != null)
		{
			mCurrentAvatarStatistics = new Dictionary<string, int>(result.CharacterStatistics);
		}
	}

	protected void GetPlayerStatistics()
	{
		PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(), OnGetPlayerStatistics, delegate(PlayFabError error)
		{
			Debug.LogError("PlayFab " + error.GenerateErrorReport());
		});
	}

	private void OnGetPlayerStatistics(GetPlayerStatisticsResult result)
	{
		if (mParentStatistics == null)
		{
			mParentStatistics = new List<StatisticValue>();
		}
		else
		{
			mParentStatistics.Clear();
		}
		if (result != null && result.Statistics != null)
		{
			mParentStatistics.AddRange(result.Statistics);
		}
	}

	public void UpdatePlayerStatistics(string StatisticName, int value)
	{
		if (mParentStatistics != null)
		{
			StatisticValue statisticValue = mParentStatistics.Find((StatisticValue x) => x.StatisticName == StatisticName);
			if (statisticValue != null && statisticValue.Value == value)
			{
				return;
			}
		}
		PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
		{
			Statistics = new List<StatisticUpdate>
			{
				new StatisticUpdate
				{
					StatisticName = StatisticName,
					Value = value
				}
			}
		}, delegate
		{
			UtDebug.Log("PlayFab UpdatePlayerStatistics Success");
		}, delegate(PlayFabError error)
		{
			Debug.LogError("PlayFab " + error.GenerateErrorReport());
		});
	}

	public void UpdateCharacterStatistics(string StatisticName, int value)
	{
		if (mCurrentCharacter != null && (mCurrentAvatarStatistics == null || !mCurrentAvatarStatistics.ContainsKey(StatisticName) || mCurrentAvatarStatistics[StatisticName] != value))
		{
			PlayFabClientAPI.UpdateCharacterStatistics(new UpdateCharacterStatisticsRequest
			{
				CharacterId = mCurrentCharacter.CharacterId,
				CharacterStatistics = new Dictionary<string, int> { { StatisticName, value } }
			}, delegate
			{
				UtDebug.Log("PlayFab UpdateCharacterStatistics Success");
			}, delegate(PlayFabError error)
			{
				Debug.LogError("PlayFab " + error.GenerateErrorReport());
			});
		}
	}

	public void UpdateCharacterData(string DataKey, string value, string userID)
	{
		PlayFabClientAPI.UpdateCharacterData(new UpdateCharacterDataRequest
		{
			CharacterId = userID,
			Data = new Dictionary<string, string> { { DataKey, value } }
		}, delegate
		{
			UtDebug.Log("PlayFab UpdateCharacterData Success");
		}, delegate(PlayFabError error)
		{
			Debug.LogError("PlayFab " + error.GenerateErrorReport());
		});
	}

	public void AddTagToPlayer(string tagName, System.EventHandler callback, object userData)
	{
		if (!string.IsNullOrEmpty(tagName))
		{
			PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
			{
				FunctionName = "addTagToPlayer",
				FunctionParameter = new
				{
					tag = tagName
				},
				GeneratePlayStreamEvent = true
			}, OnTagSuccess, OnTagFail);
		}
		else
		{
			UtDebug.LogError("Tag name cannot be null or empty");
		}
	}

	private void OnTagFail(PlayFabError obj)
	{
		if (obj != null)
		{
			UtDebug.Log("PlayFab " + obj.GenerateErrorReport());
		}
	}

	private void OnTagSuccess(ExecuteCloudScriptResult obj)
	{
		if (obj != null)
		{
			if (obj.Error != null)
			{
				Debug.LogError("PlayFab AddTagToPlayer " + obj.Error.Error + " :: " + obj.Error.Message);
			}
			else if (obj.FunctionResult != null)
			{
				UtDebug.Log("PlayFab AddTagToPlayer Success :: " + obj.FunctionResult.ToString());
			}
		}
		else
		{
			Debug.LogError("PlayFab AddTagToPlayer Fail");
		}
	}

	public void DeleteCharacter(string childUserID)
	{
		if (mCharacters != null && mCharacters.Count > 0)
		{
			PlayfabCharacterInfo playfabCharacterInfo = mCharacters.Find((PlayfabCharacterInfo x) => x.UserID == childUserID);
			if (playfabCharacterInfo != null)
			{
				playfabCharacterInfo.Deleted = "1";
				UpdateCharacterData("Deleted", playfabCharacterInfo.Deleted, childUserID);
				mPlayfabCharacterList.Remove(playfabCharacterInfo.CharacterId);
				UpdatePlayerData();
			}
		}
	}

	protected void Reset()
	{
		ParentToken = null;
		if (mCharacters != null)
		{
			mCharacters.Clear();
		}
		mCurrentCharacterSelected = false;
		mCurrentCharacter = null;
		mPlayFabLogin = null;
	}

	public virtual void OnSceneLoaded()
	{
	}

	protected static void Destroy()
	{
		mInstance = null;
	}
}
