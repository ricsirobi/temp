using UnityEngine;

public class UiDragonsEndDB : KAUI
{
	public EndGameData[] _GameData;

	public string _UpsellGameDataFileName = "RS_DATA/UpsellGameDataDO.xml";

	public AdEventType _AdEventType;

	public LocaleString _AdRewardFailedText = new LocaleString("[Review]Ad Rewards Failed.");

	private GameObject mMessageObject;

	private EndGameData mCurrentGameData;

	protected UiGameResults mResultUI;

	private UiHighScoresBox mHighScoreParentUI;

	protected UiUpsellDropDown mUpsellDropDownUI;

	public GameObject pMessageObject => mMessageObject;

	public EndGameData pCurrentGameData => mCurrentGameData;

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	public void Initialize()
	{
		if (mResultUI == null)
		{
			mResultUI = GetComponentInChildren<UiGameResults>();
		}
		if (mHighScoreParentUI == null)
		{
			mHighScoreParentUI = GetComponentInChildren<UiHighScoresBox>();
		}
		if (mUpsellDropDownUI == null)
		{
			mUpsellDropDownUI = GetComponentInChildren<UiUpsellDropDown>();
		}
	}

	public EndGameData GetDataFromType(string inType, bool isAssign = false)
	{
		if (string.IsNullOrEmpty(inType))
		{
			return null;
		}
		EndGameData[] gameData = _GameData;
		foreach (EndGameData endGameData in gameData)
		{
			if (endGameData._GameType == inType)
			{
				if (isAssign)
				{
					mCurrentGameData = endGameData;
				}
				return endGameData;
			}
		}
		return null;
	}

	public void SetGameSettings(string inType, GameObject inObj, string inResult, bool inShowHighScore = true)
	{
		mMessageObject = inObj;
		GetDataFromType(inType, isAssign: true);
		if (!string.IsNullOrEmpty(_UpsellGameDataFileName))
		{
			UpsellGameData.Init(_UpsellGameDataFileName);
		}
		if (mResultUI != null)
		{
			mResultUI.Init(this);
		}
		else
		{
			UtDebug.LogError("NO RESULT UI FOUND!!!");
		}
		if (mUpsellDropDownUI != null)
		{
			mUpsellDropDownUI.Init(this, inType, inResult);
		}
		if (mHighScoreParentUI != null)
		{
			mHighScoreParentUI.gameObject.SetActive(inShowHighScore);
			if (inShowHighScore)
			{
				mHighScoreParentUI.Init(this);
			}
		}
		else
		{
			UtDebug.LogError("NO HIGHSCORE UI FOUND!!!");
		}
	}

	public void SetRewardDisplay(AchievementReward[] inRewards)
	{
		if (mResultUI != null)
		{
			mResultUI.SetRewardDisplay(inRewards);
		}
	}

	public void SetRewardMessage(string inText)
	{
		if (mResultUI != null)
		{
			mResultUI.SetRewardMessage(inText);
		}
	}

	public void SetGrade(string inGrade, string inGradeColor)
	{
		if (mResultUI != null)
		{
			mResultUI.SetGrade(inGrade, inGradeColor);
		}
	}

	public void AllowChallenge(bool allow)
	{
		if (mResultUI != null)
		{
			mResultUI.AllowChallenge(allow);
		}
	}

	private void OnChallengeDone()
	{
		mResultUI.OnChallengeDone(UiChallengeInvite.pIsChallengeSent);
		SetVisibility(Visibility: true);
	}

	public override void SetVisibility(bool Visibility)
	{
		base.SetVisibility(Visibility);
		if (Visibility && MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		if (mResultUI != null)
		{
			mResultUI.SetVisibility(Visibility);
		}
		if (mUpsellDropDownUI != null)
		{
			mUpsellDropDownUI.SetVisibility(Visibility);
		}
		if (mHighScoreParentUI != null)
		{
			mHighScoreParentUI.SetVisibility(Visibility);
			if (!Visibility)
			{
				mHighScoreParentUI.ResetPlayerData();
			}
		}
	}

	public override void SetInteractive(bool interactive)
	{
		base.SetInteractive(interactive);
		if (mResultUI != null)
		{
			mResultUI.SetInteractive(interactive);
		}
		if (mUpsellDropDownUI != null)
		{
			mUpsellDropDownUI.SetInteractive(interactive);
		}
		if (mHighScoreParentUI != null)
		{
			mHighScoreParentUI.SetInteractive(interactive);
		}
	}

	public void SetResultData(string widgetName, string txtName, string txtValue = null, KAWidget groupData = null, KAWidget inWidget = null)
	{
		if (!(mResultUI != null))
		{
			return;
		}
		KAWidget kAWidget = groupData ?? mResultUI.FindItem("GrpScoreData");
		if (kAWidget != null && !kAWidget.GetVisibility())
		{
			kAWidget.SetVisibility(inVisible: true);
		}
		kAWidget = inWidget ?? mResultUI.FindItem(widgetName);
		KAWidget kAWidget2 = null;
		if (!(kAWidget != null))
		{
			return;
		}
		kAWidget.SetVisibility(inVisible: true);
		if (txtName != null)
		{
			kAWidget2 = kAWidget.FindChildItem("TxtDisplay");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(txtName);
				kAWidget2.SetVisibility(inVisible: true);
			}
		}
		if (txtValue != null)
		{
			kAWidget2 = kAWidget.FindChildItem("TxtDisplayAmt");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(txtValue);
				kAWidget2.SetVisibility(inVisible: true);
			}
		}
	}

	public void SetTitle(string inTitle)
	{
		if (mResultUI != null)
		{
			KAWidget kAWidget = mResultUI.FindItem("Title");
			if (kAWidget != null)
			{
				kAWidget.SetText(inTitle);
			}
		}
	}

	public void SetHighScoreData(int inScore, string inKey, bool inAscendingOrder = false)
	{
		if (mHighScoreParentUI != null)
		{
			mHighScoreParentUI.SetHighScoreData(inScore, inKey, inAscendingOrder);
		}
	}

	public void SetAdRewardData(string moduleName, int levelScore)
	{
		mResultUI.SetAdRewardData(moduleName, levelScore);
	}
}
