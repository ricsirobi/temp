using System;
using UnityEngine;

public class UiFarmsInfoCard : UiCard
{
	private enum NameValidationStatus
	{
		NONE,
		VALIDATING,
		APPROVED,
		REJECTED
	}

	[Serializable]
	public class NameValidationResultInfo
	{
		public NameValidationResult _Status;

		public LocaleString _StatusText;
	}

	public int _MinNameTextLength = 3;

	public NameValidationResultInfo[] _NameValidationResultInfo;

	public LocaleString _NameValidationFailedText = new LocaleString("There was a problem with the server.  Please try again.");

	public LocaleString _NameNotLongEnoughText = new LocaleString("The Farm name must be at least 3 characters.");

	public LocaleString _NameRejectedText = new LocaleString("This name is not approved. Please use another name.");

	public Color _ApprovedTextColor = Color.green;

	public Color _RejectedTextColor = Color.red;

	public string _CreativePointsProgressFullSprite = "AniDWDragonsMeterBarCreativePointsFull";

	public string _CreativePointsProgressSprite = "AniDWDragonsMeterBarCreativePoints";

	private KAWidget mRoomName;

	private bool mRoomNameCleared;

	private KAUIGenericDB mKAUIGenericDB;

	private KAWidget mGoBtn;

	private KAWidget mTxtCreativePoints;

	private KAWidget mAniCreativeBar;

	private KAWidget mGrpCreativeBar;

	private KAWidget mRoomCardBkg;

	private UserRoom mFarmData;

	private NameValidationStatus mNameValidationStatus;

	private string mCachedRoomName;

	private Color mDefaultNameTextColor = Color.black;

	private UiFarms mUiFarms;

	public UserRoom pFarmData
	{
		get
		{
			return mFarmData;
		}
		set
		{
			mFarmData = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mRoomName = FindItem("TxtFarmName");
		mGoBtn = FindItem("BtnGo");
		mRoomCardBkg = FindItem("RoomCardBkg");
		mTxtCreativePoints = FindItem("TxtCreativePoints");
		mAniCreativeBar = FindItem("AniCreativeBar");
		mGrpCreativeBar = FindItem("GrpCreativeBar");
		mUiFarms = (UiFarms)_UiCardParent;
		mGrpCreativeBar.SetVisibility(mUiFarms.pIsUserFarm);
		if (!mUiFarms.pIsUserFarm && mRoomName != null)
		{
			mRoomName.SetInteractive(isInteractive: false);
		}
	}

	public void RefreshUI()
	{
		UserRoom userRoom = mFarmData;
		if (userRoom != null)
		{
			if (mRoomName != null)
			{
				mRoomName.SetText(userRoom.pLocaleName);
				mRoomNameCleared = false;
			}
			if (mRoomCardBkg != null)
			{
				string farmBanner = mUiFarms.pUiFarmsListCard.GetFarmBanner(mFarmData.pItemID);
				if (!string.IsNullOrEmpty(farmBanner))
				{
					UISlicedSprite componentInChildren = mRoomCardBkg.GetComponentInChildren<UISlicedSprite>();
					if (componentInChildren != null)
					{
						componentInChildren.UpdateSprite(farmBanner);
					}
				}
			}
			if (mUiFarms.pIsUserFarm)
			{
				float num = (float)userRoom.CreativePoints;
				int num2 = userRoom.MaxCreativePointsLimit();
				mTxtCreativePoints.SetText(num + "/" + num2);
				if (num >= (float)num2)
				{
					mAniCreativeBar.GetProgressBar().UpdateSprite(_CreativePointsProgressFullSprite);
				}
				else
				{
					mAniCreativeBar.GetProgressBar().UpdateSprite(_CreativePointsProgressSprite);
				}
				mAniCreativeBar.SetProgressLevel(num / (float)num2);
			}
		}
		bool flag = !mUiFarms.pIsUserFarm || mFarmData.RoomID != FarmManager.pCurrentFarmID;
		if (MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt() && UiFarms.pUserID != null)
		{
			flag = ((MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID().Equals(UiFarms.pUserID) && FarmManager.pCurrentFarmData != null) ? (mFarmData.RoomID != FarmManager.pCurrentFarmID) : flag);
		}
		mGoBtn.SetVisibility(flag);
	}

	private void ShowGenericDB(LocaleString inString, string inOkMessageFunction)
	{
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetText(inString.GetLocalizedString(), interactive: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = inOkMessageFunction;
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	public override void OnInput(KAWidget inWidget, string inText)
	{
		base.OnInput(inWidget, inText);
		if (inWidget == mRoomName && mNameValidationStatus != 0)
		{
			mRoomName.GetLabel().color = mDefaultNameTextColor;
			mNameValidationStatus = NameValidationStatus.NONE;
		}
	}

	public override void OnSelect(KAWidget inWidget, bool inSelected)
	{
		base.OnSelect(inWidget, inSelected);
		if (!inSelected && inWidget == mRoomName && !string.Equals(inWidget.GetText(), mFarmData.pLocaleName) && mNameValidationStatus == NameValidationStatus.NONE)
		{
			if (string.IsNullOrEmpty(inWidget.GetText()))
			{
				mRoomName.SetText(mFarmData.pLocaleName);
				mRoomNameCleared = false;
				return;
			}
			if (inWidget.GetText().Length < _MinNameTextLength)
			{
				ShowGenericDB(_NameNotLongEnoughText, "KillGenericDB");
				return;
			}
			mNameValidationStatus = NameValidationStatus.VALIDATING;
			mGoBtn.SetInteractive(isInteractive: false);
			mCachedRoomName = mFarmData.Name;
			mFarmData.SetRoomName(mRoomName.GetText());
			mFarmData.Save(NameValidationEventHandler);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnGo" && mNameValidationStatus != NameValidationStatus.VALIDATING)
		{
			LoadFarm(mFarmData);
		}
		else if (inWidget == mRoomName && !mRoomNameCleared)
		{
			mRoomName.SetText("");
		}
	}

	public void LoadFarm(UserRoom farmData)
	{
		if (farmData != null)
		{
			int num = farmData.pItemID;
			if (num <= 0)
			{
				num = mUiFarms._FarmExpansionDefaultItemID;
			}
			ItemData.Load(num, OnLoadItemDataReady, farmData);
		}
	}

	public void OnLoadItemDataReady(int itemID, ItemData itemData, object inUserData)
	{
		if (itemData != null)
		{
			string text = itemData.AssetName;
			if (string.IsNullOrEmpty(text))
			{
				text = mUiFarms._FarmDefaultScene;
			}
			FarmManager.pCurrentFarmData = (UserRoom)inUserData;
			MainStreetMMOClient.UserRoomID = FarmManager.pCurrentFarmData.RoomID;
			if (MainStreetMMOClient.pIsMMOEnabled && MainStreetMMOClient.pInstance.JoinOwnerSpace(text, mUiFarms.pIsUserFarm ? UserInfo.pInstance.UserID : UiFarms.pUserID, force: true))
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
				AvAvatar.SetActive(inActive: false);
				Input.ResetInputAxes();
				mUiFarms.Exit();
			}
			else
			{
				RsResourceManager.LoadLevel(text);
			}
		}
	}

	public override void OnExitClicked()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnCardExit", SendMessageOptions.DontRequireReceiver);
		}
		mUiFarms.pUiFarmsListCard.PopOutCard();
	}

	private void NameValidationEventHandler(bool success, UserRoomSetResponse response)
	{
		if (!success && (response == null || response.StatusCode == UserRoomValidationResult.RMFValidationFailed))
		{
			mFarmData.SetRoomName(mCachedRoomName);
			mNameValidationStatus = NameValidationStatus.REJECTED;
			if (mRoomName != null)
			{
				mRoomName.GetLabel().color = _RejectedTextColor;
			}
			if (response == null)
			{
				ShowGenericDB(_NameValidationFailedText, "KillGenericDB");
			}
			else
			{
				ShowGenericDB(_NameRejectedText, "KillGenericDB");
			}
		}
		else
		{
			mGoBtn.SetInteractive(isInteractive: true);
			mNameValidationStatus = NameValidationStatus.APPROVED;
			if (mRoomName != null)
			{
				mRoomName.GetLabel().color = _ApprovedTextColor;
			}
			if (mFarmData != null)
			{
				mUiFarms.pUiFarmsListCard.RefreshUI();
			}
		}
	}
}
