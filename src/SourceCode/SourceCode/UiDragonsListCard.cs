using UnityEngine;

public class UiDragonsListCard : UiCard
{
	public enum Mode
	{
		All = 1,
		NestedDragons = 2,
		CurrentStableDragons = 4,
		ForceDragonSelection = 5
	}

	public delegate void OnDragonAgeUpDone();

	private KAWidget mCurrentDragonItem;

	private UiDragonsListCardMenu mUiDragonsListCardMenu;

	private int mSetPetID;

	private int mLastSelectedDragon = -1;

	private bool mShowInfoCard;

	private RaisedPetData mRaisedPetData;

	private KAWidget mAgeUpWidget;

	private Mode mCurrentMode = Mode.All;

	private int mAllocationStableID = -1;

	private int mAllocationNestID = -1;

	private UiDragonsStable mUiDragonsStable;

	public Mode pCurrentMode
	{
		get
		{
			return mCurrentMode;
		}
		set
		{
			mCurrentMode = value;
		}
	}

	public int pAllocationStableID => mAllocationStableID;

	public int pAllocationNestID => mAllocationNestID;

	protected override void Awake()
	{
		base.Awake();
		mUiDragonsStable = (UiDragonsStable)_UiCardParent;
	}

	public override void SetVisibility(bool inVisible)
	{
		bool visibility = GetVisibility();
		base.SetVisibility(inVisible);
		if (!visibility && inVisible)
		{
			InitializeUI();
			mUiDragonsListCardMenu.LoadDragonsList();
			mUiDragonsStable.SetMessage(this, mUiDragonsStable._SelectDragonText);
		}
	}

	public void SetNestAllocationModeInfo(int stableID, int nestID)
	{
		mAllocationStableID = stableID;
		mAllocationNestID = nestID;
	}

	public void RefreshUI()
	{
		InitializeUI();
		mUiDragonsListCardMenu.LoadDragonsList();
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (string.Equals(inWidget.name, "AgeUpIcon"))
		{
			mAgeUpWidget = inWidget;
			base.OnClick(inWidget);
			inWidget = inWidget.transform.parent.GetComponent<KAButton>();
			UiDragonsListCardMenu.StablesPetUserData stablesPetUserData = (UiDragonsListCardMenu.StablesPetUserData)inWidget.GetUserData();
			if (stablesPetUserData.pData.RaisedPetID == mUiDragonsStable.pUiDragonsInfoCard.pSelectedPetID)
			{
				mUiDragonsStable.SetVisibility(inVisible: false);
				mRaisedPetData = stablesPetUserData.pData;
				UiDragonsAgeUp.Init();
				mUiDragonsStable.Exit();
			}
		}
		base.OnClick(inWidget);
		if (mCurrentDragonItem == inWidget)
		{
			SelectDragon(SanctuaryManager.pCurPetInstance.pData.RaisedPetID);
		}
	}

	public override void OnExitClicked()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnCardExit", SendMessageOptions.DontRequireReceiver);
		}
		PopOutCard();
	}

	private void InitializeUI()
	{
		StableData.UpdateInfo();
		mUiDragonsListCardMenu = GetComponentInChildren<UiDragonsListCardMenu>();
		mCurrentDragonItem = FindItem("CurrentDragon");
		if ((bool)SanctuaryManager.pCurPetInstance)
		{
			KAWidget kAWidget = mCurrentDragonItem.FindChildItem("LockedIcon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(SanctuaryManager.IsPetLocked(SanctuaryManager.pCurPetInstance.pData));
			}
			mCurrentDragonItem.SetVisibility(inVisible: true);
			RaisedPetData pData = SanctuaryManager.pCurPetInstance.pData;
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(pData.PetTypeID);
			if (RaisedPetData.GetAgeIndex(pData.pStage) < sanctuaryPetTypeInfo._AgeData.Length - 1 && !TimedMissionManager.pInstance.IsPetEngaged(pData.RaisedPetID))
			{
				KAWidget kAWidget2 = mCurrentDragonItem.FindChildItem("AgeUpIcon");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: true);
					kAWidget2.transform.parent.GetComponent<KAButton>().SetUserData(new UiDragonsListCardMenu.StablesPetUserData(pData));
				}
			}
			SetSelectedDragonItem(pData);
		}
		else
		{
			mCurrentDragonItem.SetVisibility(inVisible: false);
		}
	}

	public void SetSelectedDragonItem(RaisedPetData pData)
	{
		KAWidget kAWidget = mCurrentDragonItem.FindChildItem("TxtDragonName");
		if (kAWidget != null)
		{
			kAWidget.SetText(pData.Name);
		}
		KAWidget kAWidget2 = mCurrentDragonItem.FindChildItem("TxtAge");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(SanctuaryData.GetDisplayTextFromPetAge(pData.pStage) + " " + SanctuaryData.FindSanctuaryPetTypeInfo(pData.PetTypeID)._NameText.GetLocalizedString());
		}
		int slotIdx = (pData.ImagePosition.HasValue ? pData.ImagePosition.Value : 0);
		ImageData.Load("EggColor", slotIdx, base.gameObject);
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (!(img.mIconTexture == null))
		{
			mCurrentDragonItem.FindChildItem("DragonIco").SetTexture(img.mIconTexture);
		}
	}

	public void SetDragonSelected(int petID)
	{
		mSetPetID = petID;
		PopOutCard();
		StableManager.RefreshActivePet();
	}

	public void SetPetHandler(bool success)
	{
		if (success)
		{
			SanctuaryManager.SetAndSaveCurrentType(RaisedPetData.GetByID(mSetPetID).PetTypeID);
			SanctuaryManager.pInstance._ForceLoadPetData = true;
			if (SanctuaryManager.pCurPetInstance != null)
			{
				Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
				SanctuaryManager.pCurPetInstance = null;
			}
			SanctuaryManager.pInstance.ReloadPet(resetFollowFlag: true);
			StableManager.RefreshActivePet();
		}
		else
		{
			UtDebug.Log("Set Current pet failed");
		}
	}

	public void SelectDragon(int petID)
	{
		if (mLastSelectedDragon == petID && mUiDragonsStable.pUiDragonsInfoCard.GetVisibility())
		{
			return;
		}
		mLastSelectedDragon = petID;
		KAWidget kAWidget = mCurrentDragonItem.FindChildItem("SelectedFrame");
		if ((bool)kAWidget)
		{
			if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pData != null && petID == SanctuaryManager.pCurPetInstance.pData.RaisedPetID)
			{
				kAWidget.SetVisibility(inVisible: true);
				mUiDragonsListCardMenu.ResetSelection();
			}
			else
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		if (!mUiDragonsStable.pUiDragonsInfoCard.GetVisibility())
		{
			Transition childTrans = new Transition(Transition.Type.SlideOut, Transition.Direction.Right, Transition.ZOrder.Bottom);
			mUiDragonsStable.pUiDragonsInfoCard.PushCard(this, null, childTrans);
			mUiDragonsStable.pUiDragonsInfoCard.pSelectedPetID = petID;
			mUiDragonsStable.pUiDragonsInfoCard.RefreshUI();
		}
		else
		{
			mShowInfoCard = true;
			mUiDragonsStable.pUiDragonsInfoCard.PopOutCard(releaseParentLink: false);
			mUiDragonsStable.pUiDragonsInfoCard.pSelectedPetID = petID;
		}
	}

	public override void TransitionDone()
	{
		base.TransitionDone();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SelectDragon(SanctuaryManager.pCurPetInstance.pData.RaisedPetID);
		}
	}

	public override void ChildReverseTransitionDone()
	{
		base.ChildReverseTransitionDone();
		if (!base.pPopOutCard && mShowInfoCard)
		{
			mShowInfoCard = false;
			mUiDragonsStable.pUiDragonsInfoCard.RefreshUI();
			Transition childTrans = new Transition(Transition.Type.SlideOut, Transition.Direction.Right, Transition.ZOrder.Bottom);
			mUiDragonsStable.pUiDragonsInfoCard.PushCard(this, null, childTrans);
		}
	}

	public override void ParentTransitionDone()
	{
	}

	private void OnUiDragonAgeUpDone()
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mRaisedPetData.PetTypeID);
		if (mRaisedPetData != null && RaisedPetData.GetAgeIndex(mRaisedPetData.pStage) >= sanctuaryPetTypeInfo._AgeData.Length - 1)
		{
			mRaisedPetData = null;
			mAgeUpWidget.SetVisibility(inVisible: false);
			mAgeUpWidget = null;
		}
		else if (mRaisedPetData == null)
		{
			mAgeUpWidget.SetVisibility(inVisible: false);
			mAgeUpWidget = null;
		}
		RefreshUI();
		mUiDragonsStable.pUiDragonsInfoCard.RefreshUI();
		mUiDragonsStable.SetVisibility(inVisible: true);
		AvAvatar.SetUIActive(inActive: false);
	}
}
