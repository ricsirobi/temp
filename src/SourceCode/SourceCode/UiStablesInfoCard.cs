using System;
using UnityEngine;

public class UiStablesInfoCard : UiCard
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

	private UiDragonsStable mUiDragonsStable;

	private UiStablesInfoCardMenu mUiStablesInfoCardMenu;

	private KAWidget mRoomName;

	private bool mRoomNameCleared;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mMoveInClicked;

	private KAWidget mGoBtn;

	private KAWidget mRoomCardBkg;

	private StableData mStableData;

	private NameValidationStatus mNameValidationStatus;

	public int _MinNameTextLength = 3;

	public NameValidationResultInfo[] _NameValidationResultInfo;

	public LocaleString _NameValidationFailedText = new LocaleString("There was a problem with the server.  Please try again.");

	public LocaleString _NameNotLongEnoughText = new LocaleString("The Stable name must be at least 3 characters.");

	public LocaleString _NameRejectedText = new LocaleString("This name is not approved. Please use another name.");

	private Color mDefaultNameTextColor = Color.black;

	public Color _ApprovedTextColor = Color.green;

	public Color _RejectedTextColor = Color.red;

	private int mSelectedStableID = -1;

	private int mSelectedPetID = -1;

	private int mSelectedNestID = -1;

	public int pSelectedStableID
	{
		get
		{
			return mSelectedStableID;
		}
		set
		{
			mSelectedStableID = value;
		}
	}

	public int pSelectedPetID
	{
		get
		{
			return mSelectedPetID;
		}
		set
		{
			mSelectedPetID = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mUiDragonsStable = (UiDragonsStable)_UiCardParent;
	}

	protected override void Start()
	{
		base.Start();
		mRoomName = FindItem("TxtStableName");
		mGoBtn = FindItem("BtnGo");
		mRoomCardBkg = FindItem("RoomCardBkg");
	}

	public void RefreshUI()
	{
		mUiStablesInfoCardMenu = GetComponentInChildren<UiStablesInfoCardMenu>();
		if (mSelectedStableID != -1)
		{
			mStableData = StableData.GetByID(mSelectedStableID);
			StableData stableData = mStableData;
			if (stableData != null)
			{
				mUiStablesInfoCardMenu.LoadNestList(stableData);
				if (mRoomName != null)
				{
					mRoomName.SetText(stableData.pLocaleName);
					mRoomNameCleared = false;
				}
				if (mRoomCardBkg != null)
				{
					string stableBanner = mUiDragonsStable.pUiStablesListCard.GetStableBanner(mStableData.ItemID);
					UISlicedSprite componentInChildren = mRoomCardBkg.GetComponentInChildren<UISlicedSprite>();
					if (!string.IsNullOrEmpty(stableBanner))
					{
						componentInChildren.UpdateSprite(stableBanner);
					}
				}
			}
		}
		mGoBtn.SetVisibility(mSelectedStableID != StableManager.pCurrentStableID);
	}

	public void AssignDragonToNest(int petID)
	{
		StableManager.MovePetToNest(mSelectedStableID, mSelectedNestID, petID);
		mUiDragonsStable.pUiDragonsListCard.PopOutCard();
		RefreshUI();
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
		if (!inSelected && inWidget == mRoomName && !string.Equals(inWidget.GetText(), mStableData.pLocaleName) && mNameValidationStatus == NameValidationStatus.NONE)
		{
			if (string.IsNullOrEmpty(inWidget.GetText()))
			{
				mRoomName.SetText(mStableData.pLocaleName);
				mRoomNameCleared = false;
				return;
			}
			if (inWidget.GetText().Length < _MinNameTextLength)
			{
				ShowGenericDB(_NameNotLongEnoughText, "KillGenericDB");
				return;
			}
			mNameValidationStatus = NameValidationStatus.VALIDATING;
			WsWebService.ValidateName(new NameValidationRequest
			{
				Category = NameCategory.Default,
				Name = inWidget.GetText()
			}, NameValidationEventHandler, inWidget);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		mMoveInClicked = false;
		if (inWidget.name == "BtnMoveIn")
		{
			UiStablesInfoCardMenu.NestPetUserData nestPetUserData = (UiStablesInfoCardMenu.NestPetUserData)inWidget.GetParentItem().GetUserData();
			mSelectedNestID = nestPetUserData._NestID;
			if (UiDragonsStable.pCurrentMode == UiDragonsStable.Mode.DragonSelection)
			{
				StableManager.MovePetToNest(mSelectedStableID, mSelectedNestID, pSelectedPetID);
				RefreshUI();
				pSelectedPetID = -1;
				mUiDragonsStable.pUiDragonsInfoCard.RefreshUI();
				mUiDragonsStable.pUiStablesListCard.PopOutCard();
			}
			else if (mUiDragonsStable.pUiDragonsInfoCard.GetVisibility())
			{
				mMoveInClicked = true;
				mUiDragonsStable.pUiDragonsInfoCard.PopOutCard();
			}
			else if (!mUiDragonsStable.pUiDragonsListCard.GetVisibility())
			{
				if (mUiDragonsStable.pUiStablesListCard.pCurrentMode == UiStablesListCard.Mode.NestAllocation)
				{
					AssignDragonToNest(pSelectedPetID);
					pSelectedPetID = -1;
					mUiDragonsStable.pUiStablesListCard.PopOutCard();
				}
				else
				{
					OpenDragonsList(nestPetUserData.pData != null);
				}
			}
		}
		else if (inWidget.name == "TemplateMyDragons")
		{
			UiStablesInfoCardMenu.NestPetUserData nestPetUserData2 = (UiStablesInfoCardMenu.NestPetUserData)inWidget.GetUserData();
			mSelectedNestID = nestPetUserData2._NestID;
			RaisedPetData petInNest = StableData.GetPetInNest(mSelectedStableID, mSelectedNestID);
			if (petInNest != null)
			{
				if (!mUiDragonsStable.pUiDragonsInfoCard.GetVisibility())
				{
					Transition childTrans = new Transition(Transition.Type.Appear, Transition.Direction.None, Transition.ZOrder.Bottom);
					Transition parentTrans = new Transition(Transition.Type.PushParents, Transition.Direction.Left, Transition.ZOrder.Top);
					mUiDragonsStable.pUiDragonsInfoCard.PushCard(this, parentTrans, childTrans);
					mUiDragonsStable.pUiDragonsInfoCard.pSelectedPetID = petInNest.RaisedPetID;
					mUiDragonsStable.pUiDragonsInfoCard.RefreshUI();
				}
				else
				{
					mUiDragonsStable.pUiDragonsInfoCard.pSelectedPetID = petInNest.RaisedPetID;
					mUiDragonsStable.pUiDragonsInfoCard.RefreshUI();
				}
				mUiDragonsStable.pUiDragonsInfoCard.SetButtons(selectBtn: true, visitBtn: true, moveInBtn: false);
			}
		}
		else if (inWidget.name == "BtnGo")
		{
			mUiDragonsStable.Exit();
			StableManager.LoadStable(mSelectedStableID);
		}
		else if (inWidget == mRoomName && !mRoomNameCleared)
		{
			mRoomName.SetText("");
		}
	}

	public override void OnExitClicked()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnCardExit", SendMessageOptions.DontRequireReceiver);
		}
		mUiDragonsStable.pUiStablesListCard.PopOutCard();
	}

	private void OpenDragonsList(bool isOnlyNestedDragons)
	{
		mUiDragonsStable.pUiStablesListCard.SetInteractive(interactive: false);
		Transition childTrans = new Transition(Transition.Type.SlideOut, Transition.Direction.Left, Transition.ZOrder.Top);
		if (isOnlyNestedDragons)
		{
			mUiDragonsStable.pUiDragonsListCard.pCurrentMode = UiDragonsListCard.Mode.NestedDragons;
		}
		mUiDragonsStable.pUiDragonsListCard.SetNestAllocationModeInfo(mSelectedStableID, mSelectedNestID);
		mUiDragonsStable.pUiDragonsListCard.PushCard(this, null, childTrans);
		mUiDragonsStable.pUiDragonsListCard.SetVisibility(inVisible: true);
	}

	private void NameValidationEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.VALIDATE_NAME)
		{
			return;
		}
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
					if (mRoomName != null)
					{
						mRoomName.GetLabel().color = _ApprovedTextColor;
					}
					if (mStableData != null)
					{
						mStableData.SetStableName(mRoomName.GetText());
						StableData.SaveData();
						mUiDragonsStable.pUiStablesListCard.RefreshUI();
					}
					break;
				}
				mNameValidationStatus = NameValidationStatus.REJECTED;
				mRoomName.GetLabel().color = _RejectedTextColor;
				NameValidationResultInfo[] nameValidationResultInfo = _NameValidationResultInfo;
				foreach (NameValidationResultInfo nameValidationResultInfo2 in nameValidationResultInfo)
				{
					if (nameValidationResultInfo2._Status == nameValidationResponse.Result)
					{
						ShowGenericDB(nameValidationResultInfo2._StatusText, "KillGenericDB");
						break;
					}
				}
			}
			else
			{
				mNameValidationStatus = NameValidationStatus.REJECTED;
				ShowGenericDB(_NameRejectedText, "KillGenericDB");
			}
			break;
		}
		case WsServiceEvent.ERROR:
			ShowGenericDB(_NameValidationFailedText, "KillGenericDB");
			mNameValidationStatus = NameValidationStatus.REJECTED;
			UtDebug.Log("Error! failed validating name");
			break;
		}
	}

	public override void ParentReverseTransitionDone()
	{
		if (mMoveInClicked)
		{
			if (!mUiDragonsStable.pUiDragonsListCard.GetVisibility())
			{
				mMoveInClicked = false;
				OpenDragonsList(isOnlyNestedDragons: true);
			}
		}
		else if (!mUiDragonsStable.pUiDragonsListCard.GetVisibility())
		{
			base.ParentReverseTransitionDone();
		}
	}

	public override void ChildTransitionDone()
	{
	}

	public override void ChildReverseTransitionDone()
	{
		base.ChildReverseTransitionDone();
		mUiDragonsStable.pUiStablesListCard.SetInteractive(interactive: true);
		mUiDragonsStable.pUiStablesListCard.RefreshStableInfoTips();
	}
}
