using JSGames.Platform.PlayFab;
using KA.Framework;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManagerDO : PlayfabManager<PlayFabManagerDO>
{
	public void CreatePlayfabCharacter(RegisterChildRequest request)
	{
		mPlayFabLogin.JSPlayer.ChildName = request.Name;
		mPlayFabLogin.JSPlayer.ChildUserID = request.UserID;
		mPlayFabLogin.JSPlayer.ChildToken = request.Token;
		mPlayFabLogin.JSPlayer.ChildType = request.Type;
		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
		{
			FunctionName = "createCharacter",
			FunctionParameter = new
			{
				name = request.Name,
				type = request.Type,
				userId = request.UserID
			},
			GeneratePlayStreamEvent = true
		}, OnRegisterChildSuccess, OnRegisterChildFail);
	}

	public RegisterChildRequest CreateRegisterChildRequest(ChildInfo childInfo, string token)
	{
		RegisterChildRequest registerChildRequest = null;
		if (childInfo != null && childInfo._Name != null && childInfo._UserProfileData != null && childInfo._UserProfileData.AvatarInfo != null && childInfo._UserProfileData.AvatarInfo.UserInfo != null)
		{
			Gender gender = (childInfo._UserProfileData.AvatarInfo.UserInfo.GenderID.HasValue ? childInfo._UserProfileData.AvatarInfo.UserInfo.GenderID.Value : Gender.Unknown);
			registerChildRequest = new RegisterChildRequest();
			registerChildRequest.Name = childInfo._Name;
			registerChildRequest.Token = token;
			registerChildRequest.UserID = childInfo._UserID;
			registerChildRequest.Type = gender.ToString();
		}
		return registerChildRequest;
	}

	public override void RegisterChild(RegisterChildRequest request)
	{
		if (ChildCharacterExist(request.UserID))
		{
			mPlayFabLogin.JSPlayer.ChildName = request.Name;
			mPlayFabLogin.JSPlayer.ChildUserID = request.UserID;
			mPlayFabLogin.JSPlayer.ChildToken = request.Token;
			mPlayFabLogin.JSPlayer.ChildType = request.Type;
			mCurrentCharacterSelected = false;
			UpdateCurrentCharacter();
		}
		else
		{
			base.RegisterChild(request);
			mCurrentCharacterSelected = false;
		}
	}

	public void SetPlayfabTitleId()
	{
		string text = string.Empty;
		if (ProductSettings.GetEnvironment() == Environment.DEV)
		{
			text = ProductSettings.pInstance.GetCustomKeyValue("PlayFabDev");
		}
		else if (ProductSettings.GetEnvironment() == Environment.QA)
		{
			text = ProductSettings.pInstance.GetCustomKeyValue("PlayFabQA");
		}
		else if (ProductSettings.GetEnvironment() == Environment.STAGING)
		{
			text = ProductSettings.pInstance.GetCustomKeyValue("PlayFabStaging");
		}
		else if (ProductSettings.GetEnvironment() == Environment.LIVE)
		{
			text = ProductSettings.pInstance.GetCustomKeyValue("PlayFabLive");
		}
		if (!string.IsNullOrEmpty(text))
		{
			PlayFabSettings.TitleId = text;
		}
		else
		{
			PlayfabManager<PlayFabManagerDO>.EnablePlayfab = false;
		}
	}

	public string GetPlayfabTitleId()
	{
		if (ProductSettings.GetEnvironment() == Environment.DEV)
		{
			return ProductSettings.pInstance.GetCustomKeyValue("PlayFabDev");
		}
		if (ProductSettings.GetEnvironment() == Environment.QA)
		{
			return ProductSettings.pInstance.GetCustomKeyValue("PlayFabQA");
		}
		if (ProductSettings.GetEnvironment() == Environment.STAGING)
		{
			return ProductSettings.pInstance.GetCustomKeyValue("PlayFabStaging");
		}
		if (ProductSettings.GetEnvironment() == Environment.LIVE)
		{
			return ProductSettings.pInstance.GetCustomKeyValue("PlayFabLive");
		}
		return "NA";
	}

	private void OnRegisterChildSuccess(ExecuteCloudScriptResult obj)
	{
		if (obj != null)
		{
			if (obj.Error != null)
			{
				UtDebug.Log("PlayFab OnRegisterChild " + obj.Error.Error + " :: " + obj.Error.Message);
			}
			else
			{
				UtDebug.Log("PlayFab OnRegisterChild Success");
				GetCharacters();
			}
		}
		Debug.LogError("PlayFab OnRegisterChild Fail");
	}

	private void OnRegisterChildFail(PlayFabError obj)
	{
		if (obj != null)
		{
			Debug.LogError("PlayFab " + obj.GenerateErrorReport());
		}
	}
}
