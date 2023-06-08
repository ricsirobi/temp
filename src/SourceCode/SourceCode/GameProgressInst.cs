using System;
using System.Collections.Generic;

public class GameProgressInst
{
	private string mName;

	private string mVersion;

	private int mGameId;

	private int mMaxChapter;

	private int mMaxDifficuty;

	private int mMaxLevel;

	private GameProgress mInstance;

	public bool pIsReady => mInstance != null;

	public void Init(int gameId, string gname, string version, int maxChapter, int maxDifficuty, int maxLevel)
	{
		mName = gname;
		mVersion = version;
		mGameId = gameId;
		mMaxChapter = maxChapter;
		mMaxDifficuty = maxDifficuty;
		mMaxLevel = maxLevel;
		WsWebService.GetGameProgress(mGameId, ServiceEventHandler, null);
	}

	private void InitDefault()
	{
		mInstance = new GameProgress();
		mInstance.Name = mName;
		mInstance.Version = mVersion;
		if (mMaxChapter > 0)
		{
			mInstance.Chapter = new GameChapter[mMaxChapter];
			for (int i = 0; i < mMaxChapter; i++)
			{
				mInstance.Chapter[i] = new GameChapter();
				mInstance.Chapter[i].Name = "";
				if (mMaxDifficuty > 0)
				{
					mInstance.Chapter[i].Difficulty = new GameDifficulty[mMaxDifficuty];
					for (int j = 0; j < mMaxDifficuty; j++)
					{
						mInstance.Chapter[i].Difficulty[j] = new GameDifficulty();
						mInstance.Chapter[i].Difficulty[j].Name = "";
						if (mMaxLevel > 0)
						{
							mInstance.Chapter[i].Difficulty[j].Level = new GameLevel[mMaxLevel];
							for (int k = 0; k < mMaxLevel; k++)
							{
								mInstance.Chapter[i].Difficulty[j].Level[k] = new GameLevel();
								mInstance.Chapter[i].Difficulty[j].Level[k].Name = "";
								mInstance.Chapter[i].Difficulty[j].Level[k].Score = 0;
								mInstance.Chapter[i].Difficulty[j].Level[k].Star = 0;
								mInstance.Chapter[i].Difficulty[j].Level[k].Custom = "";
							}
						}
						mInstance.Chapter[i].Difficulty[j].Custom = "";
					}
				}
				mInstance.Chapter[i].Custom = "";
			}
		}
		mInstance.Custom = "";
		Save();
	}

	private void ProcessWriteCustomString()
	{
		if (!pIsReady)
		{
			return;
		}
		if (mMaxChapter > 0)
		{
			for (int i = 0; i < mMaxChapter; i++)
			{
				if (mMaxDifficuty > 0)
				{
					for (int j = 0; j < mMaxDifficuty; j++)
					{
						if (mMaxLevel > 0)
						{
							for (int k = 0; k < mMaxLevel; k++)
							{
								if (mInstance.Chapter[i].Difficulty[j].Level[k].CustomDict.Count <= 0)
								{
									continue;
								}
								mInstance.Chapter[i].Difficulty[j].Level[k].Custom = "";
								foreach (KeyValuePair<string, string> item in mInstance.Chapter[i].Difficulty[j].Level[k].CustomDict)
								{
									if (mInstance.Chapter[i].Difficulty[j].Level[k].Custom == "")
									{
										GameLevel obj = mInstance.Chapter[i].Difficulty[j].Level[k];
										obj.Custom = obj.Custom + item.Key + "~" + item.Value;
										continue;
									}
									GameLevel gameLevel = mInstance.Chapter[i].Difficulty[j].Level[k];
									gameLevel.Custom = gameLevel.Custom + "~" + item.Key + "~" + item.Value;
								}
							}
						}
						if (mInstance.Chapter[i].Difficulty[j].CustomDict.Count <= 0)
						{
							continue;
						}
						mInstance.Chapter[i].Difficulty[j].Custom = "";
						foreach (KeyValuePair<string, string> item2 in mInstance.Chapter[i].Difficulty[j].CustomDict)
						{
							if (mInstance.Chapter[i].Difficulty[j].Custom == "")
							{
								GameDifficulty obj2 = mInstance.Chapter[i].Difficulty[j];
								obj2.Custom = obj2.Custom + item2.Key + "~" + item2.Value;
								continue;
							}
							GameDifficulty gameDifficulty = mInstance.Chapter[i].Difficulty[j];
							gameDifficulty.Custom = gameDifficulty.Custom + "~" + item2.Key + "~" + item2.Value;
						}
					}
				}
				if (mInstance.Chapter[i].CustomDict.Count <= 0)
				{
					continue;
				}
				mInstance.Chapter[i].Custom = "";
				foreach (KeyValuePair<string, string> item3 in mInstance.Chapter[i].CustomDict)
				{
					if (mInstance.Chapter[i].Custom == "")
					{
						GameChapter obj3 = mInstance.Chapter[i];
						obj3.Custom = obj3.Custom + item3.Key + "~" + item3.Value;
						continue;
					}
					GameChapter gameChapter = mInstance.Chapter[i];
					gameChapter.Custom = gameChapter.Custom + "~" + item3.Key + "~" + item3.Value;
				}
			}
		}
		if (mInstance.CustomDict.Count <= 0)
		{
			return;
		}
		mInstance.Custom = "";
		foreach (KeyValuePair<string, string> item4 in mInstance.CustomDict)
		{
			if (mInstance.Custom == "")
			{
				GameProgress gameProgress = mInstance;
				gameProgress.Custom = gameProgress.Custom + item4.Key + "~" + item4.Value;
				continue;
			}
			GameProgress gameProgress2 = mInstance;
			gameProgress2.Custom = gameProgress2.Custom + "~" + item4.Key + "~" + item4.Value;
		}
	}

	private void ProcessReadCustomString()
	{
		if (mInstance.Chapter.Length != 0)
		{
			mMaxChapter = mInstance.Chapter.Length;
			for (int i = 0; i < mMaxChapter; i++)
			{
				if (mInstance.Chapter[i].Difficulty.Length != 0)
				{
					mMaxDifficuty = mInstance.Chapter[i].Difficulty.Length;
					for (int j = 0; j < mMaxDifficuty; j++)
					{
						if (mInstance.Chapter[i].Difficulty[j].Level.Length != 0)
						{
							mMaxLevel = mInstance.Chapter[i].Difficulty[j].Level.Length;
							for (int k = 0; k < mMaxLevel; k++)
							{
								if (mInstance.Chapter[i].Difficulty[j].Level[k].Custom != "")
								{
									string[] array = mInstance.Chapter[i].Difficulty[j].Level[k].Custom.Split('~');
									for (int l = 0; l < array.Length; l += 2)
									{
										mInstance.Chapter[i].Difficulty[j].Level[k].CustomDict[array[l]] = array[l + 1];
									}
								}
							}
						}
						if (mInstance.Chapter[i].Difficulty[j].Custom != "")
						{
							string[] array2 = mInstance.Chapter[i].Difficulty[j].Custom.Split('~');
							for (int m = 0; m < array2.Length; m += 2)
							{
								mInstance.Chapter[i].Difficulty[j].CustomDict[array2[m]] = array2[m + 1];
							}
						}
					}
				}
				if (mInstance.Chapter[i].Custom != "")
				{
					string[] array3 = mInstance.Chapter[i].Custom.Split('~');
					for (int n = 0; n < array3.Length; n += 2)
					{
						mInstance.Chapter[i].CustomDict[array3[n]] = array3[n + 1];
					}
				}
			}
		}
		if (mInstance.Custom != "")
		{
			string[] array4 = mInstance.Custom.Split('~');
			for (int num = 0; num < array4.Length; num += 2)
			{
				mInstance.CustomDict[array4[num]] = array4[num + 1];
			}
		}
	}

	public void Save()
	{
		if (pIsReady)
		{
			ProcessWriteCustomString();
			WsWebService.SetGameProgress(mGameId, mInstance, ServiceEventHandler, null);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_GAMEPROGRESS:
		case WsServiceType.GET_GAMEPROGRESS_BY_USERID:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				GameProgress gameProgress = (GameProgress)inObject;
				if (gameProgress != null)
				{
					mInstance = gameProgress;
					ProcessReadCustomString();
				}
				else
				{
					UtDebug.Log("WEB SERVICE CALL GetGameProgress RETURNED NO DATA!!!");
					InitDefault();
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetGameProgress FAILED!!!");
				InitDefault();
				break;
			}
			break;
		case WsServiceType.SET_GAMEPROGRESS:
			if (inEvent == WsServiceEvent.ERROR)
			{
				UtDebug.LogError("WEB SERVICE CALL SetGameProgress FAILED!!!");
			}
			break;
		}
	}

	public int GetInt(string ikey, int defaultValue, int ichapter, int idifficulty, int ilevel)
	{
		if (pIsReady)
		{
			if (ichapter == -1)
			{
				if (mInstance.CustomDict.ContainsKey(ikey))
				{
					return Convert.ToInt32(mInstance.CustomDict[ikey]);
				}
				return defaultValue;
			}
			if (idifficulty == -1)
			{
				if (ichapter < mMaxChapter)
				{
					if (mInstance.Chapter[ichapter].CustomDict.ContainsKey(ikey))
					{
						return Convert.ToInt32(mInstance.Chapter[ichapter].CustomDict[ikey]);
					}
					return defaultValue;
				}
			}
			else if (ilevel == -1)
			{
				if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty)
				{
					if (mInstance.Chapter[ichapter].Difficulty[idifficulty].CustomDict.ContainsKey(ikey))
					{
						return Convert.ToInt32(mInstance.Chapter[ichapter].Difficulty[idifficulty].CustomDict[ikey]);
					}
					return defaultValue;
				}
			}
			else if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
			{
				if (mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].CustomDict.ContainsKey(ikey))
				{
					return Convert.ToInt32(mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].CustomDict[ikey]);
				}
				return defaultValue;
			}
		}
		UtDebug.LogError("Index of GameProgress out of range on GetInt() " + ichapter + ", " + idifficulty + ", " + ilevel);
		return defaultValue;
	}

	public int GetInt(string ikey, int defaultValue, int ichapter)
	{
		return GetInt(ikey, defaultValue, ichapter, -1, -1);
	}

	public int GetInt(string ikey, int defaultValue)
	{
		return GetInt(ikey, defaultValue, -1, -1, -1);
	}

	public int GetInt(string ikey)
	{
		return GetInt(ikey, 0, -1, -1, -1);
	}

	public string GetString(string ikey, string defaultValue, int ichapter, int idifficulty, int ilevel)
	{
		if (pIsReady)
		{
			if (ichapter == -1)
			{
				if (mInstance.CustomDict.ContainsKey(ikey))
				{
					return mInstance.CustomDict[ikey];
				}
				return defaultValue;
			}
			if (idifficulty == -1)
			{
				if (ichapter < mMaxChapter)
				{
					if (mInstance.Chapter[ichapter].CustomDict.ContainsKey(ikey))
					{
						return mInstance.Chapter[ichapter].CustomDict[ikey];
					}
					return defaultValue;
				}
			}
			else if (ilevel == -1)
			{
				if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty)
				{
					if (mInstance.Chapter[ichapter].Difficulty[idifficulty].CustomDict.ContainsKey(ikey))
					{
						return mInstance.Chapter[ichapter].Difficulty[idifficulty].CustomDict[ikey];
					}
					return defaultValue;
				}
			}
			else if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
			{
				if (mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].CustomDict.ContainsKey(ikey))
				{
					return mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].CustomDict[ikey];
				}
				return defaultValue;
			}
		}
		UtDebug.LogError("Index of GameProgress out of range on GetString() " + ichapter + ", " + idifficulty + ", " + ilevel);
		return defaultValue;
	}

	public string GetString(string ikey, string defaultValue, int ichapter)
	{
		return GetString(ikey, defaultValue, ichapter, -1, -1);
	}

	public string GetString(string ikey, string defaultValue)
	{
		return GetString(ikey, defaultValue, -1, -1, -1);
	}

	public string GetString(string ikey)
	{
		return GetString(ikey, "", -1, -1, -1);
	}

	public void SetInt(string ikey, int iValue, int ichapter, int idifficulty, int ilevel)
	{
		if (!pIsReady)
		{
			return;
		}
		if (ichapter == -1)
		{
			mInstance.CustomDict[ikey] = iValue.ToString();
		}
		else if (idifficulty == -1)
		{
			if (ichapter < mMaxChapter)
			{
				mInstance.Chapter[ichapter].CustomDict[ikey] = iValue.ToString();
				return;
			}
			UtDebug.LogError("Index of GameProgress out of range on SetInt() " + ichapter + ", " + idifficulty + ", " + ilevel);
		}
		else if (ilevel == -1)
		{
			if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty)
			{
				mInstance.Chapter[ichapter].Difficulty[idifficulty].CustomDict[ikey] = iValue.ToString();
				return;
			}
			UtDebug.LogError("Index of GameProgress out of range on SetInt() " + ichapter + ", " + idifficulty + ", " + ilevel);
		}
		else if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
		{
			mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].CustomDict[ikey] = iValue.ToString();
		}
		else
		{
			UtDebug.LogError("Index of GameProgress out of range on SetInt() " + ichapter + ", " + idifficulty + ", " + ilevel);
		}
	}

	public void SetInt(string ikey, int iValue, int ichapter)
	{
		SetInt(ikey, iValue, ichapter, -1, -1);
	}

	public void SetInt(string ikey, int iValue)
	{
		SetInt(ikey, iValue, -1, -1, -1);
	}

	public void SetString(string ikey, string iValue, int ichapter, int idifficulty, int ilevel)
	{
		if (!pIsReady)
		{
			return;
		}
		if (ichapter == -1)
		{
			mInstance.CustomDict[ikey] = iValue;
		}
		else if (idifficulty == -1)
		{
			if (ichapter < mMaxChapter)
			{
				mInstance.Chapter[ichapter].CustomDict[ikey] = iValue;
				return;
			}
			UtDebug.LogError("Index of GameProgress out of range on SetString() " + ichapter + ", " + idifficulty + ", " + ilevel);
		}
		else if (ilevel == -1)
		{
			if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty)
			{
				mInstance.Chapter[ichapter].Difficulty[idifficulty].CustomDict[ikey] = iValue;
				return;
			}
			UtDebug.LogError("Index of GameProgress out of range on SetString() " + ichapter + ", " + idifficulty + ", " + ilevel);
		}
		else if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
		{
			mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].CustomDict[ikey] = iValue;
		}
		else
		{
			UtDebug.LogError("Index of GameProgress out of range on SetString() " + ichapter + ", " + idifficulty + ", " + ilevel);
		}
	}

	public void SetString(string ikey, string iValue, int ichapter)
	{
		SetString(ikey, iValue, ichapter, -1, -1);
	}

	public void SetString(string ikey, string iValue)
	{
		SetString(ikey, iValue, -1, -1, -1);
	}

	public bool HasKey(string ikey, int ichapter, int idifficulty, int ilevel)
	{
		if (pIsReady)
		{
			if (ichapter == -1)
			{
				if (mInstance.CustomDict.ContainsKey(ikey))
				{
					return true;
				}
				return false;
			}
			if (idifficulty == -1)
			{
				if (ichapter < mMaxChapter)
				{
					if (mInstance.Chapter[ichapter].CustomDict.ContainsKey(ikey))
					{
						return true;
					}
					return false;
				}
			}
			else if (ilevel == -1)
			{
				if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty)
				{
					if (mInstance.Chapter[ichapter].Difficulty[idifficulty].CustomDict.ContainsKey(ikey))
					{
						return true;
					}
					return false;
				}
			}
			else if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
			{
				if (mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].CustomDict.ContainsKey(ikey))
				{
					return true;
				}
				return false;
			}
		}
		UtDebug.LogError("Index of GameProgress out of range on HasKey() " + ichapter + ", " + idifficulty + ", " + ilevel);
		return false;
	}

	public bool HasKey(string ikey, int ichapter)
	{
		return HasKey(ikey, ichapter, -1, -1);
	}

	public bool HasKey(string ikey)
	{
		return HasKey(ikey, -1, -1, -1);
	}

	public int GetScore(int ichapter, int idifficulty, int ilevel)
	{
		if (pIsReady && ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
		{
			return mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].Score;
		}
		UtDebug.LogError("Index of GameProgress out of range on GetScore() " + ichapter + ", " + idifficulty + ", " + ilevel);
		return 0;
	}

	public void SetScore(int iscore, int ichapter, int idifficulty, int ilevel)
	{
		if (pIsReady)
		{
			if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
			{
				mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].Score = iscore;
				return;
			}
			UtDebug.LogError("Index of GameProgress out of range on SetScore() " + ichapter + ", " + idifficulty + ", " + ilevel);
		}
	}

	public int GetStar(int ichapter, int idifficulty, int ilevel)
	{
		if (pIsReady && ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
		{
			return mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].Star;
		}
		UtDebug.LogError("Index of GameProgress out of range on GetStar() " + ichapter + ", " + idifficulty + ", " + ilevel);
		return 0;
	}

	public void SetStar(int istar, int ichapter, int idifficulty, int ilevel)
	{
		if (pIsReady)
		{
			if (ichapter < mMaxChapter && idifficulty < mMaxDifficuty && ilevel < mMaxLevel)
			{
				mInstance.Chapter[ichapter].Difficulty[idifficulty].Level[ilevel].Star = istar;
				return;
			}
			UtDebug.LogError("Index of GameProgress out of range on SetStar() " + ichapter + ", " + idifficulty + ", " + ilevel);
		}
	}
}
