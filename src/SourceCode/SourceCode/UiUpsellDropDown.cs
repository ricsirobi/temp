using UnityEngine;

public class UiUpsellDropDown : KAUI
{
	public float _TransitionEffectTime = 0.4f;

	private UiDragonsEndDB mParentUI;

	private string mGameType;

	private string mResult;

	private bool mInitialized;

	private bool mShown;

	private int mItemID = -1;

	private UpsellItemData mUpsellItemData;

	private KAWidget mIcoUpsellItem;

	private KAWidget mTxtUpsellName;

	private KAWidget mTxtUpsellDesc;

	private KAWidget mbtnStore;

	private Vector3 mTargetBgPos;

	private bool mSliding;

	protected override void Start()
	{
		base.Start();
		mTargetBgPos = base.transform.position;
	}

	public void Init(UiDragonsEndDB inParent, string inType, string inResult)
	{
		mParentUI = inParent;
		mGameType = inType;
		mResult = inResult;
		mInitialized = true;
		mShown = false;
		mItemID = -1;
		mIcoUpsellItem = FindItem("IcoUpsellItem");
		mTxtUpsellName = FindItem("TxtUpsellDesc");
		mTxtUpsellDesc = FindItem("TxtUpsellStore");
		mbtnStore = FindItem("StoreBtn");
		Vector3 position = base.transform.position;
		KAWidget kAWidget = FindItem("AdBoxBkg");
		if (kAWidget != null)
		{
			UITexture component = kAWidget.transform.Find("Background").gameObject.GetComponent<UITexture>();
			position.y += component.cachedTransform.localScale.y;
		}
		base.transform.position = position;
		SetVisibility(inVisible: false);
	}

	protected override void Update()
	{
		base.Update();
		if (!mShown && mInitialized && UpsellGameData.pIsReady)
		{
			mShown = true;
			if (GetUpsellData())
			{
				WsWebService.GetItemData(mItemID, ServiceEventHandler, null);
			}
		}
	}

	private bool GetUpsellData()
	{
		mUpsellItemData = UpsellGameData.GetUpsellData(mGameType, mResult);
		while (mUpsellItemData != null)
		{
			if (!mUpsellItemData.PetTypeID.HasValue || SanctuaryManager.pCurrentPetType == mUpsellItemData.PetTypeID)
			{
				if (!mUpsellItemData.CheckInventory)
				{
					PickRandomItem();
					return true;
				}
				SetItemID();
				if (mItemID != -1)
				{
					return true;
				}
				mUpsellItemData = UpsellGameData.GetNextUpsellData();
			}
			else
			{
				mUpsellItemData = UpsellGameData.GetNextUpsellData();
			}
		}
		return false;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mbtnStore)
		{
			KAUIStore.pUpdateAvatar = false;
			if (mParentUI.pMessageObject != null)
			{
				mParentUI.pMessageObject.SendMessage("OnStoreOpened", null, SendMessageOptions.DontRequireReceiver);
			}
			mParentUI.SetVisibility(Visibility: false);
			StoreLoader.Load(setDefaultMenuItem: true, mUpsellItemData.Category, mUpsellItemData.Store, base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		KAUIStore.pUpdateAvatar = true;
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			ItemData itemData = (ItemData)inObject;
			if (mTxtUpsellName != null)
			{
				mTxtUpsellName.SetText(itemData.ItemName);
			}
			if (mTxtUpsellDesc != null)
			{
				mTxtUpsellDesc.SetText(itemData.Description);
			}
			string[] array = itemData.IconName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnIconLoaded, typeof(Texture));
			SlideToTargetPosition();
		}
	}

	public void OnIconLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (mIcoUpsellItem != null)
			{
				mIcoUpsellItem.SetTexture((Texture)inObject);
				mIcoUpsellItem.SetState(KAUIState.NOT_INTERACTIVE);
				mIcoUpsellItem.SetVisibility(inVisible: true);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error !!! Icon not available");
			break;
		}
	}

	private void OnStoreClosed()
	{
		mParentUI.SetVisibility(Visibility: true);
		if (mParentUI.pMessageObject != null)
		{
			mParentUI.pMessageObject.SendMessage("OnStoreClosed", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(mUpsellItemData != null && inVisible);
	}

	public override void SetInteractive(bool interactive)
	{
		if (mSliding)
		{
			mStateBeforeSlide = ((!interactive) ? KAUIState.NOT_INTERACTIVE : KAUIState.INTERACTIVE);
		}
		else
		{
			base.SetInteractive(interactive);
		}
	}

	public override void OnSlideEnd()
	{
		mSliding = false;
		base.OnSlideEnd();
	}

	public void SetItemID()
	{
		mItemID = -1;
		for (int i = 0; i < mUpsellItemData.ItemID.Length && CommonInventoryData.pInstance.FindItem(mUpsellItemData.ItemID[i]) == null; i++)
		{
			mItemID = mUpsellItemData.ItemID[i];
		}
	}

	private void PickRandomItem()
	{
		int num = 0;
		if (mUpsellItemData.ItemID.Length > 1)
		{
			num = Random.Range(0, mUpsellItemData.ItemID.Length);
		}
		mItemID = mUpsellItemData.ItemID[num];
	}

	public void SlideToTargetPosition()
	{
		if (mParentUI.GetVisibility())
		{
			SetVisibility(inVisible: true);
			SlideTo(mTargetBgPos, _TransitionEffectTime);
			mSliding = true;
		}
	}
}
