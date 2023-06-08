using System;
using System.Collections.Generic;
using UnityEngine;

public class UiMysteryBox : KAUI
{
	public enum AnimState
	{
		None,
		Initialize,
		PrizeInfo,
		MoveToCenter,
		ShuffleAnim,
		ReArrange,
		UserSelection,
		AwardPrize,
		Exit
	}

	[Serializable]
	public class AnimTimes
	{
		public float _PrizeShow = 3f;

		public float _PrizeMoveToCenter = 2f;

		public float _PrizeShuffle = 2f;

		public float _PrizeBackToOrgPos = 1f;

		public float _PrizeRotate = 0.5f;

		public float _FinalPrizeMoveToCenter = 1f;

		public float _FinalPrizeScale = 1f;

		public float _ExitUI = 7f;
	}

	public AnimTimes _AnimTimes = new AnimTimes();

	public Vector3 _CardHighlightScale = new Vector3(1.5f, 1.5f, 1f);

	public LocaleString _MysteryBoxHeaderText = new LocaleString("[[ItemName]] Prize");

	public LocaleString _SelectPrizeText = new LocaleString("Select a prize now!");

	public GameObject _PrizeWinParticle;

	public AudioClip _PrizeWinMusic;

	private int mItemID = -1;

	private int mFinalPrizeItemID = -1;

	private bool mInitialized;

	private bool mUiToolbarActive = true;

	private GameObject mMsgObject;

	private Vector3 mCardPopupPosition = Vector3.zero;

	private float mAnimStateWaitTime;

	private UiMysteryBoxMenu mPrizeMenu;

	private KAWidget mCloseBtn;

	private KAWidget mCardPopUp;

	private KAWidget mTxtTitle;

	private KAWidget mTxtStatusMsg;

	private bool mUpdateDescription;

	private int mPrizeQuantity = -1;

	private List<ItemData> mMysteryPrizeItems = new List<ItemData>();

	private AnimState mAnimationState = AnimState.Initialize;

	public int pFinalPrizeItemID => mFinalPrizeItemID;

	public GameObject pMsgObject
	{
		get
		{
			return mMsgObject;
		}
		set
		{
		}
	}

	public AnimState pAnimationState => mAnimationState;

	protected override void Start()
	{
		base.Start();
		mUiToolbarActive = AvAvatar.GetUIActive();
		AvAvatar.SetUIActive(inActive: false);
		mPrizeMenu = base.transform.GetComponentInChildren<UiMysteryBoxMenu>();
		KAUI.SetExclusive(this, new Color(0.5f, 0.5f, 0.5f, 0.9f));
		mCloseBtn = FindItem("BtnClose");
		mCardPopUp = FindItem("CardPopUp");
		mTxtTitle = FindItem("TxtTitle");
		mTxtStatusMsg = FindItem("TxtStatusMsg");
		mCardPopupPosition = mCardPopUp.GetLocalPosition(Vector2.zero);
		mPrizeMenu._CardShufflePosition = mCardPopupPosition;
		mInitialized = true;
	}

	public void OpenMysteryBox(int itemID, CommonInventoryResponseItem[] finalPrizes, GameObject msgObject)
	{
		if (finalPrizes != null && finalPrizes.Length != 0)
		{
			mFinalPrizeItemID = finalPrizes[0].ItemID;
		}
		mMsgObject = msgObject;
		mItemID = itemID;
		mMysteryPrizeItems = null;
		mUpdateDescription = false;
	}

	public void OpenMysteryBox(List<PrizeItemResponse> finalPrize, GameObject msgObject, bool updateDescription = false, int quantity = -1)
	{
		mFinalPrizeItemID = finalPrize[0].PrizeItemID;
		mMsgObject = msgObject;
		mItemID = finalPrize[0].ItemID;
		mMysteryPrizeItems = finalPrize[0].MysteryPrizeItems;
		mUpdateDescription = updateDescription;
		mPrizeQuantity = quantity;
	}

	protected override void Update()
	{
		if (mInitialized && mItemID > 0)
		{
			mInitialized = false;
			PopulatePrizes();
		}
		UpdateAnimState();
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mCloseBtn)
		{
			Exit();
		}
	}

	private void UpdateAnimState()
	{
		mAnimStateWaitTime -= Time.deltaTime;
		switch (mAnimationState)
		{
		case AnimState.Initialize:
			if (mPrizeMenu.AllItemsReady())
			{
				SwitchAnimState(AnimState.PrizeInfo);
			}
			break;
		case AnimState.PrizeInfo:
			if (mAnimStateWaitTime <= 0f)
			{
				SwitchAnimState(AnimState.MoveToCenter);
			}
			break;
		case AnimState.MoveToCenter:
			if (mAnimStateWaitTime <= 0f)
			{
				SwitchAnimState(AnimState.ShuffleAnim);
			}
			break;
		case AnimState.ShuffleAnim:
			if (mAnimStateWaitTime <= 0f)
			{
				SwitchAnimState(AnimState.ReArrange);
			}
			break;
		case AnimState.ReArrange:
			if (mAnimStateWaitTime <= 0f)
			{
				SwitchAnimState(AnimState.UserSelection);
			}
			break;
		case AnimState.Exit:
			if (mAnimStateWaitTime <= 0f)
			{
				Exit();
			}
			break;
		case AnimState.UserSelection:
		case AnimState.AwardPrize:
			break;
		}
	}

	private void SwitchAnimState(AnimState state)
	{
		switch (state)
		{
		case AnimState.PrizeInfo:
			mAnimStateWaitTime = _AnimTimes._PrizeShow;
			break;
		case AnimState.MoveToCenter:
			mPrizeMenu.pIsAnimating = true;
			mAnimStateWaitTime = _AnimTimes._PrizeMoveToCenter;
			mPrizeMenu.MovePrizesToCenter();
			break;
		case AnimState.ShuffleAnim:
			mPrizeMenu.pIsAnimating = true;
			mAnimStateWaitTime = _AnimTimes._PrizeShuffle;
			break;
		case AnimState.ReArrange:
			mPrizeMenu.pIsAnimating = true;
			mAnimStateWaitTime = _AnimTimes._PrizeBackToOrgPos;
			mPrizeMenu.RearrangeForChoose();
			break;
		case AnimState.UserSelection:
			mTxtStatusMsg.SetVisibility(inVisible: true);
			mTxtStatusMsg.SetText(_SelectPrizeText.GetLocalizedString());
			break;
		case AnimState.AwardPrize:
			mTxtStatusMsg.SetVisibility(inVisible: false);
			break;
		case AnimState.Exit:
			mCloseBtn.SetVisibility(inVisible: true);
			mAnimStateWaitTime = _AnimTimes._ExitUI;
			break;
		}
		mAnimationState = state;
	}

	public void PopulatePrizes()
	{
		if (mPrizeMenu == null)
		{
			mPrizeMenu = base.transform.GetComponentInChildren<UiMysteryBoxMenu>();
		}
		ItemData.Load(mItemID, OnMysteryBoxLoadItemDataReady, mMysteryPrizeItems);
	}

	private void OnMysteryBoxLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		mTxtTitle.SetText(_MysteryBoxHeaderText.GetLocalizedString().Replace("[[ItemName]]", dataItem.ItemName));
		mPrizeMenu.PopulatePrizes(itemID, dataItem, inUserData);
	}

	public void PrizeSelected(KAWidget prizeItem)
	{
		mPrizeMenu.SetState(KAUIState.NOT_INTERACTIVE);
		SetState(KAUIState.NOT_INTERACTIVE);
		SwitchAnimState(AnimState.AwardPrize);
		Vector3 position = prizeItem.transform.position;
		Vector3 vector = mCardPopUp.GetLocalPosition(position);
		mCardPopUp.SetPosition(vector.x, vector.y);
		mCardPopUp.SetScale(prizeItem.GetScale());
		mCardPopUp.SetVisibility(inVisible: true);
		mCardPopUp.MoveTo(new Vector2(mCardPopupPosition.x, mCardPopupPosition.y), _AnimTimes._FinalPrizeMoveToCenter);
		mCardPopUp.OnMoveToDone += CardReachedCenter;
	}

	public void CardScaledUp()
	{
		TweenScale tweenScale = TweenScale.Begin(mCardPopUp.gameObject, _AnimTimes._PrizeRotate, new Vector3(0f, _CardHighlightScale.y, 1f));
		tweenScale.eventReceiver = base.gameObject;
		tweenScale.callWhenFinished = "CardRotatedToBackHalf";
	}

	public void CardReachedCenter(UnityEngine.Object widget)
	{
		TweenScale tweenScale = TweenScale.Begin(mCardPopUp.gameObject, _AnimTimes._FinalPrizeScale, _CardHighlightScale);
		tweenScale.eventReceiver = base.gameObject;
		tweenScale.callWhenFinished = "GetFinalPrize";
	}

	private void GetFinalPrize()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		ItemData.Load(mFinalPrizeItemID, OnLoadItemDataReady, null);
	}

	private void OnLoadItemDataReady(int itemID, ItemData item, object inUserData)
	{
		string text = item.ItemName;
		if (mUpdateDescription && mPrizeQuantity > 0)
		{
			text = mPrizeQuantity + " " + item.ItemName;
		}
		mCardPopUp.FindChildItem("AniIconImage").SetText(text);
		KAWidget kAWidget = mCardPopUp.FindChildItem("Icon");
		CoBundleItemData coBundleItemData = new CoBundleItemData(item.IconName, null);
		kAWidget.SetUserData(coBundleItemData);
		coBundleItemData.LoadResource();
		CardScaledUp();
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public void CardRotatedToBackHalf()
	{
		KAWidget kAWidget = mCardPopUp;
		kAWidget.FindChildItem("CardInfo").FindChildItem("CardFront").SetVisibility(inVisible: false);
		kAWidget.FindChildItem("CardInfo").FindChildItem("CardBack").SetVisibility(inVisible: true);
		TweenScale tweenScale = TweenScale.Begin(mCardPopUp.gameObject, _AnimTimes._PrizeRotate, _CardHighlightScale);
		tweenScale.eventReceiver = base.gameObject;
		tweenScale.callWhenFinished = "CardRotatedToBack";
		if (_PrizeWinParticle != null)
		{
			_PrizeWinParticle.SetActive(value: true);
		}
		if ((bool)_PrizeWinMusic)
		{
			SnChannel.Play(_PrizeWinMusic, "SFX_Pool", 0, inForce: true);
		}
	}

	public void CardRotatedToBack()
	{
		SwitchAnimState(AnimState.Exit);
		SetState(KAUIState.INTERACTIVE);
	}

	public void Exit()
	{
		if (mUiToolbarActive)
		{
			AvAvatar.SetUIActive(inActive: true);
		}
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.transform.root.gameObject);
		mMsgObject.SendMessage("OnMysteryBoxClosed", SendMessageOptions.DontRequireReceiver);
	}
}
