using System.Collections.Generic;
using UnityEngine;

public class UiBuddyList : KAUI
{
	public enum BuddyListType
	{
		ONLINE,
		OFFLINE,
		IGNORED,
		REQUEST
	}

	public TextAsset _TutorialTextAsset;

	public string _Intro;

	public Vector2 _Size;

	public BuddyListStrings _Strings;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public string _HouseLevel = "";

	public string _FarmingLevel = "";

	public AudioClip _MessageSFX;

	public LocaleString _AddBestBuddyText = new LocaleString("ADD TO BEST B.F.F.");

	public LocaleString _UnBestBuddyText = new LocaleString("UN-BEST B.F.F.");

	public int _ApproveBuddyAchievementID = 60;

	public int _SocialAddBuddyAchievementID = 103;

	public int _SocialOtherPlayerAddBuddyAchievementID = 124;

	private GameObject mUiGenericDB;

	private bool mMoving;

	private Vector2 mMovingOffset;

	private AvPhotoManager mStillPhotoManager;

	private UiBuddyListMenu mBuddyListMenu;

	private KAWidget mFriendReqBkg;

	private KAWidget mIcoPlayerPic;

	private KAWidget mTxtPlayerCode;

	private KAWidget mCodeEnterBtn;

	private KAWidget mTxtBuddyCodeEdit;

	private KAWidget mTabTitle;

	private KAToggleButton mOnlineBtn;

	private KAToggleButton mOfflineBtn;

	private KAToggleButton mIgnoredBtn;

	private KAToggleButton mRequestsBtn;

	private UiBuddyActionsGroup mUiBuddyActionGroup;

	private BuddyListType mSelectedBuddyListType;

	private int mCurrentMessageIndex = -1;

	private bool mFriendReqTabSelected;

	private static bool mIsLoadingAsset;

	private static UiBuddyList mInstance;

	public static UiBuddyList pInstance => mInstance;

	public static void ShowBuddyList(string inBundleURL)
	{
		if (!(pInstance != null) && !mIsLoadingAsset)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			LoadBuddyListBundle(inBundleURL);
		}
	}

	public static void LoadBuddyListBundle(string bResName)
	{
		mIsLoadingAsset = true;
		string[] array = bResName.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(GameObject));
	}

	public static void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			mIsLoadingAsset = false;
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			gameObject.name = "PfUiBuddyList";
			mInstance = gameObject.GetComponent<UiBuddyList>();
			if (AvAvatar.pToolbar != null && UiProfile.pInstance == null)
			{
				gameObject.transform.parent = AvAvatar.pToolbar.transform;
				if (!AvAvatar.pToolbar.activeInHierarchy)
				{
					gameObject.SetActive(value: false);
				}
			}
			RsResourceManager.ReleaseBundleData(inURL);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mIsLoadingAsset = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected override void Start()
	{
		base.Start();
		mStillPhotoManager = AvPhotoManager.Init("PfMessagePhotoMgr");
		KAUI.SetExclusive(this);
		KAWidget kAWidget = FindItem("AvatarBkg");
		mIcoPlayerPic = kAWidget.FindChildItem("Picture");
		kAWidget.FindChildItem("TxtName").SetText(AvatarData.pInstance.DisplayName);
		mFriendReqBkg = FindItem("FriendReqBkg");
		mTxtPlayerCode = kAWidget.FindChildItem("TxtBuddyCode");
		mCodeEnterBtn = mFriendReqBkg.FindChildItem("CodeEnterBtn");
		mTxtBuddyCodeEdit = mFriendReqBkg.FindChildItem("TxtBuddyCodeEdit");
		mTabTitle = FindItem("TabTitle");
		mOnlineBtn = (KAToggleButton)FindItem("OnlineBtn");
		mOfflineBtn = (KAToggleButton)FindItem("OfflineBtn");
		mIgnoredBtn = (KAToggleButton)FindItem("IgnoredBtn");
		mRequestsBtn = (KAToggleButton)FindItem("RequestsBtn");
		mStillPhotoManager.TakePhotoUI(UserInfo.pInstance.UserID, (Texture2D)mIcoPlayerPic.GetTexture(), ProfileAvPhotoCallback, mIcoPlayerPic);
		mBuddyListMenu = GetComponentInChildren<UiBuddyListMenu>();
		mUiBuddyActionGroup = (UiBuddyActionsGroup)_UiList[0];
		mUiBuddyActionGroup.InitItems();
		OnClick(mOnlineBtn);
		BuddyList.ReInit(UpdateBuddyListCallac);
		UserInfo.GetBuddyCode(MyBuddyCodeEventHandler);
		if (_TutorialTextAsset != null)
		{
			TutorialManager.StartTutorial(_TutorialTextAsset, _Intro, bMarkDone: true, 12u, null);
		}
	}

	public void OnDisable()
	{
		BuddyList.RemoveSyncBuddyListEventHandler(UpdateBuddyListCallac);
	}

	public void MyBuddyCodeEventHandler(string code)
	{
		if (mTxtPlayerCode != null)
		{
			mTxtPlayerCode.SetText(code);
		}
	}

	public void UpdateBuddyListCallac()
	{
		UpdateBuddyList();
	}

	public void UpdateBuddyList(bool resetHighlighter = true)
	{
		mBuddyListMenu.ClearItems();
		if (!mFriendReqTabSelected)
		{
			Buddy[] pList = BuddyList.pList;
			if (pList != null)
			{
				Buddy[] array = pList;
				foreach (Buddy buddy in array)
				{
					bool flag = false;
					if (buddy.Status == BuddyStatus.Approved)
					{
						BuddyListType buddyListType = mSelectedBuddyListType;
						if (buddyListType != 0)
						{
							if (buddyListType == BuddyListType.OFFLINE && !buddy.Online)
							{
								goto IL_005d;
							}
						}
						else if (buddy.Online)
						{
							goto IL_005d;
						}
					}
					else if (mSelectedBuddyListType == BuddyListType.IGNORED && (buddy.Status == BuddyStatus.BlockedBySelf || buddy.Status == BuddyStatus.BlockedByBoth))
					{
						flag = true;
					}
					goto IL_0080;
					IL_005d:
					flag = true;
					goto IL_0080;
					IL_0080:
					if (flag)
					{
						AddBuddy(buddy.UserID, buddy.DisplayName, buddy.BestBuddy, buddy.OnMobile);
					}
				}
			}
		}
		else if (BuddyList.pMessageList != null)
		{
			for (int i = 0; i < BuddyList.pMessageList.Count; i++)
			{
				MessageInfo messageInfo = BuddyList.pMessageList[i];
				if (messageInfo.MessageTypeID == 5)
				{
					KAWidget kAWidget = DuplicateWidget(mBuddyListMenu._Template);
					kAWidget.SetText(i.ToString());
					kAWidget.SetVisibility(inVisible: true);
					kAWidget.SetInteractive(isInteractive: true);
					KAWidget kAWidget2 = kAWidget.FindChildItem("TxtBuddyName");
					Buddy buddy2 = BuddyList.pInstance.GetBuddy(messageInfo.FromUserID);
					if (buddy2 != null && !string.IsNullOrEmpty(buddy2.DisplayName))
					{
						kAWidget2.SetVisibility(inVisible: true);
						kAWidget2.SetText(buddy2.DisplayName);
					}
					else
					{
						WsWebService.GetDisplayNameByUserID(messageInfo.FromUserID, ServiceEventHandler, kAWidget2);
					}
					KAWidget kAWidget3 = kAWidget.FindChildItem("BuddyPicture");
					mStillPhotoManager.TakePhotoUI(messageInfo.FromUserID, (Texture2D)kAWidget3.GetTexture(), ProfileAvPhotoCallback, kAWidget3);
					kAWidget.FindChildItem("BestBuddyBtn").SetVisibility(inVisible: false);
					mBuddyListMenu.AddWidget(kAWidget);
					kAWidget.SetUserData(new BuddyListData(messageInfo.FromUserID));
				}
			}
		}
		if (resetHighlighter)
		{
			mUiBuddyActionGroup.SetActionAvailable((mBuddyListMenu.GetItemCount() <= 0) ? null : mBuddyListMenu.FindItemAt(0), mSelectedBuddyListType);
		}
	}

	public void AddBuddy(string inUserID, string inName, bool bestBuddy, bool onMobile)
	{
		KAWidget kAWidget = DuplicateWidget(mBuddyListMenu._Template);
		kAWidget.SetText(inUserID);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.SetInteractive(isInteractive: true);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtBuddyName");
		kAWidget2.SetText(inName);
		kAWidget2.SetInteractive(isInteractive: true);
		kAWidget2.SetVisibility(inVisible: true);
		if (string.IsNullOrEmpty(inName))
		{
			WsWebService.GetDisplayNameByUserID(inUserID, ServiceEventHandler, kAWidget2);
		}
		KAWidget kAWidget3 = kAWidget.FindChildItem("BuddyPicture");
		mStillPhotoManager.TakePhotoUI(inUserID, (Texture2D)kAWidget3.GetTexture(), ProfileAvPhotoCallback, kAWidget3);
		kAWidget3.SetVisibility(inVisible: true);
		kAWidget2 = kAWidget.FindChildItem("BestBuddyBtn");
		kAWidget2.SetVisibility(mSelectedBuddyListType != BuddyListType.IGNORED);
		((KAToggleButton)kAWidget2).SetChecked(bestBuddy);
		kAWidget2.SetToolTipText(bestBuddy ? StringTable.GetStringData(_UnBestBuddyText._ID, _UnBestBuddyText._Text) : StringTable.GetStringData(_AddBestBuddyText._ID, _AddBestBuddyText._Text));
		kAWidget2 = kAWidget.FindChildItem("BestBuddyBkg");
		kAWidget2.SetVisibility(bestBuddy && mSelectedBuddyListType != BuddyListType.IGNORED);
		kAWidget2 = kAWidget.FindChildItem("MobilePic");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(onMobile);
		}
		mBuddyListMenu.AddWidget(kAWidget);
		if (mUiBuddyActionGroup.CurrentSelectedBuddy != null && mUiBuddyActionGroup.CurrentSelectedBuddy.GetText().Equals(kAWidget.GetText()))
		{
			mUiBuddyActionGroup.CurrentSelectedBuddy = kAWidget;
		}
		kAWidget.SetUserData(new BuddyListData(inUserID));
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			KAWidget kAWidget = (KAWidget)inUserData;
			if (!(kAWidget == null))
			{
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetText((string)inObject);
			}
		}
	}

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		KAWidget kAWidget = (KAWidget)inUserData;
		if (!(kAWidget == null))
		{
			kAWidget.SetVisibility(inVisible: false);
			kAWidget.SetTexture((Texture2D)tex);
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		TutorialManager.StopTutorials();
		string text = item.name;
		if (text == "CloseBtn")
		{
			CloseUI();
			Input.ResetInputAxes();
		}
		else if (item == mOnlineBtn)
		{
			mTabTitle.SetText(item.GetText());
			mFriendReqTabSelected = false;
			mFriendReqBkg.SetVisibility(inVisible: false);
			mSelectedBuddyListType = BuddyListType.ONLINE;
			UpdateBuddyList();
		}
		else if (item == mOfflineBtn)
		{
			mTabTitle.SetText(item.GetText());
			mFriendReqTabSelected = false;
			mFriendReqBkg.SetVisibility(inVisible: false);
			mSelectedBuddyListType = BuddyListType.OFFLINE;
			UpdateBuddyList();
		}
		else if (item == mIgnoredBtn)
		{
			mTabTitle.SetText(item.GetText());
			mFriendReqTabSelected = false;
			mFriendReqBkg.SetVisibility(inVisible: false);
			mSelectedBuddyListType = BuddyListType.IGNORED;
			UpdateBuddyList();
		}
		else if (item == mRequestsBtn)
		{
			mTabTitle.SetText(item.GetText());
			mFriendReqTabSelected = true;
			mFriendReqBkg.SetVisibility(inVisible: true);
			mSelectedBuddyListType = BuddyListType.REQUEST;
			UpdateBuddyList();
		}
		else if (text == "DragTab" && !mMoving)
		{
			AvAvatar.pInputEnabled = false;
			ObClickable.pGlobalActive = false;
			UICursorManager.pCursorManager.SetVisibility(inHide: false);
			KAUI.SetExclusive(this, Color.clear);
			Input.ResetInputAxes();
			mMoving = true;
			mMovingOffset = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(KAInput.mousePosition.x, KAInput.mousePosition.y, 0f));
			mMovingOffset -= new Vector2(base.transform.localPosition.x, base.transform.localPosition.y);
		}
		else if (text == "BuddyPicture")
		{
			CloseUI();
			ProfileLoader.ShowProfile(item.GetParentItem().GetText());
		}
		else if (item == mTxtBuddyCodeEdit)
		{
			AvAvatar.pInputEnabled = false;
			mCodeEnterBtn.SetDisabled(isDisabled: true);
		}
		else if (text == "BestBuddyBtn")
		{
			mUiBuddyActionGroup.CurrentSelectedBuddy = item.GetParentItem();
			KAToggleButton obj = (KAToggleButton)item;
			string text2 = item.GetParentItem().GetText();
			bool bestBuddy = obj.IsChecked();
			WsWebService.UpdateBestBuddy(text2, bestBuddy, null, null);
			Buddy buddy = BuddyList.pInstance.GetBuddy(text2);
			if (buddy != null)
			{
				buddy.BestBuddy = bestBuddy;
			}
			BuddyList.Sort();
			UpdateBuddyList(resetHighlighter: false);
		}
		else if (item == mCodeEnterBtn)
		{
			mCodeEnterBtn.SetDisabled(isDisabled: true);
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			KAUI.SetExclusive(this, _MaskColor);
			AvAvatar.pInputEnabled = false;
			BuddyList.pInstance.AddBuddyByFriendCode(mTxtBuddyCodeEdit.GetText(), BuddyActionEventHandler);
			mTxtBuddyCodeEdit.SetText("");
		}
		else if (text == "BuddyTemplate")
		{
			mUiBuddyActionGroup.CurrentSelectedBuddy = item;
			UpdateSelection();
			BuddyListData buddyListData = (BuddyListData)item.GetUserData();
			if (buddyListData != null && buddyListData._ClanData != null)
			{
				mUiBuddyActionGroup.SetClanActionAvailable(clan: true);
			}
			else
			{
				mUiBuddyActionGroup.SetClanActionAvailable(clan: false);
			}
		}
	}

	private void OnFarmUIClosed()
	{
		SetVisibility(inVisible: true);
		UpdateSelection();
	}

	private void ShowDialog(LocaleString text)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mUiGenericDB = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
		component._MessageObject = base.gameObject;
		component._OKMessage = "OnClose";
		component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		component.SetTextByID(text._ID, text._Text, interactive: false);
		KAUI.SetExclusive(component, _MaskColor);
		AvAvatar.pInputEnabled = false;
		if (_MessageSFX != null)
		{
			SnChannel.Play(_MessageSFX, "SFX_Pool", inForce: true, null);
		}
	}

	public void OnDisconnect()
	{
		OnClose();
	}

	public void OnClose()
	{
		SetInteractive(interactive: true);
		ObClickable.pGlobalActive = true;
		AvAvatar.pInputEnabled = true;
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB.GetComponent<KAUIGenericDB>());
			Object.Destroy(mUiGenericDB);
			mUiGenericDB = null;
		}
		KAUI.SetExclusive(this);
		UpdateSelection();
	}

	public void CloseUI()
	{
		if (UiProfile.pInstance == null)
		{
			AvAvatar.pInputEnabled = true;
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		Object.Destroy(base.gameObject);
	}

	public void OnIgnore()
	{
		if (mSelectedBuddyListType != BuddyListType.REQUEST)
		{
			KAUI.SetExclusive(this, _MaskColor);
			KAUICursorManager.SetDefaultCursor("Loading");
			SetInteractive(interactive: false);
			BuddyList.pInstance.BlockBuddy(mUiGenericDB.name, BuddyActionEventHandler);
		}
		else if (int.TryParse(mUiBuddyActionGroup.CurrentSelectedBuddy.GetText(), out mCurrentMessageIndex))
		{
			KAUI.SetExclusive(this, _MaskColor);
			KAUICursorManager.SetDefaultCursor("Loading");
			SetInteractive(interactive: false);
			BuddyList.pInstance.BlockBuddy(BuddyList.pMessageList[mCurrentMessageIndex].FromUserID, BuddyReqActionEventHandler);
		}
		Object.Destroy(mUiGenericDB);
	}

	public void OnDelete()
	{
		if (mSelectedBuddyListType != BuddyListType.REQUEST)
		{
			KAUI.SetExclusive(this, _MaskColor);
			KAUICursorManager.SetDefaultCursor("Loading");
			SetInteractive(interactive: false);
			BuddyList.pInstance.RemoveBuddy(mUiGenericDB.name, BuddyActionEventHandler);
		}
		else if (int.TryParse(mUiBuddyActionGroup.CurrentSelectedBuddy.GetText(), out mCurrentMessageIndex))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			SetInteractive(interactive: false);
			KAUI.SetExclusive(this, _MaskColor);
			BuddyList.pInstance.RemoveBuddy(BuddyList.pMessageList[mCurrentMessageIndex].FromUserID, BuddyReqActionEventHandler);
		}
		Object.Destroy(mUiGenericDB);
	}

	private void BuddyActionEventHandler(WsServiceType inType, object inResult)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		AvAvatar.pInputEnabled = true;
		KAUI.RemoveExclusive(this);
		if (inResult == null)
		{
			ShowErrorDB();
			return;
		}
		if (inType == WsServiceType.ADD_BUDDY_BY_FRIEND_CODE)
		{
			BuddyActionResult buddyActionResult = (BuddyActionResult)inResult;
			switch (buddyActionResult.Result)
			{
			case BuddyActionResultType.Unknown:
				ShowDialog(_Strings._GenericErrorText);
				break;
			case BuddyActionResultType.Success:
				ShowDialog(_Strings._BuddyCodeSuccessText);
				break;
			case BuddyActionResultType.BuddyListFull:
				ShowDialog(_Strings._BuddyListFullText);
				break;
			case BuddyActionResultType.FriendBuddyListFull:
				ShowDialog(_Strings._OtherBuddyListFullText);
				break;
			case BuddyActionResultType.AlreadyInList:
				if (buddyActionResult.Status == BuddyStatus.BlockedByOther)
				{
					ShowDialog(_Strings._BlockedByOtherText);
				}
				else if (BuddyList.pInstance != null && BuddyList.pIsReady && BuddyList.pInstance.GetBuddyStatus(buddyActionResult.BuddyUserID) == BuddyStatus.Approved)
				{
					ShowDialog(_Strings._AlreadyInListText);
				}
				else
				{
					ShowDialog(_Strings._WaitingForApprovalText);
				}
				break;
			case BuddyActionResultType.InvalidFriendCode:
				ShowDialog(_Strings._InvalidBuddyCodeText);
				break;
			case BuddyActionResultType.CannotAddSelf:
				ShowDialog(_Strings._AddSelfText);
				break;
			}
		}
		UpdateBuddyList();
	}

	private void BuddyReqActionEventHandler(WsServiceType inType, object inResult)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		KAUI.RemoveExclusive(this);
		AvAvatar.pInputEnabled = true;
		if (inResult == null)
		{
			ShowErrorDB();
			return;
		}
		RemoveMessage();
		UpdateBuddyList();
	}

	public void OnCloseWindows(GameObject go)
	{
		if (go != base.gameObject)
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mMoving)
		{
			return;
		}
		if (Input.GetMouseButtonUp(0))
		{
			mMoving = false;
			AvAvatar.pInputEnabled = true;
			ObClickable.pGlobalActive = true;
			UICursorManager.pCursorManager.SetVisibility(inHide: true);
			KAUI.RemoveExclusive(this);
			Input.ResetInputAxes();
			return;
		}
		Vector2 vector = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(KAInput.mousePosition.x, KAInput.mousePosition.y, 0f));
		Vector2 vector2 = vector - mMovingOffset;
		Vector2 vector3 = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
		vector3 -= _Size * 0.5f;
		Rect rect = new Rect(0f - vector3.x, 0f - vector3.y, vector3.x * 2f, vector3.y * 2f);
		if (vector2.x < rect.xMin)
		{
			mMovingOffset.x = vector.x - rect.xMin;
			vector2.x = rect.xMin;
		}
		else if (vector2.x > rect.xMax)
		{
			mMovingOffset.x = vector.x - rect.xMax;
			vector2.x = rect.xMax;
		}
		if (vector2.y < rect.yMin)
		{
			mMovingOffset.y = vector.y - rect.yMin;
			vector2.y = rect.yMin;
		}
		else if (vector2.y > rect.yMax)
		{
			mMovingOffset.y = vector.y - rect.yMax;
			vector2.y = rect.yMax;
		}
		base.transform.localPosition = new Vector3(vector2.x, vector2.y, base.transform.localPosition.z);
	}

	private void BuddyListEventHandler(WsServiceType inType, object inResult)
	{
		if (inResult == null || (JoinBuddyResultType)inResult == JoinBuddyResultType.JoinFailedCommon)
		{
			ShowDialog(_Strings._JoinBuddyErrorText);
			return;
		}
		if ((JoinBuddyResultType)inResult == JoinBuddyResultType.JoinFailedBuddyLeft)
		{
			ShowDialog(_Strings._BuddyLeftText);
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		OnClose();
		Object.Destroy(base.gameObject);
	}

	public override void OnInput(KAWidget inWidget, string inText)
	{
		base.OnInput(inWidget, inText);
		if (inWidget == mTxtBuddyCodeEdit)
		{
			UpdateCodeEnterButtonState();
		}
	}

	public void ProcessFocusLost(KAWidget item, bool enterHit)
	{
		if (!(item != mTxtBuddyCodeEdit))
		{
			UpdateCodeEnterButtonState();
			AvAvatar.pInputEnabled = true;
		}
	}

	private void UpdateCodeEnterButtonState()
	{
		string text = mTxtBuddyCodeEdit.GetText();
		bool disabled = text.Trim().Length < 1 || text.Length == 0;
		mCodeEnterBtn.SetDisabled(disabled);
	}

	private void JoinOwnerSpace(string level, string text)
	{
		if (MainStreetMMOClient.pIsMMOEnabled && MainStreetMMOClient.pInstance.JoinOwnerSpace(level, text))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			AvAvatar.SetActive(inActive: false);
			Input.ResetInputAxes();
			CloseUI();
		}
	}

	private void ApproveBuddyActionEventHandler(WsServiceType inType, object inResult)
	{
		if (inResult == null)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			ShowErrorDB();
		}
		else
		{
			RemoveMessage();
			BuddyList.ReInit(UpdateBuddyListOnApprove);
		}
	}

	private void RemoveMessage()
	{
		if (mCurrentMessageIndex > -1 && mCurrentMessageIndex < BuddyList.pMessageList.Count)
		{
			if (BuddyList.pMessageList[mCurrentMessageIndex].UserMessageQueueID.HasValue)
			{
				WsWebService.SaveMessage(BuddyList.pMessageList[mCurrentMessageIndex].UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
			}
			UiChatHistory.SystemMessageAccepted(BuddyList.pMessageList[mCurrentMessageIndex]);
			BuddyList.RemoveMessage(mCurrentMessageIndex);
			mCurrentMessageIndex = -1;
		}
	}

	private void ShowErrorDB()
	{
		mUiGenericDB = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
		component._MessageObject = base.gameObject;
		component._OKMessage = "OnClose";
		component.SetText(StringTable.GetStringData(_Strings._GenericErrorText._ID, _Strings._GenericErrorText._Text), interactive: false);
		component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(component, _MaskColor);
		AvAvatar.pInputEnabled = false;
	}

	private void UpdateBuddyListOnApprove()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		KAUI.RemoveExclusive(this);
		AvAvatar.pInputEnabled = true;
		BuddyList.RemoveSyncBuddyListEventHandler(UpdateBuddyListOnApprove);
		UpdateBuddyList();
	}

	public void BuddyListActions(string itemName)
	{
		KAWidget currentSelectedBuddy = mUiBuddyActionGroup.CurrentSelectedBuddy;
		if (currentSelectedBuddy == null)
		{
			return;
		}
		switch (itemName)
		{
		case "MessagesBtn":
			CloseUI();
			MessageBoardLoader.Load(currentSelectedBuddy.GetText());
			break;
		case "InviteBtn":
			SetInteractive(interactive: false);
			ObClickable.pGlobalActive = false;
			BuddyList.pInstance.InviteBuddy(currentSelectedBuddy.GetText(), null);
			ShowDialog(_Strings._InviteSentText);
			break;
		case "VisitBtn":
			Input.ResetInputAxes();
			KAUICursorManager.SetDefaultCursor("Loading");
			SetInteractive(interactive: false);
			KAUI.SetExclusive(this, Color.clear);
			ObClickable.pGlobalActive = false;
			FarmManager.pCurrentFarmData = null;
			BuddyList.pInstance.JoinBuddy(currentSelectedBuddy.GetText(), BuddyListEventHandler);
			break;
		case "HomeBtn":
			JoinOwnerSpace(_HouseLevel, currentSelectedBuddy.GetText());
			break;
		case "FarmingBtn":
			SetVisibility(inVisible: false);
			UiFarms.OpenFriendFarmListUI(currentSelectedBuddy.GetText(), base.gameObject);
			break;
		case "IgnoreBtn":
		{
			string s = currentSelectedBuddy.GetText();
			if (mFriendReqTabSelected && int.TryParse(s, out var result))
			{
				s = BuddyList.pMessageList[result].FromUserID;
			}
			mUiGenericDB = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
			KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
			mUiGenericDB.name = s;
			component._MessageObject = base.gameObject;
			component._YesMessage = "OnIgnore";
			component._NoMessage = "OnClose";
			component.SetText(StringTable.GetStringData(_Strings._IgnorePlayerText._ID, _Strings._IgnorePlayerText._Text), interactive: false);
			component.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			KAUI.SetExclusive(component, _MaskColor);
			AvAvatar.pInputEnabled = false;
			break;
		}
		case "DeleteBtn":
			if (mBuddyListMenu.GetNumItems() > 0)
			{
				mUiGenericDB = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
				KAUIGenericDB component2 = mUiGenericDB.GetComponent<KAUIGenericDB>();
				mUiGenericDB.name = currentSelectedBuddy.GetText();
				component2._MessageObject = base.gameObject;
				component2._YesMessage = "OnDelete";
				component2._NoMessage = "OnClose";
				component2.SetText(StringTable.GetStringData(_Strings._DeletePlayerText._ID, _Strings._DeletePlayerText._Text), interactive: false);
				component2.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				KAUI.SetExclusive(component2, _MaskColor);
				AvAvatar.pInputEnabled = false;
			}
			else if (mBuddyListMenu.GetNumItems() > 0 && mSelectedBuddyListType == BuddyListType.REQUEST && int.TryParse(currentSelectedBuddy.GetText(), out mCurrentMessageIndex))
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				SetInteractive(interactive: false);
				KAUI.SetExclusive(this, _MaskColor);
				AvAvatar.pInputEnabled = false;
				BuddyList.pInstance.RemoveBuddy(BuddyList.pMessageList[mCurrentMessageIndex].FromUserID, BuddyReqActionEventHandler);
			}
			break;
		case "AddBtn":
			if (int.TryParse(currentSelectedBuddy.GetText(), out mCurrentMessageIndex))
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				SetInteractive(interactive: false);
				KAUI.SetExclusive(this, _MaskColor);
				AvAvatar.pInputEnabled = false;
				string fromUserID = BuddyList.pMessageList[mCurrentMessageIndex].FromUserID;
				bool addFromFriendCode = UiBuddyMessage.ShouldAddThroughFriendCode(BuddyList.pMessageList[mCurrentMessageIndex]);
				BuddyList.pInstance.ApproveBuddy(fromUserID, ApproveBuddyActionEventHandler, addFromFriendCode);
				UserAchievementTask.Set(new List<AchievementTask>
				{
					new AchievementTask(_SocialAddBuddyAchievementID, fromUserID),
					new AchievementTask(fromUserID, _SocialOtherPlayerAddBuddyAchievementID, UserInfo.pInstance.UserID),
					new AchievementTask(_ApproveBuddyAchievementID, fromUserID),
					new AchievementTask(fromUserID, _ApproveBuddyAchievementID, UserInfo.pInstance.UserID)
				}.ToArray());
			}
			break;
		case "ClanCrestTemplate":
		case "ClanBtn":
		{
			BuddyListData buddyListData = (BuddyListData)currentSelectedBuddy.GetUserData();
			if (buddyListData != null && buddyListData._ClanData != null)
			{
				UiClans.ShowClan(buddyListData._UserID, buddyListData._ClanData._Group);
			}
			break;
		}
		}
	}

	public void UpdateSelection()
	{
		if (mUiBuddyActionGroup.CurrentSelectedBuddy != null)
		{
			mBuddyListMenu.OnSelect(mUiBuddyActionGroup.CurrentSelectedBuddy, inSelected: true);
		}
	}

	public void ClanDataUpdated(KAWidget item)
	{
		if (mUiBuddyActionGroup.CurrentSelectedBuddy == item)
		{
			mUiBuddyActionGroup.SetClanActionAvailable(clan: true);
		}
	}
}
