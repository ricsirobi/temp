using System;
using UnityEngine;

public class ObClickableNest : ObClickable
{
	public ContextSensitiveState[] _NestFullMenus;

	public ContextSensitiveState[] _NestEmptyMenus;

	public ContextSensitiveState[] _PetLockedMenus;

	[NonSerialized]
	public SanctuaryPet pCurrPet;

	[NonSerialized]
	public int pNestID;

	public GameObject _NonMemberSign;

	public LocaleString _BecomeMemberText = new LocaleString("To Interact with this pet you have to be a member.Do you want to Become a member?");

	public LocaleString _BecomeMemberTitleText = new LocaleString("MemberShip Expired ");

	public TextMesh _BusyTextMesh;

	public GameObject _BusyStableQuestSign;

	public LocaleString _DragonBusyStableQuestText = new LocaleString("Your Dragon is out for quests");

	private bool mIsPetLocked;

	private int mPetID;

	private UiDragonsStable mUiStable;

	private bool mOpenDragonSelection;

	private bool mIsSanctuaryCurrentPet;

	private bool mIsMoveInMode;

	private NestCSM mCSMInstance;

	public Action pActionOnClose;

	private void Start()
	{
		mCSMInstance = GetComponent<NestCSM>();
		if (_NonMemberSign != null)
		{
			_NonMemberSign.SetActive(value: false);
		}
		_BusyStableQuestSign.SetActive(value: false);
		_BusyTextMesh.text = _DragonBusyStableQuestText.GetLocalizedString();
	}

	public override void OnActivate()
	{
		base.OnActivate();
		_ActivateObject = base.gameObject;
		if (_ActivateObject != null)
		{
			if (mIsPetLocked)
			{
				_ActivateObject.GetComponent<NestCSM>()._Menus = _PetLockedMenus;
			}
			else if (HasDragon())
			{
				_ActivateObject.GetComponent<NestCSM>()._Menus = _NestFullMenus;
			}
			else
			{
				_ActivateObject.GetComponent<NestCSM>()._Menus = _NestEmptyMenus;
			}
			if (mUiStable == null && HasDragon())
			{
				UiDragonsStable.OpenDragonInfoUI(base.gameObject, mPetID, setExclusive: false);
			}
		}
	}

	public void RemovePet()
	{
		if (pCurrPet != null)
		{
			pCurrPet.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(pCurrPet.gameObject);
		}
		mIsSanctuaryCurrentPet = false;
		pCurrPet = null;
	}

	public void ReleasePet()
	{
		pCurrPet = null;
	}

	public bool HasDragon()
	{
		if (StableManager.pCurrentStableData != null && StableManager.pCurrentStableData.NestList != null && StableManager.pCurrentStableData.NestList.Count > 0)
		{
			return StableManager.pCurrentStableData.NestList.Find((NestData nd) => nd.ID == pNestID).PetID != 0;
		}
		return false;
	}

	public void OnDragonMoveIn(int petId)
	{
		if (petId != mPetID || !(pCurrPet != null))
		{
			mPetID = petId;
			StableManager.MovePetToNest(StableManager.pCurrentStableID, pNestID, petId);
			mUiStable.pUiDragonsListCard.PopOutCard();
		}
	}

	public void CreatePet(RaisedPetData pdata)
	{
		if (pCurrPet == null || pdata.RaisedPetID != pCurrPet.pData.RaisedPetID || mIsSanctuaryCurrentPet)
		{
			RemovePet();
			SanctuaryManager.CreatePet(pdata, base.transform.parent.position, base.transform.parent.rotation, base.gameObject, "Full", applyCustomSkin: true);
		}
		else
		{
			OnPetReady(pCurrPet);
		}
		mIsSanctuaryCurrentPet = false;
	}

	public void AssignSanctuaryCurrentPet(SanctuaryPet pet)
	{
		mIsSanctuaryCurrentPet = true;
		OnPetReady(pet);
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		if (pet == null)
		{
			UtDebug.LogError("Unable to create pet in nest!", 20);
			return;
		}
		mPetID = pet.pData.RaisedPetID;
		pCurrPet = pet;
		pet.pEnablePetAnim = true;
		pet.SetAvatar(null);
		pet.SetFollowAvatar(follow: false);
		pet.PlayAnimation("IdleSit", WrapMode.Loop);
		pet.AIActor.SetState(AISanctuaryPetFSM.IDLE);
		pet.EnablePettingCollider(t: true);
		pet.transform.position = base.transform.parent.position;
		pet.transform.rotation = base.transform.parent.rotation;
		if (pCurrPet != null && SanctuaryManager.IsPetLocked(pCurrPet.pData))
		{
			mIsPetLocked = true;
		}
		else
		{
			mIsPetLocked = false;
		}
		if (_NonMemberSign != null)
		{
			_NonMemberSign.SetActive(mIsPetLocked);
		}
	}

	public void OnStableUIOpened(UiDragonsStable UiStable)
	{
		mOpenDragonSelection = false;
		mUiStable = UiStable;
		if (mIsMoveInMode)
		{
			if (pCurrPet != null)
			{
				mUiStable.pUiDragonsListCard.pCurrentMode = UiDragonsListCard.Mode.NestedDragons;
			}
			mUiStable.pUiDragonsListCard.SetNestAllocationModeInfo(StableManager.pCurrentStableID, pNestID);
			mUiStable.pUiDragonsListCard.RefreshUI();
			mIsMoveInMode = false;
		}
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
	}

	public void OnCSMClosed()
	{
		if (mUiStable != null)
		{
			mUiStable.pUiDragonsInfoCard.PopOutCard();
		}
	}

	public void OnPetMoveIn()
	{
		if (mUiStable != null && mUiStable.pUiDragonsInfoCard.GetVisibility())
		{
			mOpenDragonSelection = true;
			mUiStable.pUiDragonsInfoCard.PopOutCard();
			return;
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.EnableAllInputs(inActive: false);
		mIsMoveInMode = true;
		UiDragonsStable.OpenDragonListUI(base.gameObject, UiDragonsStable.Mode.DragonNestAllocation);
	}

	public void OnStableUIClosed()
	{
		if (mOpenDragonSelection)
		{
			mOpenDragonSelection = false;
			mIsMoveInMode = true;
			UiDragonsStable.OpenDragonListUI(base.gameObject, UiDragonsStable.Mode.DragonNestAllocation);
			return;
		}
		mCSMInstance.Close();
		if (pActionOnClose != null)
		{
			pActionOnClose();
			pActionOnClose = null;
		}
	}

	public void OnClick(GameObject obj)
	{
		if (obj.Equals(_NonMemberSign))
		{
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
		}
	}

	private void OnIAPStoreClosed()
	{
		_NonMemberSign.SetActive(!SubscriptionInfo.pIsMember);
	}
}
