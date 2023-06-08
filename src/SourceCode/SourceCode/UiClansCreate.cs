using System;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;

public class UiClansCreate : KAUI
{
	private enum NameValidationStatus
	{
		NONE,
		VALIDATING,
		APPROVED,
		REJECTED
	}

	public enum ClanTicketType
	{
		None,
		InviteOnly,
		Closed,
		ClanSize
	}

	[Serializable]
	public class ClanTicketData
	{
		public int _ItemID;

		public ClanTicketType _Type;
	}

	[Serializable]
	public class ClanTickets
	{
		public int _StoreID;

		public List<ClanTicketData> _Tickets;
	}

	public UiClansCreateTypeMenu _ClanTypeMenu;

	public UiClansCreateSizeMenu _ClanSizeMenu;

	public UiClansCrestSelector _UiCrestSelector;

	public Color _ApprovedTextColor = Color.green;

	public Color _RejectedTextColor = Color.red;

	public int _GemsRequired = 75;

	public ClansCreateGroupResultInfo[] _CreateGroupResultInfo;

	public ClansEditGroupResultInfo[] _EditGroupResultInfo;

	public ClansNameValidationResultInfo[] _NameValidationResultInfo;

	public int _MinNameTextLength = 3;

	public UITexture _CrestLogo;

	public UITexture _CrestBackground;

	public float _ErrorArrowDisplayDuration = 5f;

	public LocaleString _ClanActivationFailedText = new LocaleString("There was a problem with the server.  Please try again.");

	public LocaleString _ClanEditFailedText = new LocaleString("There was a problem with the server.  Please try again.");

	public LocaleString _NameValidationFailedText = new LocaleString("There was a problem with the server.  Please try again.");

	public LocaleString _NameNotLongEnoughText = new LocaleString("The Clan name must be at least 3 characters.");

	public LocaleString _NotEnoughGemsText = new LocaleString("You do not have enough Gems to create a Clan.  Buy some now?");

	public LocaleString _NameRejectedText = new LocaleString("This name is not approved. Please use another name.");

	public LocaleString _ClanCreateInfoNotFilledText = new LocaleString("Please fill in the required information to create a clan.");

	public LocaleString _ClanEditInfoNotFilledText = new LocaleString("Please fill in the required information to modify your clan details.");

	public int _CreateClanAchievementTaskID = 163;

	public ClanTickets _ClanTickets;

	public List<int> _ClanSizes;

	private bool mSetTypeMenuPage;

	private Group mClan;

	private KAEditBox mTxtEditClanName;

	private KAEditBox mTxtEditClanDesc;

	private KAWidget mClanCrest;

	private KAWidget mBtnClanSymbol;

	private KAWidget mBtnCreate;

	private KAWidget mBtnSave;

	private KAWidget mGrpCreate;

	private KAWidget mTxtClanNameError;

	private KAWidget mAniArrowChooseCrest;

	private KAWidget mAniArrowClanName;

	private KAWidget mAniArrowDesc;

	private KAWidget mTxtGemsCount;

	private KAWidget mGemsAniLoading;

	private KAUIGenericDB mKAUIGenericDB;

	private Color mDefaultClanNameTextColor = Color.black;

	private NameValidationStatus mNameValidationStatus;

	private bool mCrestModified;

	private float mErrorArrowDisplayTimer;

	private bool mIsSaveClan;

	private StoreData mClanTicketStoreData;

	private bool mStoreDataFailed;

	private int mDefaultClanSize;

	private int mEditClanSize;

	private int mGemCostForClanSize;

	private int mGemCostForClanType;

	private void Initialize()
	{
		mTxtEditClanName = (KAEditBox)FindItem("TxtEditClanName");
		mTxtEditClanDesc = (KAEditBox)FindItem("TxtEditClanDescription");
		mClanCrest = FindItem("ClanCrestTemplate");
		mBtnClanSymbol = FindItem("BtnClanSymbol");
		mBtnCreate = FindItem("BtnCreate");
		mBtnSave = FindItem("BtnSave");
		mGrpCreate = FindItem("GrpCreate");
		mTxtClanNameError = FindItem("TxtClanNameError");
		mAniArrowChooseCrest = FindItem("AniArrowChooseCrest");
		mAniArrowClanName = FindItem("AniArrowClanName");
		mAniArrowDesc = FindItem("AniArrowDesc");
		mDefaultClanNameTextColor = mTxtEditClanName.GetLabel().color;
		if (mBtnSave != null && mClan == null)
		{
			mBtnSave.SetVisibility(inVisible: false);
		}
		KAWidget kAWidget = FindItem("GemsCount");
		if (kAWidget != null)
		{
			mTxtGemsCount = kAWidget.FindChildItem("AniGemsCount");
			mGemsAniLoading = kAWidget.FindChildItem("AniLoading");
		}
		InitClanSizeMenu();
		_ClanTypeMenu.SetVisibility(inVisible: true);
		_ClanSizeMenu.SetVisibility(inVisible: true);
	}

	protected override void Start()
	{
		base.Start();
		Initialize();
		ItemStoreDataLoader.Load(_ClanTickets._StoreID, OnStoreLoaded);
	}

	protected void SetMenuData(KAUIMenu menu, string data)
	{
		for (int i = 0; i < menu.GetItemCount(); i++)
		{
			if (string.Compare(menu.GetItemAt(i).name, data, StringComparison.OrdinalIgnoreCase) == 0)
			{
				menu.GoToPage(i + 1, instant: true);
				break;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mSetTypeMenuPage)
		{
			mSetTypeMenuPage = false;
			SetMenuData(_ClanTypeMenu, mClan.Type.ToString());
			SetMenuData(_ClanSizeMenu, mClan.MemberLimit.Value.ToString());
		}
		if (mErrorArrowDisplayTimer > 0f)
		{
			mErrorArrowDisplayTimer -= Time.deltaTime;
			if (mErrorArrowDisplayTimer <= 0f)
			{
				mAniArrowClanName.SetVisibility(inVisible: false);
				mAniArrowChooseCrest.SetVisibility(inVisible: false);
				mAniArrowDesc.SetVisibility(inVisible: false);
			}
		}
		if (mIsSaveClan && mNameValidationStatus != NameValidationStatus.VALIDATING && mClanTicketStoreData != null)
		{
			mIsSaveClan = false;
			SetInteractive(interactive: true);
			if (mBtnSave.GetVisibility())
			{
				OnClick(mBtnSave);
			}
			else
			{
				OnClick(mBtnCreate);
			}
		}
	}

	private void InitClanSizeMenu()
	{
		KAWidget kAWidget = FindItem("ClanSizeTemplate");
		if (_ClanSizeMenu != null && kAWidget != null)
		{
			_ClanSizeMenu.ClearItems();
			foreach (int clanSize in _ClanSizes)
			{
				if (mClan == null || clanSize >= mClan.MemberLimit)
				{
					KAWidget kAWidget2 = _ClanSizeMenu.DuplicateWidget(kAWidget);
					kAWidget2.name = clanSize.ToString();
					kAWidget2.SetText(kAWidget2.name);
					kAWidget2.SetVisibility(inVisible: true);
					_ClanSizeMenu.AddWidget(kAWidget2);
				}
			}
		}
		mDefaultClanSize = _ClanSizeMenu.GetClanSize();
	}

	public void InitClanUI(Group inGroup)
	{
		if (mGrpCreate != null && !mGrpCreate.GetVisibility())
		{
			mClan = inGroup;
			base.gameObject.SetActive(value: true);
			mEditClanSize = 0;
			Reset();
			mGrpCreate.SetVisibility(inVisible: true);
			mBtnSave.SetVisibility(inVisible: false);
			InitClanSizeMenu();
			SetDefaultGem();
			mNameValidationStatus = NameValidationStatus.NONE;
			mSetTypeMenuPage = false;
		}
	}

	public void EditClan(Group inGroup, Texture inTexture)
	{
		mClan = inGroup;
		base.gameObject.SetActive(value: true);
		Initialize();
		if (mClan.MemberLimit.HasValue)
		{
			mEditClanSize = mClan.MemberLimit.Value;
		}
		mGrpCreate.SetVisibility(inVisible: false);
		mBtnSave.SetVisibility(inVisible: true);
		mTxtEditClanName.SetText(mClan.Name);
		mTxtEditClanDesc.SetText(mClan.Description);
		mTxtGemsCount.SetText("0");
		mNameValidationStatus = NameValidationStatus.APPROVED;
		mSetTypeMenuPage = true;
		ClansCrestInfo clansCrestInfo = new ClansCrestInfo();
		clansCrestInfo.Logo = mClan.Logo;
		clansCrestInfo.CrestIcon = inTexture;
		mClan.GetFGColor(out clansCrestInfo.ColorFG);
		mClan.GetBGColor(out clansCrestInfo.ColorBG);
		_UiCrestSelector.pCrestInfo = clansCrestInfo;
		UpdateCrest(clansCrestInfo);
	}

	private void OnDisable()
	{
		if (mTxtClanNameError != null)
		{
			mTxtClanNameError.SetVisibility(inVisible: false);
		}
		mClan = null;
		_UiCrestSelector.gameObject.SetActive(value: false);
		SetVisibility(inVisible: true);
	}

	private void ShowGenericDB(LocaleString inString, string inOkMessageFunction)
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetText(inString.GetLocalizedString(), interactive: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = inOkMessageFunction;
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	public override void OnInput(KAWidget inWidget, string inText)
	{
		base.OnInput(inWidget, inText);
		if (inWidget == mTxtEditClanName && mNameValidationStatus != 0)
		{
			mTxtEditClanName.GetLabel().color = mDefaultClanNameTextColor;
			mNameValidationStatus = NameValidationStatus.NONE;
			if (mTxtClanNameError.GetVisibility())
			{
				mTxtClanNameError.SetVisibility(inVisible: false);
			}
		}
	}

	public override void OnSelect(KAWidget inWidget, bool inSelected)
	{
		base.OnSelect(inWidget, inSelected);
		if (inSelected || !(inWidget == mTxtEditClanName) || string.Equals(inWidget.GetText(), ((KAEditBox)inWidget)._DefaultText.GetLocalizedString()))
		{
			return;
		}
		mTxtEditClanName.SetText(mTxtEditClanName.GetText().TrimEnd());
		if (mNameValidationStatus == NameValidationStatus.NONE)
		{
			if (mClan != null && mClan.Name == inWidget.GetText())
			{
				mTxtEditClanName.GetLabel().color = _ApprovedTextColor;
				mNameValidationStatus = NameValidationStatus.APPROVED;
				return;
			}
			if (inWidget.GetText().Length < _MinNameTextLength)
			{
				mTxtClanNameError.SetVisibility(inVisible: true);
				mTxtClanNameError.SetText(_NameNotLongEnoughText.GetLocalizedString());
				return;
			}
			mNameValidationStatus = NameValidationStatus.VALIDATING;
			WsWebService.ValidateName(new NameValidationRequest
			{
				Category = NameCategory.Group,
				Name = inWidget.GetText()
			}, GroupsEventHandler, inWidget);
		}
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

	private void BuyGemsOnline()
	{
		KillGenericDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private bool IsRequiredInfoFilled()
	{
		if (mNameValidationStatus == NameValidationStatus.VALIDATING)
		{
			SetInteractive(interactive: false);
			mIsSaveClan = true;
			return false;
		}
		bool flag = true;
		if (mNameValidationStatus != NameValidationStatus.APPROVED)
		{
			mAniArrowClanName.SetVisibility(inVisible: true);
			flag = false;
		}
		if (string.IsNullOrEmpty(mTxtEditClanDesc.GetText()) || string.Equals(mTxtEditClanDesc.GetText(), mTxtEditClanDesc._DefaultText.GetLocalizedString()))
		{
			mAniArrowDesc.SetVisibility(inVisible: true);
			flag = false;
		}
		if (!mCrestModified)
		{
			mAniArrowChooseCrest.SetVisibility(inVisible: true);
			flag = false;
		}
		if (!flag)
		{
			if (mBtnSave.GetVisibility())
			{
				ShowGenericDB(_ClanEditInfoNotFilledText, "KillGenericDB");
			}
			else
			{
				ShowGenericDB(_ClanCreateInfoNotFilledText, "KillGenericDB");
			}
			mErrorArrowDisplayTimer = _ErrorArrowDisplayDuration;
		}
		return flag;
	}

	public override void SetInteractive(bool interactive)
	{
		base.SetInteractive(interactive);
		UiClans.pInstance.SetInteractive(interactive);
		_ClanTypeMenu.SetInteractive(interactive);
		KAUICursorManager.SetDefaultCursor(interactive ? "Arrow" : "Loading");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnCreate)
		{
			if (IsRequiredInfoFilled())
			{
				if (Money.pCashCurrency >= GetTotalGemCost())
				{
					SetInteractive(interactive: false);
					CreateGroupRequest obj = new CreateGroupRequest
					{
						Name = mTxtEditClanName.GetText(),
						Description = mTxtEditClanDesc.GetText(),
						Type = _ClanTypeMenu.GetClanType(),
						Logo = _UiCrestSelector.pCrestInfo.Logo,
						MaxMemberLimit = _ClanSizeMenu.GetClanSize()
					};
					string color = HexUtil.ColorToHex(_UiCrestSelector.pCrestInfo.ColorFG) + "," + HexUtil.ColorToHex(_UiCrestSelector.pCrestInfo.ColorBG);
					obj.Color = color;
					WsWebService.CreateGroup(obj, GroupsEventHandler, null);
				}
				else
				{
					ShowNeedMoreGemsDB();
				}
			}
		}
		else if (inWidget == mBtnSave)
		{
			if (IsRequiredInfoFilled())
			{
				if (Money.pCashCurrency >= GetTotalGemCost())
				{
					SetInteractive(interactive: false);
					EditGroupRequest obj2 = new EditGroupRequest
					{
						GroupID = mClan.GroupID,
						Name = mTxtEditClanName.GetText(),
						Description = mTxtEditClanDesc.GetText(),
						Type = _ClanTypeMenu.GetClanType(),
						Logo = _UiCrestSelector.pCrestInfo.Logo,
						MaxMemberLimit = mEditClanSize
					};
					string color2 = HexUtil.ColorToHex(_UiCrestSelector.pCrestInfo.ColorFG) + "," + HexUtil.ColorToHex(_UiCrestSelector.pCrestInfo.ColorBG);
					obj2.Color = color2;
					WsWebService.EditGroup(obj2, GroupsEventHandler, null);
				}
				else
				{
					ShowNeedMoreGemsDB();
				}
			}
		}
		else if (inWidget == mBtnClanSymbol)
		{
			mAniArrowChooseCrest.SetVisibility(inVisible: false);
			ShowCrestSelector(show: true);
		}
	}

	private void GroupsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.CREATE_GROUP:
			switch (inEvent)
			{
			default:
				return;
			case WsServiceEvent.COMPLETE:
			{
				SetInteractive(interactive: true);
				if (inObject == null)
				{
					break;
				}
				CreateGroupResult createGroupResult = (CreateGroupResult)inObject;
				if (createGroupResult == null)
				{
					break;
				}
				mClan = createGroupResult.Group;
				if (mClan != null && !mClan.TotalMemberCount.HasValue)
				{
					mClan.TotalMemberCount = 1;
				}
				string inOkMessageFunction2 = (createGroupResult.Success ? "OnClanCreated" : "KillGenericDB");
				ClansCreateGroupResultInfo[] createGroupResultInfo = _CreateGroupResultInfo;
				foreach (ClansCreateGroupResultInfo clansCreateGroupResultInfo in createGroupResultInfo)
				{
					if (clansCreateGroupResultInfo._Status == createGroupResult.Status)
					{
						ShowGenericDB(clansCreateGroupResultInfo._StatusText, inOkMessageFunction2);
						break;
					}
				}
				return;
			}
			case WsServiceEvent.ERROR:
				break;
			}
			SetInteractive(interactive: true);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ClanActivationFailedText.GetLocalizedString(), null, null, "");
			break;
		case WsServiceType.EDIT_GROUP:
			switch (inEvent)
			{
			default:
				return;
			case WsServiceEvent.COMPLETE:
			{
				SetInteractive(interactive: true);
				if (inObject == null)
				{
					break;
				}
				EditGroupResult editGroupResult = (EditGroupResult)inObject;
				if (editGroupResult == null)
				{
					break;
				}
				if (editGroupResult.Success)
				{
					mClan.Name = mTxtEditClanName.GetText();
					mClan.Description = mTxtEditClanDesc.GetText();
					mClan.Type = _ClanTypeMenu.GetClanType();
					mClan.MemberLimit = mEditClanSize;
					if (mCrestModified)
					{
						mClan.Logo = _UiCrestSelector.pCrestInfo.Logo;
						mClan.Color = HexUtil.ColorToHex(_UiCrestSelector.pCrestInfo.ColorFG) + "," + HexUtil.ColorToHex(_UiCrestSelector.pCrestInfo.ColorBG);
					}
				}
				string inOkMessageFunction = (editGroupResult.Success ? "OnClanEditted" : "KillGenericDB");
				ClansEditGroupResultInfo[] editGroupResultInfo = _EditGroupResultInfo;
				foreach (ClansEditGroupResultInfo clansEditGroupResultInfo in editGroupResultInfo)
				{
					if (clansEditGroupResultInfo._Status == editGroupResult.Status)
					{
						ShowGenericDB(clansEditGroupResultInfo._StatusText, inOkMessageFunction);
						break;
					}
				}
				return;
			}
			case WsServiceEvent.ERROR:
				break;
			}
			SetInteractive(interactive: true);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ClanEditFailedText.GetLocalizedString(), null, null, "");
			break;
		case WsServiceType.VALIDATE_NAME:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				NameValidationResponse nameValidationResponse = (NameValidationResponse)inObject;
				if (nameValidationResponse != null)
				{
					if (string.IsNullOrEmpty(nameValidationResponse.ErrorMessage))
					{
						mNameValidationStatus = NameValidationStatus.APPROVED;
						mTxtEditClanName.GetLabel().color = _ApprovedTextColor;
						break;
					}
					mNameValidationStatus = NameValidationStatus.REJECTED;
					mTxtEditClanName.GetLabel().color = _RejectedTextColor;
					ClansNameValidationResultInfo[] nameValidationResultInfo = _NameValidationResultInfo;
					foreach (ClansNameValidationResultInfo clansNameValidationResultInfo in nameValidationResultInfo)
					{
						if (clansNameValidationResultInfo._Status == nameValidationResponse.Result)
						{
							mTxtClanNameError.SetVisibility(inVisible: true);
							mTxtClanNameError.SetText(clansNameValidationResultInfo._StatusText.GetLocalizedString());
							break;
						}
					}
				}
				else
				{
					mNameValidationStatus = NameValidationStatus.REJECTED;
					mTxtClanNameError.SetVisibility(inVisible: true);
					mTxtClanNameError.SetText(_NameRejectedText.GetLocalizedString());
				}
				break;
			}
			case WsServiceEvent.ERROR:
				mTxtClanNameError.SetVisibility(inVisible: true);
				mTxtClanNameError.SetText(_NameValidationFailedText.GetLocalizedString());
				mNameValidationStatus = NameValidationStatus.REJECTED;
				UtDebug.Log("Error! failed validating name");
				break;
			}
			break;
		}
	}

	private void ShowNeedMoreGemsDB()
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB.SetText(_NotEnoughGemsText.GetLocalizedString(), interactive: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._YesMessage = "BuyGemsOnline";
		mKAUIGenericDB._NoMessage = "KillGenericDB";
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void OnClanCreated()
	{
		Reset();
		KillGenericDB();
		Money.UpdateMoneyFromServer();
		AvatarData.SetGroupName(mClan);
		Group.AddGroup(mClan);
		UserProfile.pProfileData.AddGroup(mClan.GroupID, UserRole.Leader);
		UserAchievementTask.Set(_CreateClanAchievementTaskID, mClan.GroupID);
		UiClans.pInstance.SetClan(mClan);
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		component?.UpdateDisplayName(component.pPlayerMounted, UserProfile.pProfileData.HasGroup());
	}

	private void OnClanEditted()
	{
		Reset();
		KillGenericDB();
		AvatarData.SetGroupName(mClan);
		Group.AddGroup(mClan);
		UiClans.pInstance._UiClansDetails.DetermineAuthority();
		UiClans.pInstance._UiClansDetails.UpdateClanMemberCount();
		UiClans.pInstance.OnClickBackBtn();
		Money.UpdateMoneyFromServer();
	}

	public void ShowCrestSelector(bool show)
	{
		UiClans.pInstance.ShowBackBtn(show, base.gameObject);
		_UiCrestSelector.gameObject.SetActive(show);
		SetVisibility(!show);
		_ClanTypeMenu.SetVisibility(!show);
		_ClanSizeMenu.SetVisibility(!show);
	}

	public void UpdateCrest(ClansCrestInfo crestInfo)
	{
		mCrestModified = true;
		mClanCrest.SetVisibility(inVisible: true);
		if (_CrestLogo != null)
		{
			_CrestLogo.mainTexture = crestInfo.CrestIcon;
			_CrestLogo.color = crestInfo.ColorFG;
		}
		if (_CrestBackground != null)
		{
			_CrestBackground.color = crestInfo.ColorBG;
		}
	}

	private void OnBackBtnClicked()
	{
		ShowCrestSelector(show: false);
	}

	private void Reset()
	{
		mCrestModified = false;
		mTxtEditClanName.GetLabel().color = mDefaultClanNameTextColor;
		mTxtEditClanName.SetText("");
		mTxtEditClanDesc.SetText("");
		if (_ClanTypeMenu.GetNumItems() > 0)
		{
			_ClanTypeMenu.GoToPage(1, instant: true);
		}
		mClanCrest.SetVisibility(inVisible: false);
	}

	private void SetDefaultGem()
	{
		if (mClanTicketStoreData != null)
		{
			switch (GetCurrentClanType())
			{
			case GroupType.InviteOnly:
				UpdateGemsForType(ClanTicketType.InviteOnly);
				break;
			case GroupType.Closed:
				UpdateGemsForType(ClanTicketType.Closed);
				break;
			}
			UpdateGemsForType(ClanTicketType.ClanSize);
		}
	}

	public void OnStoreLoaded(StoreData sd)
	{
		mClanTicketStoreData = sd;
		if (mClanTicketStoreData != null)
		{
			mStoreDataFailed = false;
			if (mGemsAniLoading != null)
			{
				mGemsAniLoading.SetVisibility(inVisible: false);
			}
			if (mTxtGemsCount != null)
			{
				mTxtGemsCount.SetVisibility(inVisible: true);
			}
			SetDefaultGem();
		}
		else
		{
			mStoreDataFailed = true;
		}
	}

	private int GetGemsForTicketType(ClanTicketType type)
	{
		if (mClanTicketStoreData != null && _ClanTickets != null && _ClanTickets._Tickets.Count > 0)
		{
			ClanTicketData clanTicketData = _ClanTickets._Tickets.Find((ClanTicketData t) => t._Type == type);
			if (clanTicketData != null)
			{
				return mClanTicketStoreData.FindItem(clanTicketData._ItemID)?.FinalCashCost ?? 0;
			}
		}
		else if (mStoreDataFailed)
		{
			ItemStoreDataLoader.Load(_ClanTickets._StoreID, OnStoreLoaded);
		}
		return 0;
	}

	public void UpdateGemsForType(ClanTicketType type)
	{
		int gemsForTicketType = GetGemsForTicketType(type);
		switch (type)
		{
		case ClanTicketType.ClanSize:
		{
			int num = ((mClan == null) ? mDefaultClanSize : mClan.MemberLimit.Value);
			mEditClanSize = _ClanSizeMenu.GetClanSize();
			mGemCostForClanSize = ((mEditClanSize > num) ? ((mEditClanSize - num) * gemsForTicketType) : 0);
			break;
		}
		case ClanTicketType.None:
		case ClanTicketType.InviteOnly:
		case ClanTicketType.Closed:
			mGemCostForClanType = gemsForTicketType;
			break;
		}
		mTxtGemsCount.SetText(GetTotalGemCost().ToString());
	}

	public int GetTotalGemCost()
	{
		return (mGrpCreate.GetVisibility() ? _GemsRequired : 0) + mGemCostForClanSize + mGemCostForClanType;
	}

	public GroupType GetCurrentClanType()
	{
		if (mClan == null)
		{
			return GroupType.None;
		}
		return mClan.Type;
	}
}
