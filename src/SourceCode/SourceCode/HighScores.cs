using System.Collections.Generic;
using UnityEngine;

public class HighScores
{
	public string ModuleName;

	public int GameID;

	public bool IsMultiPlayer;

	public int Difficulty;

	public int Level;

	public bool Win;

	public bool Loss;

	public GameObject mMessageObject;

	private static HighScores mInstance = new HighScores();

	private List<GameDataUserData> UserDataList;

	public eLaunchPage mDisplayType;

	public WsServiceEventHandler mCallback;

	private static GameObject mHighScoresInterface;

	public static HighScores pInstance => mInstance;

	public GameObject pHighScoresInterface => mHighScoresInterface;

	public HighScores()
	{
		UserDataList = new List<GameDataUserData>();
		Win = false;
		Loss = false;
		mDisplayType = eLaunchPage.BUDDYSCORE;
		mCallback = ServiceEventHandler;
	}

	public static eLaunchPage GetDisplayLaunchPage()
	{
		return mInstance.mDisplayType;
	}

	public static GameDataUserData GetGameData(int index)
	{
		if (index < 0)
		{
			Debug.LogError("Array Out of Range");
			return null;
		}
		if (mInstance.UserDataList.Count == 0)
		{
			return null;
		}
		return mInstance.UserDataList[index];
	}

	public static GameDataUserData GetGameData(string name)
	{
		for (int i = 0; i < mInstance.UserDataList.Count; i++)
		{
			if (mInstance.UserDataList[i].Name == name)
			{
				return mInstance.UserDataList[i];
			}
		}
		return null;
	}

	public static void SetCurrentGameSettings(string moduleName, int gameID, bool isMultiPlayer, int difficulty, int level)
	{
		UtDebug.Assert(level > 0);
		ClearGameData();
		mInstance.ModuleName = moduleName;
		mInstance.GameID = gameID;
		mInstance.IsMultiPlayer = isMultiPlayer;
		mInstance.Difficulty = difficulty;
		mInstance.Level = level;
		mInstance.Win = false;
		mInstance.Loss = false;
	}

	public static void AddGameData(string inDataName, string inDataValue)
	{
		if (mInstance == null)
		{
			return;
		}
		if (mInstance.UserDataList != null)
		{
			for (int i = 0; i < mInstance.UserDataList.Count; i++)
			{
				if (mInstance.UserDataList[i].Name == inDataName)
				{
					mInstance.UserDataList[i].Item = inDataValue;
					return;
				}
			}
		}
		GameDataUserData gameDataUserData = new GameDataUserData();
		gameDataUserData.Name = inDataName;
		gameDataUserData.Item = inDataValue;
		mInstance.UserDataList.Add(gameDataUserData);
	}

	public static void SendGameData()
	{
		if (mInstance == null)
		{
			return;
		}
		string text = null;
		if (UserInfo.pInstance != null)
		{
			text = UserInfo.pInstance.UserID;
		}
		if (text == null)
		{
			Debug.LogError("UserID is null");
			return;
		}
		if (GetGameData(0) != null)
		{
			string xMLString = GetXMLString();
			WsWebService.SetGameData(text, mInstance.GameID, mInstance.IsMultiPlayer, mInstance.Difficulty, mInstance.Level, xMLString, mInstance.Win, mInstance.Loss, null, null);
		}
		ClearGameData();
	}

	public static void ClearGameData()
	{
		if (mInstance != null && mInstance.UserDataList != null)
		{
			mInstance.UserDataList.Clear();
		}
	}

	public static string GetXMLString()
	{
		string text = "";
		text += "<data>\n";
		for (int i = 0; i < mInstance.UserDataList.Count; i++)
		{
			GameDataUserData gameDataUserData = mInstance.UserDataList[i];
			text = text + "<" + gameDataUserData.Name + ">" + gameDataUserData.Item + "</" + gameDataUserData.Name + ">";
		}
		return text + "\n</data>";
	}

	public static void SetGameState(eGameState state)
	{
		if (state == eGameState.EWIN)
		{
			mInstance.Win = true;
		}
		if (state == eGameState.ELOSS)
		{
			mInstance.Loss = true;
		}
		if (state == eGameState.ETIE)
		{
			mInstance.Win = true;
			mInstance.Loss = true;
		}
	}

	private static void LaunchInterface()
	{
		mHighScoresInterface = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiHighScores"));
		mHighScoresInterface.gameObject.BroadcastMessage("LaunchInterface", mInstance.mDisplayType, SendMessageOptions.RequireReceiver);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			_ = 127;
		}
	}

	public static void LaunchHighScoresMenu(eLaunchPage dispType, GameObject messageObject)
	{
		if (mInstance == null)
		{
			return;
		}
		string text = null;
		if (UserInfo.pInstance != null)
		{
			text = UserInfo.pInstance.UserID;
		}
		if (text == null)
		{
			Debug.LogError("UserID is null");
			return;
		}
		if (GetGameData(0) != null)
		{
			string xMLString = GetXMLString();
			WsWebService.SetGameData(text, mInstance.GameID, mInstance.IsMultiPlayer, mInstance.Difficulty, mInstance.Level, xMLString, mInstance.Win, mInstance.Loss, mInstance.mCallback, null);
		}
		mInstance.mMessageObject = messageObject;
		mInstance.mDisplayType = dispType;
		LaunchInterface();
	}

	public static void SetUIDimension(int Width, int Height)
	{
		if (mInstance != null)
		{
			_ = mHighScoresInterface == null;
		}
	}

	public static void SetUIPosition(int X, int Y)
	{
		if (mInstance != null)
		{
			_ = mHighScoresInterface == null;
		}
	}
}
