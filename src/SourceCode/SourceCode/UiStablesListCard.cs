using System;
using System.Collections.Generic;
using UnityEngine;

public class UiStablesListCard : UiCard
{
	public enum Mode
	{
		General = 1,
		NestAllocation
	}

	[Serializable]
	public class StableImageMap
	{
		public int _StableType;

		public string _IconSprite;

		public string _BannerSprite;
	}

	private Mode mCurrentMode = Mode.General;

	private UiDragonsStable mUiDragonsStable;

	private UiStablesListCardMenu mUiStablesListCardMenu;

	private KAWidget mCurrentRoomItem;

	private int mLastSelectedStableID = -1;

	private bool mShowInfoCard;

	public List<StableImageMap> _StableIcon = new List<StableImageMap>();

	public string _StableIconForUnknown;

	public string _StableBannerForUnknown;

	public string _StableIconStore;

	public StoreLoader.Selection _StoreInfo;

	private StableData mHeaderStableData;

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

	public StableData pHeaderStableData => mHeaderStableData;

	protected override void Awake()
	{
		base.Awake();
		mUiDragonsStable = (UiDragonsStable)_UiCardParent;
	}

	public string GetStableIcon(int stableType)
	{
		if (_StableIcon != null)
		{
			foreach (StableImageMap item in _StableIcon)
			{
				if (item._StableType == stableType)
				{
					return item._IconSprite;
				}
			}
		}
		return _StableIconForUnknown;
	}

	public string GetStableBanner(int stableType)
	{
		if (_StableIcon != null)
		{
			foreach (StableImageMap item in _StableIcon)
			{
				if (item._StableType == stableType)
				{
					return item._BannerSprite;
				}
			}
		}
		return _StableBannerForUnknown;
	}

	public override void SetVisibility(bool inVisible)
	{
		bool visibility = GetVisibility();
		base.SetVisibility(inVisible);
		if (!visibility && inVisible)
		{
			InitializeUI();
		}
	}

	private void InitializeUI()
	{
		StableData.UpdateInfo();
		RefreshUI();
	}

	public void RefreshUI()
	{
		mLastSelectedStableID = -1;
		mUiStablesListCardMenu = GetComponentInChildren<UiStablesListCardMenu>();
		mCurrentRoomItem = FindItem("CurrentRoom");
		mHeaderStableData = StableManager.pCurrentStableData;
		if (mHeaderStableData == null)
		{
			mHeaderStableData = StableData.GetByID(0);
		}
		if (mHeaderStableData != null)
		{
			mCurrentRoomItem.SetVisibility(inVisible: true);
			SetSelectedStableItem(mHeaderStableData);
		}
		else
		{
			mCurrentRoomItem.SetVisibility(inVisible: false);
		}
		mUiStablesListCardMenu.LoadStablesList();
		if (mCurrentMode == Mode.NestAllocation)
		{
			mUiDragonsStable.SetMessage(mUiDragonsStable.pUiStablesInfoCard, mUiDragonsStable._SelectNestText);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCurrentRoomItem && mHeaderStableData != null)
		{
			SelectRoom(mHeaderStableData.ID);
		}
	}

	public void RefreshStableInfoTips()
	{
		if (mCurrentMode == Mode.NestAllocation)
		{
			mUiDragonsStable.SetMessage(mUiDragonsStable.pUiStablesInfoCard, mUiDragonsStable._SelectNestText);
		}
		else if (GetState() == KAUIState.INTERACTIVE)
		{
			mUiDragonsStable.SetMessage(this, mUiDragonsStable._SelectStableText);
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

	private void SetSelectedStableItem(StableData sData)
	{
		mCurrentRoomItem.FindChildItem("TxtRoomName").SetText(sData.pLocaleName);
	}

	public void OpenStore()
	{
		mUiDragonsStable.Exit();
		if (_StoreInfo != null)
		{
			StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, base.gameObject);
		}
	}

	public void SelectRoom(int stableID)
	{
		if (mLastSelectedStableID == stableID)
		{
			return;
		}
		mLastSelectedStableID = stableID;
		KAWidget kAWidget = mCurrentRoomItem.FindChildItem("SelectedFrame");
		if ((bool)kAWidget)
		{
			if (stableID == mHeaderStableData.ID)
			{
				kAWidget.SetVisibility(inVisible: true);
				mUiStablesListCardMenu.ResetSelection();
			}
			else
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		if (!mUiDragonsStable.pUiStablesInfoCard.GetVisibility())
		{
			Transition childTrans = new Transition(Transition.Type.SlideOut, Transition.Direction.Right, Transition.ZOrder.Bottom);
			mUiDragonsStable.pUiStablesInfoCard.PushCard(this, null, childTrans);
			mUiDragonsStable.pUiStablesInfoCard.pSelectedStableID = stableID;
			mUiDragonsStable.pUiStablesInfoCard.RefreshUI();
		}
		else
		{
			mShowInfoCard = true;
			mUiDragonsStable.pUiStablesInfoCard.PopOutCard(releaseParentLink: false);
			mUiDragonsStable.pUiStablesInfoCard.pSelectedStableID = stableID;
		}
	}

	public override void TransitionDone()
	{
		base.TransitionDone();
		if (mHeaderStableData != null)
		{
			SelectRoom(mHeaderStableData.ID);
		}
	}

	public override void ChildReverseTransitionDone()
	{
		base.ChildReverseTransitionDone();
		if (!base.pPopOutCard && mShowInfoCard)
		{
			mShowInfoCard = false;
			mUiDragonsStable.pUiStablesInfoCard.RefreshUI();
			Transition childTrans = new Transition(Transition.Type.SlideOut, Transition.Direction.Right, Transition.ZOrder.Bottom);
			mUiDragonsStable.pUiStablesInfoCard.PushCard(this, null, childTrans);
		}
	}

	public override void ParentTransitionDone()
	{
	}
}
