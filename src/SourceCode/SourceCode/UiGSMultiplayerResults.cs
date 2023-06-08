using UnityEngine;

public class UiGSMultiplayerResults : KAUI
{
	public GameObject _MessageObject;

	public LocaleString _AddBuddyLaterText = new LocaleString("You cannot do that at this time.  Please try again later.");

	public LocaleString _BuddyListFullText = new LocaleString("Your Friends list is full. You cannot add a new Friend.");

	public LocaleString _FriendBuddyListFullText = new LocaleString("This person's Friends list is full.");

	public LocaleString _AddBuddyWaitText = new LocaleString("You have a pending request from this Blaster.");

	public LocaleString _PayoutFailText = new LocaleString("Failed to get rewards. Please try again later.");

	public float _PayoutCallWaitTime = 60f;

	private KAWidget mBtnOK;

	private KAWidget[] mScore;

	private KAWidget[] mName;

	private KAWidget[] mPicture;

	private KAWidget[] mTxtWinner;

	private KAWidget mTxtTie;

	private KAWidget mBtnPlayAgain;

	private GauntletMMOPlayer[] mPlayers;

	private KAWidget[] mBtnsBFF = new KAWidget[2];

	private KAWidget[] mTrophies = new KAWidget[2];

	private GameObject mUiGenericDB;

	private float mPayoutCallTimer;

	private bool mIsPayoutStuck;

	private bool mPayoutDone;

	private KAUIGenericDB mKAUIGenericDB;

	private int mPlayerIdx = -1;

	private int mPlayerPos;

	public int pPlayerPos => mPlayerPos;

	protected override void Start()
	{
		base.Start();
		mBtnOK = FindItem("btnOK");
		mScore = new KAWidget[2];
		mScore[0] = FindItem("txtScorePlayer01");
		mScore[1] = FindItem("txtScorePlayer02");
		mName = new KAWidget[2];
		mName[0] = FindItem("txtNamePlayer01");
		mName[1] = FindItem("txtNamePlayer02");
		mPicture = new KAWidget[2];
		mPicture[0] = FindItem("PicPlayer01");
		mPicture[1] = FindItem("PicPlayer02");
		mTxtWinner = new KAWidget[2];
		mTxtWinner[0] = FindItem("txtWinnerPlayer01");
		mTxtWinner[1] = FindItem("txtWinnerPlayer02");
		mTxtTie = FindItem("txtGameTie");
		mBtnPlayAgain = FindItem("btnPlayAgain");
		mBtnsBFF[0] = FindItem("btnBFFPlayer01");
		mBtnsBFF[1] = FindItem("btnBFFPlayer02");
		mTrophies[0] = FindItem("TrophyPlayer01");
		mTrophies[1] = FindItem("TrophyPlayer02");
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBtnOK)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("ShowMultiplayerMenu", null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (item == mBtnPlayAgain)
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			GauntletMMOClient.pInstance.PlayGameAgain();
		}
		else if (item == mBtnsBFF[0] || item == mBtnsBFF[1])
		{
			item.SetDisabled(isDisabled: true);
			GauntletMMOPlayer otherPlayer = GetOtherPlayer();
			if (otherPlayer != null)
			{
				BuddyList.pInstance.AddBuddy(otherPlayer._UserID, "", BuddyListEventHandler);
			}
			else
			{
				UtDebug.LogError("No MMO player");
			}
		}
	}

	private void BuddyListEventHandler(WsServiceType inType, object inResult)
	{
		if (inResult == null)
		{
			ShowDialog(_AddBuddyLaterText._ID, _AddBuddyLaterText._Text, "OnCloseDB");
			return;
		}
		switch (((BuddyActionResult)inResult).Result)
		{
		case BuddyActionResultType.Unknown:
			ShowDialog(_AddBuddyLaterText._ID, _AddBuddyLaterText._Text, "OnOkDB");
			break;
		case BuddyActionResultType.Success:
			OnOkDB();
			break;
		case BuddyActionResultType.BuddyListFull:
			ShowDialog(_BuddyListFullText._ID, _BuddyListFullText._Text, "OnOkDB");
			break;
		case BuddyActionResultType.FriendBuddyListFull:
			ShowDialog(_FriendBuddyListFullText._ID, _FriendBuddyListFullText._Text, "OnOkDB");
			break;
		case BuddyActionResultType.AlreadyInList:
			ShowDialog(_AddBuddyWaitText._ID, _AddBuddyWaitText._Text, "OnOkDB");
			break;
		}
	}

	private void ShowDialog(int id, string text, string okMessage)
	{
		mUiGenericDB = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
		component._MessageObject = base.gameObject;
		component._OKMessage = okMessage;
		component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		component.SetTextByID(id, text, interactive: false);
		DisableAllButtons(isDisable: true);
	}

	public void OnOkDB()
	{
		if (mUiGenericDB != null)
		{
			Object.Destroy(mUiGenericDB);
			mUiGenericDB = null;
		}
		DisableAllButtons(isDisable: false);
	}

	public void SetResults(GauntletMMOPlayer[] inPlayers)
	{
		mPlayers = inPlayers;
	}

	public override void SetVisibility(bool t)
	{
		base.SetVisibility(t);
		if (t && mPlayers != null)
		{
			int num = 0;
			int num2 = 0;
			bool flag = true;
			int num3 = -1;
			for (int i = 0; i < mPlayers.Length; i++)
			{
				int score = mPlayers[i]._Score;
				if (num2 < score)
				{
					num2 = score;
					num = i;
				}
				mScore[i].SetText(score.ToString());
				mPlayers[i].SetItemData(mPicture[i], mName[i], null);
				if (i > 0 && mPlayers[i]._Score != mPlayers[i - 1]._Score)
				{
					flag = false;
				}
				bool num4 = mPlayers[i]._UserID == UserInfo.pInstance.UserID;
				if (!num4 && BuddyList.pInstance != null && BuddyList.pInstance.GetBuddy(mPlayers[i]._UserID) == null)
				{
					mBtnsBFF[i].SetVisibility(inVisible: true);
				}
				else
				{
					mBtnsBFF[i].SetVisibility(inVisible: false);
				}
				if (num4)
				{
					mPlayerIdx = i;
				}
				else
				{
					num3 = i;
				}
			}
			mTxtTie.SetVisibility(flag);
			for (int j = 0; j < mTxtWinner.Length; j++)
			{
				if (flag)
				{
					mTxtWinner[j].SetVisibility(inVisible: false);
				}
				else
				{
					mTxtWinner[j].SetVisibility((j == num) ? true : false);
				}
			}
			GauntletMMOPlayer gauntletMMOPlayer = mPlayers[num3];
			if (gauntletMMOPlayer != null)
			{
				if (BuddyList.pInstance.GetBuddyStatus(gauntletMMOPlayer._UserID) == BuddyStatus.Unknown)
				{
					mBtnsBFF[num3].SetDisabled(isDisabled: false);
				}
				else
				{
					mBtnsBFF[num3].SetDisabled(isDisabled: true);
				}
			}
			else
			{
				UtDebug.LogError("No MMO player");
			}
			if (mBtnOK != null && !mPayoutDone)
			{
				mBtnOK.SetInteractive(isInteractive: false);
			}
			if (!flag && (bool)GauntletRailShootManager.pInstance)
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				mPlayerPos = ((mPlayers[num]._UserID == UserInfo.pInstance.UserID) ? (-1) : (-2));
				string text = GauntletRailShootManager.pInstance._GameModuleName + "Multiplayer";
				GauntletRailShootManager.pInstance._EndDBUI.SetAdRewardData(text, mPlayerPos);
				WsWebService.ApplyPayout(text, mPlayerPos, ServiceEventHandler, null);
			}
		}
		if (!t && mPlayers != null)
		{
			GauntletMMOPlayer[] array = mPlayers;
			for (int k = 0; k < array.Length; k++)
			{
				array[k].DestroyMe();
			}
			mPlayers = null;
		}
	}

	private void DisableAllButtons(bool isDisable)
	{
		mBtnOK.SetDisabled(isDisable);
		mBtnPlayAgain.SetDisabled(isDisable);
	}

	private GauntletMMOPlayer GetOtherPlayer()
	{
		for (int i = 0; i < mPlayers.Length; i++)
		{
			if (UserInfo.pInstance.UserID != mPlayers[i]._UserID)
			{
				return mPlayers[i];
			}
		}
		return null;
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.APPLY_PAYOUT)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			if (mIsPayoutStuck)
			{
				if (mKAUIGenericDB != null)
				{
					Object.Destroy(mKAUIGenericDB.gameObject);
					mKAUIGenericDB = null;
				}
				mIsPayoutStuck = false;
			}
			AchievementReward[] array = (AchievementReward[])inObject;
			if (array != null)
			{
				AchievementReward[] array2 = array;
				foreach (AchievementReward achievementReward in array2)
				{
					int? pointTypeID = achievementReward.PointTypeID;
					if (pointTypeID.HasValue && pointTypeID.GetValueOrDefault() == 11)
					{
						int value = achievementReward.Amount.Value;
						UserRankData.AddPoints(11, value);
						mTrophies[mPlayerIdx].SetVisibility(inVisible: true);
						mTrophies[mPlayerIdx].SetText(((value > 0) ? "+" : "") + value);
						int num = ((mPlayerIdx != 1) ? 1 : 0);
						value *= -1;
						mTrophies[num].SetVisibility(inVisible: true);
						mTrophies[num].SetText(((value > 0) ? "+" : "") + value);
					}
				}
			}
			else
			{
				UtDebug.Log("reward data is null!!!");
			}
			mBtnOK.SetInteractive(isInteractive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mPayoutDone = true;
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.Log("Error fetching reward Data!!!");
			if (!mIsPayoutStuck)
			{
				mBtnOK.SetInteractive(isInteractive: true);
				KAUICursorManager.SetDefaultCursor("Arrow");
				mPayoutDone = true;
			}
			break;
		case WsServiceEvent.PROGRESS:
			if (!mIsPayoutStuck)
			{
				if (mPayoutCallTimer >= _PayoutCallWaitTime && mKAUIGenericDB == null)
				{
					KAUICursorManager.SetDefaultCursor("Arrow");
					mPayoutCallTimer = 0f;
					mIsPayoutStuck = true;
					mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "PayoutFail");
					mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
					mKAUIGenericDB.SetTextByID(_PayoutFailText._ID, _PayoutFailText._Text, interactive: false);
					mKAUIGenericDB._OKMessage = "ClickedOk";
					mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
					KAUI.SetExclusive(mKAUIGenericDB);
				}
				else
				{
					mPayoutCallTimer += Time.deltaTime;
				}
			}
			break;
		}
	}
}
