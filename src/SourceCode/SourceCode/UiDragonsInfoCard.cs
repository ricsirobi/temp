using UnityEngine;

public class UiDragonsInfoCard : UiCard
{
	private KAWidget mSlideInBtn;

	private UiDragonsInfoCardItem mInfoCardItem;

	private UiDragonsStable mUiDragonsStable;

	private int mSelectedPetID = -1;

	public int pSelectedPetID
	{
		get
		{
			return mSelectedPetID;
		}
		set
		{
			mSelectedPetID = value;
			if (mInfoCardItem != null)
			{
				mInfoCardItem.pSelectedPetID = mSelectedPetID;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		mUiDragonsStable = (UiDragonsStable)_UiCardParent;
	}

	private void Init()
	{
		mInfoCardItem = (UiDragonsInfoCardItem)FindItem("TemplateInfoCardItem");
		mInfoCardItem.SetMessageObject(base.gameObject);
		mInfoCardItem.pSelectedPetID = mSelectedPetID;
		mSlideInBtn = FindItem("BtnSlideIn");
	}

	public void SetSlideBtnActive(bool active)
	{
		if (mSlideInBtn != null)
		{
			mSlideInBtn.SetVisibility(active);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mSlideInBtn == inWidget)
		{
			PopOutCard();
		}
	}

	public override void OnExitClicked()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnCardExit", SendMessageOptions.DontRequireReceiver);
		}
		if (mUiDragonsStable.pUiDragonsListCard.GetVisibility())
		{
			mUiDragonsStable.pUiDragonsListCard.PopOutCard();
		}
		else
		{
			PopOutCard();
		}
	}

	public void RefreshUI()
	{
		Init();
		mInfoCardItem.RefreshUI();
		bool flag = true;
		StableData byPetID = StableData.GetByPetID(mSelectedPetID);
		if (mUiDragonsStable == null)
		{
			mUiDragonsStable = (UiDragonsStable)_UiCardParent;
		}
		if (UiDragonsStable.pCurrentMode == UiDragonsStable.Mode.DragonNestAllocation || UiDragonsStable.pCurrentMode == UiDragonsStable.Mode.StableSelection)
		{
			if (byPetID != null && byPetID.ID == mUiDragonsStable.pUiDragonsListCard.pAllocationStableID && byPetID.GetNestByPetID(mSelectedPetID).ID == mUiDragonsStable.pUiDragonsListCard.pAllocationNestID)
			{
				flag = false;
			}
			SetButtons(selectBtn: false, visitBtn: true, flag);
		}
		else if (mUiDragonsStable.pUiDragonsListCard.pCurrentMode == UiDragonsListCard.Mode.ForceDragonSelection && byPetID != null)
		{
			SetButtons(selectBtn: true, visitBtn: false, moveInBtn: false);
		}
		else
		{
			if (byPetID != null)
			{
				flag = false;
			}
			SetButtons(!flag, visitBtn: true, flag);
		}
	}

	public void SetButtons(bool selectBtn, bool visitBtn, bool moveInBtn)
	{
		mInfoCardItem.SetButtons(selectBtn, visitBtn, moveInBtn);
	}

	public void OnDragonMoveIn(int petID)
	{
		if (UiDragonsStable.pCurrentMode == UiDragonsStable.Mode.DragonSelection)
		{
			mUiDragonsStable.pUiStablesInfoCard.pSelectedPetID = petID;
			mUiDragonsStable.pUiStablesListCard.pCurrentMode = UiStablesListCard.Mode.NestAllocation;
			OpenStablesList();
		}
		else if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnDragonMoveIn", petID);
		}
		else
		{
			mUiDragonsStable.pUiStablesInfoCard.AssignDragonToNest(petID);
		}
	}

	private void OpenStablesList()
	{
		Transition childTrans = new Transition(Transition.Type.SlideOut, Transition.Direction.Left, Transition.ZOrder.Top);
		Transition parentTrans = new Transition(Transition.Type.PushParents, Transition.Direction.Left, Transition.ZOrder.Top);
		mUiDragonsStable.pUiStablesListCard.PushCard(this, parentTrans, childTrans);
		mUiDragonsStable.pUiStablesListCard.RefreshUI();
		mUiDragonsStable.pUiStablesListCard.SetVisibility(inVisible: true);
		mUiDragonsStable.SetMessage(mUiDragonsStable.pUiStablesListCard, mUiDragonsStable._SelectNestText);
	}

	public void OnSelectDragonFinish(int petID)
	{
		if (mUiDragonsStable.pUiDragonsListCard.GetVisibility())
		{
			mUiDragonsStable.pUiDragonsListCard.SetDragonSelected(petID);
		}
		else
		{
			StableManager.RefreshActivePet();
		}
	}

	public void OnSelectDragonFailed(int petID)
	{
		UtDebug.Log("Failed selecting the pet");
	}

	public void OnVisitDragon(int petID)
	{
		mUiDragonsStable.Exit();
	}

	public void OnInteractivity(bool Disable)
	{
		mUiDragonsStable.Interactivity(Disable);
	}

	public void OnStableQuest()
	{
		base.OnExitClicked();
	}

	public void OnIAPStoreClosed()
	{
		if (StableManager.pInstance != null)
		{
			StableManager.pInstance.RefreshMembership();
		}
		mUiDragonsStable.pUiDragonsListCard.RefreshUI();
	}

	public override void ChildTransitionDone()
	{
	}

	public override void ParentTransitionDone()
	{
	}

	private void OnSelectDragonStart(int petID)
	{
		mUiDragonsStable.SelectPet(petID);
	}
}
