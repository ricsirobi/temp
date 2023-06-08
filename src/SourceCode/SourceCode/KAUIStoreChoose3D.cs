using System;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;
using UnityEngine.AI;

public class KAUIStoreChoose3D : KAUIStoreChoose
{
	[Serializable]
	public class DragonIdle
	{
		public int _TypeID;

		public string _Animation;
	}

	public Transform _ZoomMarker;

	public float _ZoomInScale = 2f;

	public float _ZoomOutScale = 0.8f;

	public float _ZoomInSpeedMultiplier = 4f;

	public List<int> _ZoomInForItems = new List<int> { 420, 421, 18 };

	public bool _AllowZoomInZoomOut = true;

	public float _DragonUiScale = 1f;

	public float _DragonAverageZoom = 1f;

	public List<DragonIdle> _DragonIdleOverrides;

	public bool _DisablePetMoodParticles = true;

	public GameObject _3DMarker;

	public GameObject _3DAvatarMarker;

	public bool _AllowScaling;

	public Vector2 _ScreenDisplayArea;

	public Camera _Camera;

	private StorePreviewCategory mCurPreviewCategory;

	private Vector3 mPrevPosition = Vector3.zero;

	private string mPrevPartName = "";

	private CustomAvatarState mCustomAvatar;

	private GameObject mPrevAvatar;

	private GameObject mAvatarMale;

	private GameObject mAvatarFemale;

	private bool mAvatarLoaded;

	private bool mZoom;

	private bool mResetRotation;

	private bool mModified;

	private bool mPreviousFollowAvatar;

	private AvatarDataPart[] mCachedDataParts;

	private AvatarPartChanger mChanger = new AvatarPartChanger();

	private AvAvatarController mAvatarController;

	private bool mLoading3DData;

	private KAWidget mBtnPreviewMovie;

	private KAWidget mBtnPreviewMoveLt;

	private KAWidget mBtnPreviewMoveRt;

	private KAWidget mTxtPageItemCount;

	private bool mApplyingPurchaseItem;

	private ItemData mPurchasedAvatarItem;

	private CommonInventoryResponseItem[] mCommonInventoryResponseItemList;

	private int mPetTypeID = -1;

	private SanctuaryPet mCurrentPet;

	private RaisedPetAccType mSelectedDragonAccType;

	private float mScaleLength;

	private KAWidget m2DViewer;

	private KAWidget mPreviewFront;

	private KAWidget mPreviewBack;

	private KAWidget mBtnPreview;

	private bool mIsPreviewFaceFront = true;

	protected bool mLoadingData;

	protected SanctuaryPet mPet;

	protected ItemPrefabResData m3DData = new ItemPrefabResData();

	protected ItemTextureResData mTexData = new ItemTextureResData();

	protected GameObject pPreviewInstance
	{
		get
		{
			if (StorePreview.pInstance != null)
			{
				return StorePreview.pInstance.pCur3DInstance;
			}
			return null;
		}
	}

	protected override void Start()
	{
		if (StorePreview.pInstance == null)
		{
			StorePreview.Start(null, this);
		}
		else
		{
			StorePreview.pInstance.mStoreChooseUI = this;
		}
		m2DViewer = FindItem("Ui2DPreview");
		mPreviewFront = m2DViewer.FindChildItem("AniIconFront");
		mPreviewBack = m2DViewer.FindChildItem("AniInfoBehind");
		mBtnPreview = FindItem("PreviewInfoBtn");
		if (_AllowScaling)
		{
			if (_Camera == null)
			{
				_Camera = Camera.main;
			}
			Vector3 vector = _Camera.WorldToScreenPoint(_3DMarker.transform.position);
			float num = Mathf.Max(_ScreenDisplayArea.x, _ScreenDisplayArea.y);
			Vector3 a = _Camera.ScreenToWorldPoint(vector + new Vector3(0f, (0f - num) / 2f, 0f));
			Vector3 b = _Camera.ScreenToWorldPoint(vector + new Vector3(0f, num / 2f, 0f));
			mScaleLength = Vector3.Distance(a, b);
		}
		mLoadingData = false;
		mPrevPosition = AvAvatar.GetPosition();
		mLoadingData = false;
		SubstanceCustomization.Init("Dragon");
		mBtnPreviewMovie = FindItem("BtnPreviewMovie");
		mBtnPreviewMoveLt = FindItem("BtnPreviewMoveLt");
		mBtnPreviewMoveRt = FindItem("BtnPreviewMoveRt");
		mTxtPageItemCount = FindItem("CountTxt");
		mCachedDataParts = AvatarData.pInstanceInfo.GetClonedParts();
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		base.Start();
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (KAUIStore.pLocked)
		{
			return;
		}
		_StoreUI.EnableStoreMenu(inEnable: false);
		if (item == mBtnPreviewMoveLt)
		{
			MovePreviewPageLeft();
		}
		else if (item == mBtnPreviewMoveRt)
		{
			MovePreviewPageRight();
		}
		else if (item == mBtnPreviewMovie)
		{
			SetStoreStateActive(active: false);
			MovieManager.SetBackgroundColor(Color.black);
			MovieManager.Play(base.pChosenItemData._ItemData.GetAttribute("Movie", string.Empty), null, OnMoviePlayed, skipMovie: true);
		}
		else if (item == mBtnPreview && (bool)GameConfig.pInstance)
		{
			string[] array = ((UtPlatform.IsiOS() || (UtPlatform.IsAndroid() && !UtPlatform.IsAmazon())) ? GameConfig.GetKeyData("UIMembershipBenefitsNoTrial") : GameConfig.GetKeyData("UIMembershipBenefits"))?.Split('/') ?? Array.Empty<string>();
			if (array.Length >= 3)
			{
				SetStoreStateActive(active: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUIMembershipBenefitsLoaded, typeof(GameObject));
			}
		}
	}

	private void OnUIMembershipBenefitsLoaded(string inUrl, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserdata)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UnityEngine.Object.Instantiate((GameObject)inObject);
			SetStoreStateActive(active: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.ERROR:
			SetStoreStateActive(active: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void OnMoviePlayed()
	{
		SetStoreStateActive(active: true);
	}

	public void SetStoreStateActive(bool active)
	{
		if (active)
		{
			SetState(KAUIState.INTERACTIVE);
			KAUIStore.pInstance.SetState(KAUIState.INTERACTIVE);
			KAUIStore.pInstance.pMainMenu.SetState(KAUIState.INTERACTIVE);
			KAUIStore.pInstance.pCategoryMenu.SetState(KAUIState.INTERACTIVE);
			KAUIStore.pInstance.pChooseMenu.SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			SetState(KAUIState.NOT_INTERACTIVE);
			KAUIStore.pInstance.SetState(KAUIState.NOT_INTERACTIVE);
			KAUIStore.pInstance.pMainMenu.SetState(KAUIState.NOT_INTERACTIVE);
			KAUIStore.pInstance.pCategoryMenu.SetState(KAUIState.NOT_INTERACTIVE);
			KAUIStore.pInstance.pChooseMenu.SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	public void EnablePreviewButtons(bool inEnableLeft, bool inEnableRight)
	{
		if (mBtnPreviewMoveLt != null)
		{
			mBtnPreviewMoveLt.SetDisabled(!inEnableLeft);
		}
		if (mBtnPreviewMoveRt != null)
		{
			mBtnPreviewMoveRt.SetDisabled(!inEnableRight);
		}
	}

	public void SetPreviewButtonsVisible(bool visibility)
	{
		if (mBtnPreviewMoveLt != null)
		{
			mBtnPreviewMoveLt.SetVisibility(visibility);
		}
		if (mBtnPreviewMoveRt != null)
		{
			mBtnPreviewMoveRt.SetVisibility(visibility);
		}
	}

	private void ResetPreviewUI(bool inVisible, StorePreviewCategory inCategory)
	{
		switch (inCategory)
		{
		case StorePreviewCategory.Normal3D:
			if (!inVisible)
			{
				Show2DViewer(show: false);
			}
			if (UtPlatform.IsMobile())
			{
				mChosenIcon.SetVisibility(inVisible: false);
			}
			if (KAUIStoreCategory.pInstance != null)
			{
				KAUIStoreCategory.pInstance.EnableBackGround(!inVisible);
			}
			break;
		case StorePreviewCategory.Avatar:
			if (inVisible)
			{
				mPrevPartName = GetCategoryPartName();
				if (!mAvatarLoaded)
				{
					LoadAvatar();
					break;
				}
				SetZoom();
				AvAvatar.pObject.SetActive(value: true);
			}
			else if (mAvatarLoaded)
			{
				AvatarData.RestoreDefault();
				mCustomAvatar.FromAvatarData(AvatarData.pInstance);
				mCustomAvatar.UpdateShaders(AvAvatar.pObject);
				mCustomAvatar.RestoreAll();
				mCustomAvatar.mIsDirty = true;
				AvAvatar.pObject.SetActive(value: false);
				mPrevPartName = "";
			}
			break;
		case StorePreviewCategory.RaisedPet:
			if (mCurrentPet != mPet && mPet != null)
			{
				DestroyAccessoryObj(mPet);
				UnityEngine.Object.Destroy(mPet.gameObject);
				mPet = null;
			}
			if (inVisible)
			{
				if (mCurrentPet == null)
				{
					LoadDragon();
					break;
				}
				mCurrentPet.UpdateRaisedPetAccessories(SanctuaryManager.pCurPetInstance.pData);
				mCurrentPet.gameObject.SetActive(value: true);
				DragonIdle dragonIdle = null;
				if (_DragonIdleOverrides != null)
				{
					dragonIdle = _DragonIdleOverrides.Find((DragonIdle di) => di._TypeID == SanctuaryManager.pCurPetData.PetTypeID);
				}
				if (dragonIdle != null)
				{
					mCurrentPet.animation.Play(dragonIdle._Animation);
				}
				else if (!string.IsNullOrEmpty(mCurrentPet._AnimNameIdle))
				{
					mCurrentPet.animation.Play(mCurrentPet._AnimNameIdle);
				}
			}
			else if (mCurrentPet != null)
			{
				mCurrentPet.gameObject.SetActive(value: false);
			}
			break;
		}
	}

	public override void ShowAD(bool t)
	{
		base.ShowAD(t);
		if (!UtPlatform.IsMobile())
		{
			mChosenIcon.SetVisibility(inVisible: false);
		}
	}

	private void Show2DViewer(bool show)
	{
		if (mPreviewFront != null)
		{
			mPreviewFront.SetVisibility(show);
		}
	}

	public override void ProcessChooseSelection(KAWidget item)
	{
		base.ProcessChooseSelection(item);
		ShowPreviewInfoBtn(show: false);
		if (item == null)
		{
			SetPreviewButtonsVisible(visibility: false);
			Show2DViewer(show: false);
			_StoreUI.pChooseMenu._IAPGemsStorePreview.SetVisibility(inVisible: false);
			if (mBtnPreviewMovie != null)
			{
				mBtnPreviewMovie.SetVisibility(inVisible: false);
			}
			return;
		}
		if (base.pChosenItemData != null)
		{
			mCurPreviewCategory = KAUIStore.GetPreviewCategory(base.pChosenItemData._ItemData);
		}
		if (mCurPreviewCategory == StorePreviewCategory.Normal3D)
		{
			StoreWidgetUserData storeWidgetUserData = (StoreWidgetUserData)item.GetUserData();
			if (_StoreUI.pCategoryMenu.pType == KAStoreMenuItemData.StoreType.IAPStore && storeWidgetUserData.PurchaseStoreType == KAStoreMenuItemData.StoreType.IAPStore)
			{
				IAPItemWidgetUserData iAPItemWidgetUserData = (IAPItemWidgetUserData)storeWidgetUserData;
				int num = 0;
				if (!string.IsNullOrEmpty(iAPItemWidgetUserData._IAPItemData.SubscriptionOffer) && (!SubscriptionInfo.pIsMember || SubscriptionInfo.pIsTrialMember))
				{
					Show2DViewer(show: false);
					_StoreUI.ShowPreviewBuyButton(visible: false);
					_StoreUI.ShowAdsButton(visible: false);
					_StoreUI.pChooseMenu._IAPGemsStorePreview.Preview(iAPItemWidgetUserData);
					_StoreUI.ShowFullPreviewButton(visible: false);
				}
				else if (mPreviewFront != null && iAPItemWidgetUserData._ItemTextureData._Texture != null)
				{
					Show2DViewer(show: true);
					mPreviewFront.ResetWidget();
					mPreviewBack.SetVisibility(inVisible: false);
					_StoreUI.pChooseMenu._IAPGemsStorePreview.SetVisibility(inVisible: false);
					if (iAPItemWidgetUserData._PreviewAssetsData != null && iAPItemWidgetUserData._PreviewAssetsData.Count > num && !string.IsNullOrEmpty(iAPItemWidgetUserData._PreviewAssetsData[num].Name))
					{
						string[] array = iAPItemWidgetUserData._PreviewAssetsData[num].Name.Split('/');
						if (array != null && array.Length > 2)
						{
							StorePreview.ScalePreview(mPreviewFront, iAPItemWidgetUserData._PreviewAssetsData[num].Size);
							mPreviewFront.SetVisibility(inVisible: false);
							mPreviewFront.SetTextureFromBundle(array[0] + "/" + array[1], array[2], null, StorePreview.OnPreviewLoaded);
						}
					}
					else
					{
						CoBundleItemData coBundleItemData = new CoBundleItemData(iAPItemWidgetUserData._IAPItemData.IconName, null);
						coBundleItemData._Item = mPreviewFront;
						coBundleItemData.LoadResource();
					}
				}
				if (!mPreviewBack)
				{
					return;
				}
				num++;
				if (iAPItemWidgetUserData._PreviewAssetsData != null && iAPItemWidgetUserData._PreviewAssetsData.Count > num && iAPItemWidgetUserData._PreviewAssetsData[num] != null && !string.IsNullOrEmpty(iAPItemWidgetUserData._PreviewAssetsData[num].Name))
				{
					mIsPreviewFaceFront = true;
					string[] array2 = iAPItemWidgetUserData._PreviewAssetsData[num].Name.Split('/');
					if (array2 != null && array2.Length > 2)
					{
						StorePreview.ScalePreview(mPreviewBack, iAPItemWidgetUserData._PreviewAssetsData[num].Size);
						mPreviewBack.SetTextureFromBundle(array2[0] + "/" + array2[1], array2[2]);
					}
				}
				if (_StoreUI.pCategory == "Membership")
				{
					ShowPreviewInfoBtn(show: true);
				}
				return;
			}
			Show2DViewer(show: false);
			if (item != null && base.pChosenItemData != null)
			{
				mChosenIcon.SetVisibility(inVisible: true);
			}
		}
		if (StorePreview.pInstance != null)
		{
			StorePreview.pInstance.Reset();
		}
		if (base.pChosenItemData == null)
		{
			return;
		}
		if (base.pChosenItemData._ItemData.IsPrizeItem())
		{
			List<int> prizesSorted = base.pChosenItemData._ItemData.GetPrizesSorted();
			if (prizesSorted != null && prizesSorted.Count > 0)
			{
				StorePreview.pInstance.ShowPreview(base.pChosenItemData._ItemData.ItemID, prizesSorted.ToArray(), _3DMarker.transform, mPreviewFront, _AllowScaling, mScaleLength);
			}
		}
		else if (base.pChosenItemData._ItemData.IsBundleItem())
		{
			List<int> bundledItems = base.pChosenItemData._ItemData.GetBundledItems();
			if (bundledItems != null && bundledItems.Count > 0)
			{
				StorePreview.pInstance.ShowPreview(base.pChosenItemData._ItemData.ItemID, bundledItems.ToArray(), _3DMarker.transform, mPreviewFront, _AllowScaling, mScaleLength, base.pChosenItemData._ItemData.Relationship);
			}
		}
		else
		{
			StorePreview.pInstance.ShowPreview(base.pChosenItemData._ItemData.ItemID, base.pChosenItemData._ItemData.ItemID, _3DMarker.transform, mPreviewFront, _AllowScaling, mScaleLength);
		}
		if (mBtnPreviewMovie != null)
		{
			mBtnPreviewMovie.SetVisibility(base.pChosenItemData._ItemData.HasAttribute("Movie"));
		}
	}

	private void ShowPreviewInfoBtn(bool show)
	{
		if (mBtnPreview != null)
		{
			mBtnPreview.SetVisibility(show);
			mBtnPreview.GetComponent<BoxCollider>().enabled = show;
		}
	}

	public override void RotatePreview(float rotationDelta)
	{
		if (mCurPreviewCategory == StorePreviewCategory.Normal3D)
		{
			if (pPreviewInstance != null)
			{
				float num = rotationDelta * Time.deltaTime;
				if (num != 0f)
				{
					pPreviewInstance.transform.Rotate(0f, num, 0f, Space.World);
				}
			}
		}
		else if (mCurPreviewCategory == StorePreviewCategory.Avatar)
		{
			if (mPrevAvatar != null)
			{
				float num2 = rotationDelta * Time.deltaTime;
				if (num2 != 0f)
				{
					AvAvatar.mTransform.Rotate(0f, num2, 0f);
				}
			}
		}
		else if (mCurPreviewCategory == StorePreviewCategory.RaisedPet && mPet != null)
		{
			float num3 = rotationDelta * Time.deltaTime;
			if (num3 != 0f)
			{
				mPet.gameObject.transform.Rotate(0f, num3, 0f);
			}
		}
	}

	protected override void Update()
	{
		if (mAvatarController != null && mAvatarController.pAvatarCustomization != null)
		{
			mAvatarController.pAvatarCustomization.DoUpdate();
		}
		if (mCurPreviewCategory == StorePreviewCategory.Normal3D)
		{
			base.Update();
		}
		else if (mCurPreviewCategory == StorePreviewCategory.Avatar)
		{
			base.Update();
			if (AvatarData.pIsReady && mAvatarLoaded && _AllowZoomInZoomOut)
			{
				ZoomInAndOut();
			}
			if (mCustomAvatar == null || !mCustomAvatar.mIsDirty)
			{
				return;
			}
			if (AvatarData.GetGender() == Gender.Male)
			{
				if (mAvatarMale != null)
				{
					mCustomAvatar.UpdateShaders(mAvatarMale);
					mCustomAvatar.mIsDirty = false;
				}
			}
			else if (mAvatarFemale != null)
			{
				mCustomAvatar.UpdateShaders(mAvatarFemale);
				mCustomAvatar.mIsDirty = false;
			}
		}
		else if (mCurPreviewCategory == StorePreviewCategory.RaisedPet)
		{
			base.Update();
			if (mPet != null && mSelectedDragonAccType != 0 && mLoading3DData && m3DData.IsDataLoaded() && (string.IsNullOrEmpty(mTexData._ResFullName) || mTexData.IsDataLoaded()))
			{
				mLoading3DData = false;
				OnDataReady();
			}
		}
	}

	public override void OnStoreSelected(KAStoreInfo inStoreInfo)
	{
		base.OnStoreSelected(inStoreInfo);
		if (inStoreInfo != null)
		{
			OnCategorySelected((!string.IsNullOrEmpty(KAUIStore._EnterCategoryName)) ? inStoreInfo.GetCategory(KAUIStore._EnterCategoryName) : inStoreInfo.GetDefaultCategory());
		}
	}

	protected virtual void OnCategorySelected(KAStoreMenuItemData pd)
	{
		if (pd == null)
		{
			return;
		}
		switch (pd._PreviewMode)
		{
		case StorePreviewCategory.Normal3D:
			Show2DViewer(show: false);
			break;
		case StorePreviewCategory.Avatar:
			if (!string.IsNullOrEmpty(mPrevPartName))
			{
				AvatarData.RestoreDefault();
				mCustomAvatar.FromAvatarData(AvatarData.pInstance);
				mCustomAvatar.UpdateShaders(AvAvatar.pObject);
				mCustomAvatar.RestoreAll();
				mCustomAvatar.mIsDirty = true;
				mPrevPartName = GetCategoryPartName();
			}
			break;
		}
		if (_StoreUI != null)
		{
			_StoreUI.ShowPreviewBuyButton(visible: false);
			_StoreUI.ShowPreviewRotateButtons(visible: false);
			_StoreUI.ShowFullPreviewButton(visible: false);
			_StoreUI.ShowAdsButton(visible: false);
			if (_StoreUI.pChooseUI != null)
			{
				_StoreUI.pChooseUI.ResetSelectedItemData();
			}
		}
		if (mTxtPageItemCount != null)
		{
			mTxtPageItemCount.SetVisibility(inVisible: false);
		}
		SetPreviewButtonsVisible(visibility: false);
		_StoreUI.pChooseMenu._IAPGemsStorePreview.SetVisibility(inVisible: false);
		if (mBtnPreviewMovie != null)
		{
			mBtnPreviewMovie.SetVisibility(inVisible: false);
		}
		if (StorePreview.pInstance != null)
		{
			StorePreview.pInstance.Reset();
		}
		PreviewItemReady(pd._PreviewMode, null);
	}

	public override void MovePreviewPageLeft()
	{
		base.MovePreviewPageLeft();
		StorePreview.pInstance.GotoPrevPage();
		EnablePreviewButtons(StorePreview.pInstance.pEnableLeftButton, StorePreview.pInstance.pEnableRightButton);
		_StoreUI.ShowPreviewBuyButton(visible: true);
	}

	public override void MovePreviewPageRight()
	{
		base.MovePreviewPageRight();
		StorePreview.pInstance.GotoNextPage();
		EnablePreviewButtons(StorePreview.pInstance.pEnableLeftButton, StorePreview.pInstance.pEnableRightButton);
		_StoreUI.ShowPreviewBuyButton(visible: true);
	}

	public override void OnSyncUIClosed(bool isPurchaseSuccess)
	{
		base.OnSyncUIClosed(isPurchaseSuccess);
		if (mApplyingPurchaseItem)
		{
			KAUIStore.LockStore();
		}
		if (mCommonInventoryResponseItemList != null && mCommonInventoryResponseItemList.Length != 0 && mPurchasedAvatarItem != null && mPurchasedAvatarItem.HasCategory(657))
		{
			KAUIStore.pInstance.HideStoreUIs(hide: true);
			KAUIStore.pInstance._CustomizationPending = true;
			UiAvatarItemCustomization.Init(mCommonInventoryResponseItemList, null, OnCloseCustomizeItem, multiItemCustomizationUI: false);
			ShowStoreAvatar(show: false);
		}
	}

	private void PurchaseItemDataListCallback(PreviewItemDataList inList, int inParentItemID, bool inSuccess)
	{
		if (!inSuccess || inList == null || inList.pList == null || inList.pList.Count == 0)
		{
			return;
		}
		ResetPreview();
		mApplyingPurchaseItem = false;
		if (KAUIStore.pLocked)
		{
			KAUIStore.UnlockStore();
		}
		bool flag = false;
		foreach (PreviewItemData p in inList.pList)
		{
			if (p != null && p.pItemData != null)
			{
				OnPurchaseItem(inList.pList.Count > 1, p.pItemData);
				if (p.pItemData != null && KAUIStore.pInstance.pStatCompareMenu.ShowStatsOnCategory(p.pItemData))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			flag = false;
			KAUIStore.pInstance.pStatCompareMenu.RemoveStatPreview();
			KAUIStore.pInstance.pStatCompareMenu.UpdateStatsCompareData(-1, inList.pList);
		}
	}

	public virtual void OnPurchaseItem(bool isBundle, ItemData itemData)
	{
		switch (KAUIStore.GetPreviewCategory(itemData))
		{
		case StorePreviewCategory.Avatar:
		{
			if (!mAvatarLoaded)
			{
				LoadAvatar();
				AvAvatar.pObject.SetActive(value: false);
			}
			AvatarData.RestorePartData();
			mPurchasedAvatarItem = itemData;
			string itemPartType = AvatarData.GetItemPartType(itemData);
			mChanger._PrtTypeName = itemPartType;
			mModified = true;
			ApplyItem(itemData, itemPartType, inUpdateInventory: true);
			if (mCustomAvatar != null)
			{
				mCustomAvatar.CacheState();
			}
			mChanger._SaveDefault = false;
			mChanger.ApplySelection(AvatarData.pInstanceInfo, itemData.AssetName, itemData);
			AvatarData.RestoreCurrentPartCheckBundle(mChanger._PrtTypeName);
			if (mCustomAvatar != null)
			{
				mCustomAvatar.ToAvatarData(AvatarData.pInstanceInfo);
			}
			break;
		}
		case StorePreviewCategory.RaisedPet:
		{
			int attribute = itemData.GetAttribute("PetTypeID", -1);
			RaisedPetAccType accessoryType = RaisedPetData.GetAccessoryType(itemData);
			if (attribute <= 0 || (SanctuaryManager.pCurPetData != null && SanctuaryManager.pCurPetData.PetTypeID == attribute && accessoryType != 0 && SanctuaryManager.pCurPetData.pStage != RaisedPetStage.BABY))
			{
				string tex = "";
				if (itemData.Texture != null)
				{
					tex = itemData.Texture[0].TextureName;
				}
				int i;
				for (i = 0; i < mCommonInventoryResponseItemList.Length && mCommonInventoryResponseItemList[i].ItemID != itemData.ItemID; i++)
				{
				}
				SanctuaryManager.pCurPetInstance.pData.SetAccessory(accessoryType, itemData.AssetName, tex, itemData.ItemID, mCommonInventoryResponseItemList[i].CommonInventoryID);
				SanctuaryManager.pCurPetInstance.pData.SaveData();
				SanctuaryManager.pCurPetInstance.UpdateData(SanctuaryManager.pCurPetInstance.pData, noHat: false);
				SanctuaryManager.pPendingMMOPetCheck = true;
				SanctuaryManager.pInstance.TakePicture(SanctuaryManager.pCurPetInstance.gameObject);
				if (SanctuaryManager.pInstance.pPetMeter != null)
				{
					SanctuaryManager.pInstance.pPetMeter.RefreshAll();
				}
			}
			break;
		}
		}
	}

	public override void PurchaseSuccessful(CommonInventoryResponseItem[] ret)
	{
		mCommonInventoryResponseItemList = ret;
		if (base.pChosenItemData == null || base.pChosenItemData._ItemData == null)
		{
			return;
		}
		List<int> list = null;
		if (base.pChosenItemData._ItemData.IsBundleItem())
		{
			list = base.pChosenItemData._ItemData.GetBundledItems();
		}
		else
		{
			list = new List<int>();
			list.Add(base.pChosenItemData._ItemData.ItemID);
		}
		if (list == null || list.Count == 0)
		{
			return;
		}
		PreviewItemDataList previewItemDataList = null;
		foreach (int item in list)
		{
			if (item > 0)
			{
				if (previewItemDataList == null)
				{
					previewItemDataList = new PreviewItemDataList(0);
				}
				previewItemDataList.AddItem(item);
			}
		}
		if (previewItemDataList != null)
		{
			mApplyingPurchaseItem = true;
			previewItemDataList.LoadItemDataList(PurchaseItemDataListCallback);
		}
	}

	private void OnStoreExit(bool inCheckProcessing)
	{
		ResetPreview();
		if (mAvatarLoaded)
		{
			_StoreUI._ProcessingClose = true;
			if (mPurchasedAvatarItem == null || !mPurchasedAvatarItem.HasCategory(228))
			{
				AvatarData.RestorePartData();
				ProcessStoreExit();
			}
			else
			{
				mAvatarController.EquipFlightSuit(mPurchasedAvatarItem, OnAllItemsDownloaded);
			}
		}
		if (mPet != null)
		{
			DestroyAccessoryObj(mPet);
			UnityEngine.Object.Destroy(mPet.gameObject);
		}
		if (mCurrentPet != null)
		{
			DestroyAccessoryObj(mCurrentPet);
			UnityEngine.Object.Destroy(mCurrentPet.gameObject);
		}
	}

	private void ProcessStoreExit()
	{
		mCustomAvatar.RestoreAll();
		AvatarData.SetDontDestroyOnBundles(inDontDestroy: true);
		if (mModified)
		{
			SaveAvatarData();
			return;
		}
		AvatarData.pInstanceInfo.mInstance.Part = mCachedDataParts;
		SwapAvatars();
		_StoreUI._ProcessingClose = false;
	}

	public void OnAllItemsDownloaded()
	{
		mAvatarController.pAvatarCustomization.pCustomAvatar.UpdateShaders(AvAvatar.pObject);
		mAvatarController.pAvatarCustomization.SaveCustomAvatar();
		ProcessStoreExit();
	}

	public string GetCategoryPartName()
	{
		string result = string.Empty;
		if (base.pChosenItemData != null)
		{
			result = AvatarData.GetItemPartType(base.pChosenItemData._ItemData);
		}
		return result;
	}

	private void LoadAvatar()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfAvatar"));
		gameObject.name = "PfAvatarStore";
		AvatarData.ApplyCurrent(gameObject);
		if (AvatarData.GetGender() == Gender.Female || AvatarData.GetGender() == Gender.Unknown)
		{
			mAvatarFemale = gameObject;
		}
		else
		{
			mAvatarMale = gameObject;
		}
		ProcessLoadedAvatar(gameObject, AvatarData.GetGender() == Gender.Male);
		mCustomAvatar = new CustomAvatarState();
		mCustomAvatar.FromAvatarData(AvatarData.pInstance);
		mCustomAvatar.UpdateShaders(gameObject);
		CustomAvatarState.mCurrentInstance = mCustomAvatar;
		Transform transform = UtUtilities.FindChildTransform(gameObject, "DisplayName");
		if (transform != null)
		{
			GameObject gameObject2 = transform.gameObject;
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: false);
			}
		}
	}

	public void SetZoom()
	{
		mZoom = false;
		mResetRotation = false;
		if (base.pChosenItemData == null)
		{
			return;
		}
		foreach (int zoomInForItem in _ZoomInForItems)
		{
			if (base.pChosenItemData._ItemData.HasCategory(zoomInForItem))
			{
				mZoom = true;
				mResetRotation = true;
				break;
			}
		}
	}

	protected void LoadDragon()
	{
		if (!(SanctuaryManager.pCurPetInstance != null))
		{
			return;
		}
		SanctuaryManager.pCurPetInstance.gameObject.SetActive(value: true);
		GameObject gameObject = SanctuaryManager.pInstance.CloneCurrentPet(isNullyfyAvtarFollow: true, Vector3.zero, Quaternion.identity);
		if (!UtPlatform.IsMobile())
		{
			LODGroup componentInChildren = gameObject.GetComponentInChildren<LODGroup>();
			if (componentInChildren != null)
			{
				componentInChildren.ForceLOD(0);
			}
		}
		mCurrentPet = gameObject.GetComponent<SanctuaryPet>();
		SetPet(mCurrentPet);
	}

	private void ApplyItem(ItemData inItemData)
	{
		if (inItemData.GroupItem())
		{
			AvatarData.SetGroupPart(AvatarData.pInstanceInfo, inItemData.ItemID);
			ApplyItem(inItemData, AvatarData.GetItemPartType(inItemData));
			if (inItemData.Relationship != null)
			{
				ItemDataRelationship[] relationship = inItemData.Relationship;
				for (int i = 0; i < relationship.Length; i++)
				{
					ItemData.Load(relationship[i].ItemId, OnItemDataReady, null);
				}
			}
		}
		else
		{
			ApplyItems(inItemData);
		}
	}

	public void OnItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		ApplyItem(dataItem, AvatarData.GetItemPartType(dataItem));
		if (mCustomAvatar != null)
		{
			mCustomAvatar.SetInventoryId(AvatarData.GetItemPartType(dataItem), -1, saveDefault: true);
		}
	}

	private void ApplyItems(ItemData inItemData)
	{
		string itemPartType = AvatarData.GetItemPartType(inItemData);
		mChanger._PrtTypeName = itemPartType;
		if (itemPartType == AvatarData.pPartSettings.AVATAR_PART_LEGS || itemPartType == AvatarData.pPartSettings.AVATAR_PART_FEET || itemPartType == AvatarData.pPartSettings.AVATAR_PART_TOP || itemPartType == AvatarData.pPartSettings.AVATAR_PART_HAIR || itemPartType == AvatarData.pPartSettings.AVATAR_PART_HAT || itemPartType == AvatarData.pPartSettings.AVATAR_PART_FACEMASK || itemPartType == AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT || itemPartType == AvatarData.pPartSettings.AVATAR_PART_BACK || itemPartType == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND || itemPartType == AvatarData.pPartSettings.AVATAR_PART_HAND || itemPartType == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			if (AvatarData.IsDefaultSaved(itemPartType))
			{
				AvatarData.RestorePartData();
			}
			mChanger._SaveDefault = true;
			mChanger.ApplySelection(AvatarData.pInstanceInfo, inItemData.AssetName, inItemData);
			AvatarData.RestoreCurrentPartCheckBundle(itemPartType);
		}
		ApplyItem(inItemData, itemPartType);
	}

	public void ApplyItem(ItemData inItem, string partName, bool inUpdateInventory = false)
	{
		if (mCustomAvatar == null)
		{
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_SCAR)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL1, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_FACE_DECAL)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL2, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HEAD)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName != AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			mCustomAvatar.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DETAILEYES, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.EYEMASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HAIR)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, CustomAvatarState.pCustomAvatarSettings.MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, CustomAvatarState.pCustomAvatarSettings.HIGHLIGHT, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_HIGHLIGHT));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_FEET)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HAND)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (inUpdateInventory && mCommonInventoryResponseItemList != null && mCommonInventoryResponseItemList.Length != 0)
		{
			CommonInventoryResponseItem commonInventoryResponseItem = Array.Find(mCommonInventoryResponseItemList, (CommonInventoryResponseItem r) => r.ItemID == inItem.ItemID);
			if (commonInventoryResponseItem != null)
			{
				mCustomAvatar.SetInventoryId(AvatarData.GetPartName(inItem), commonInventoryResponseItem.CommonInventoryID);
			}
		}
	}

	public void FinalizeZoomInZoomOut()
	{
		if (mPrevAvatar != null)
		{
			float num = (mZoom ? _ZoomInScale : _ZoomOutScale);
			AvAvatar.mTransform.localScale = new Vector3(num, num, num);
			AvAvatar.mTransform.localPosition = (mZoom ? _ZoomMarker.position : _3DAvatarMarker.transform.position);
			if (mZoom && mResetRotation)
			{
				AvAvatar.mTransform.eulerAngles = _3DAvatarMarker.transform.eulerAngles;
			}
		}
	}

	private void ZoomInAndOut()
	{
		if (!(mPrevAvatar != null))
		{
			return;
		}
		Vector3 vector = (mZoom ? _ZoomMarker.position : _3DAvatarMarker.transform.position);
		float b = (mZoom ? _ZoomInScale : _ZoomOutScale);
		if (mZoom && mResetRotation)
		{
			Vector3 eulerAngles = _3DAvatarMarker.transform.eulerAngles;
			Vector3 eulerAngles2 = AvAvatar.mTransform.eulerAngles;
			if (eulerAngles2.y > 180f)
			{
				eulerAngles.y = 360f;
			}
			eulerAngles2 = Vector3.Lerp(eulerAngles2, eulerAngles, Time.deltaTime * _ZoomInSpeedMultiplier);
			AvAvatar.mTransform.eulerAngles = eulerAngles2;
			if ((eulerAngles - AvAvatar.mTransform.eulerAngles).magnitude <= 1f)
			{
				mResetRotation = false;
			}
		}
		Vector3 localPosition = AvAvatar.mTransform.localPosition;
		localPosition = Vector3.Lerp(localPosition, vector, Time.deltaTime * _ZoomInSpeedMultiplier);
		if ((localPosition - vector).magnitude > 5f)
		{
			localPosition = vector;
		}
		AvAvatar.mTransform.localPosition = localPosition;
		float x = AvAvatar.mTransform.localScale.x;
		x = Mathf.Lerp(x, b, Time.deltaTime * _ZoomInSpeedMultiplier);
		AvAvatar.mTransform.localScale = new Vector3(x, x, x);
	}

	private void OnDataReady()
	{
		Texture2D newTexture = (Texture2D)mTexData._Texture;
		if (m3DData._Prefab != null && m3DData._Prefab.gameObject != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(m3DData._Prefab.gameObject);
			if (mTexData._Texture != null)
			{
				UtUtilities.SetObjectTexture(obj, 0, mTexData._Texture);
			}
			mPet.SetAccessory(mSelectedDragonAccType, obj, newTexture);
		}
	}

	private void SaveAvatarData()
	{
		SwapAvatars();
		AvatarData.Save();
		AvatarData.pInstanceInfo.RemovePart();
		AvatarData.pInstanceInfo.LoadBundlesAndUpdateAvatar();
		UiToolbar.pAvatarModified = true;
		_StoreUI._ProcessingClose = false;
	}

	private void SwapAvatars()
	{
		if (mPrevAvatar != null && AvAvatar.pObject != null)
		{
			GameObject pObject = AvAvatar.pObject;
			mPrevAvatar.SetActive(value: true);
			AvAvatar.pObject = mPrevAvatar;
			UnityEngine.Object.Destroy(pObject);
			mPrevAvatar = null;
			AvAvatar.SetPosition(mPrevPosition);
			AvatarData.pInstanceInfo.mAvatar = AvAvatar.pObject;
			if (mPreviousFollowAvatar && SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: true);
			}
			if (mAvatarMale != null)
			{
				UnityEngine.Object.Destroy(mAvatarMale);
			}
			mAvatarMale = null;
			if (mAvatarFemale != null)
			{
				UnityEngine.Object.Destroy(mAvatarFemale);
			}
			mAvatarFemale = null;
			mAvatarLoaded = false;
		}
	}

	private void ProcessLoadedAvatar(GameObject inObject, bool inMale)
	{
		if (mPrevAvatar != null)
		{
			UnityEngine.Object.Destroy(mPrevAvatar);
			mPrevAvatar = null;
		}
		if (inObject.GetComponent<AvAvatarController>() != null)
		{
			inObject.GetComponent<AvAvatarController>().enabled = false;
		}
		if (inObject.GetComponent<AvAvatarProperties>() != null)
		{
			inObject.GetComponent<AvAvatarProperties>().enabled = false;
		}
		if (inObject.GetComponent<AvSpellCast>() != null)
		{
			inObject.GetComponent<AvSpellCast>().enabled = false;
		}
		inObject.GetComponent<Collider>().enabled = false;
		mAvatarLoaded = true;
		AvAvatar.CacheAvatar();
		mPrevAvatar = AvAvatar.pObject;
		mPrevAvatar.SetActive(value: false);
		inObject.transform.localScale = Vector3.one * 0.8f;
		AvAvatar.pObject = inObject;
		AvatarData.pInstanceInfo.mAvatar = AvAvatar.pObject;
		if (SanctuaryManager.pCurPetInstance != null)
		{
			mPreviousFollowAvatar = SanctuaryManager.pCurPetInstance._FollowAvatar;
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
		}
		if (mAvatarFemale != null)
		{
			mAvatarFemale.SetActive(!inMale);
		}
		if (mAvatarMale != null)
		{
			mAvatarMale.SetActive(inMale);
		}
		AvAvatar.SetPosition(_3DAvatarMarker.transform);
	}

	public void SetPet(SanctuaryPet pet)
	{
		mPet = pet;
		if (_DisablePetMoodParticles)
		{
			mPet.DisableAllMoodParticles();
		}
		mPet._IdleAnimName = mPet._AnimNameIdle;
		if (mPet.pData.pGlowEffect != null && !mPet.pIsGlowDisabled)
		{
			mPet.pIsGlowDisabled = true;
			mPet.RemoveGlowEffect();
		}
		if (_3DMarker != null)
		{
			mPet.transform.position = _3DMarker.transform.position;
			mPet.transform.rotation = _3DMarker.transform.rotation;
			mPet.SetFollowAvatar(follow: false);
			Transform transform = mPet.transform.Find(mPet._RootBone);
			if (transform != null)
			{
				transform.localEulerAngles = Vector3.zero;
			}
		}
		AIBehavior_PrefabRef componentInChildren = mPet.GetComponentInChildren<AIBehavior_PrefabRef>();
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
		NavMeshAgent component = mPet.GetComponent<NavMeshAgent>();
		if (component != null)
		{
			component.enabled = false;
		}
		mPet.SetState(Character_State.idle);
		if (mPet != mCurrentPet)
		{
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mPetTypeID);
			if (sanctuaryPetTypeInfo != null)
			{
				if (mSelectedDragonAccType != 0)
				{
					mPet.transform.localScale = Vector3.one * _DragonUiScale * sanctuaryPetTypeInfo._AgeData[RaisedPetData.GetAgeIndex(RaisedPetStage.ADULT)]._UiScale * _DragonAverageZoom;
				}
				else
				{
					mPet.transform.localScale = Vector3.one * _DragonUiScale * sanctuaryPetTypeInfo._AgeData[RaisedPetData.GetAgeIndex(RaisedPetStage.BABY)]._UiScale * _DragonAverageZoom;
				}
			}
		}
		else
		{
			mPet.transform.localScale = Vector3.one * _DragonUiScale * SanctuaryManager.pCurPetInstance.pCurAgeData._UiScale * _DragonAverageZoom;
		}
		mPet.StopLookAtObject();
		DragonIdle dragonIdle = null;
		if (_DragonIdleOverrides != null)
		{
			dragonIdle = _DragonIdleOverrides.Find((DragonIdle di) => di._TypeID == mPetTypeID);
		}
		if (dragonIdle != null)
		{
			mPet.animation.Play(dragonIdle._Animation);
		}
		else if (!string.IsNullOrEmpty(mPet._AnimNameIdle))
		{
			mPet.animation.Play(mPet._AnimNameIdle);
		}
		KAUIStore.EnableStoreUIs(inEnable: true);
		_StoreUI.SetupFullPreviewInfo(mPet.transform, StorePreviewCategory.RaisedPet);
	}

	public override void ResetPreview()
	{
		base.ResetPreview();
		if (_StoreUI != null)
		{
			_StoreUI.ShowPreviewRotateButtons(visible: false);
			_StoreUI.ShowPreviewBuyButton(visible: false);
			_StoreUI.ShowFullPreviewButton(visible: false);
			_StoreUI.ShowAdsButton(visible: false);
		}
		if (mTxtPageItemCount != null)
		{
			mTxtPageItemCount.SetVisibility(inVisible: false);
		}
		SetPreviewButtonsVisible(visibility: false);
		if (mBtnPreviewMovie != null)
		{
			mBtnPreviewMovie.SetVisibility(inVisible: false);
		}
	}

	private void CreatePet(SanctuaryPetTypeInfo petType, RaisedPetStage rs, ItemData currentData, out string assetName, out string textureName)
	{
		if (StorePreview.pInstance != null && StorePreview.pInstance.mLoadingWidget != null)
		{
			StorePreview.pInstance.mLoadingWidget.SetVisibility(inVisible: true);
		}
		string resName = "";
		int ageIndex = RaisedPetData.GetAgeIndex(rs);
		if (petType._AgeData[ageIndex]._PetResList.Length != 0)
		{
			resName = petType._AgeData[ageIndex]._PetResList[0]._Prefab;
		}
		RaisedPetData raisedPetData = RaisedPetData.CreateCustomizedPetData(petType._TypeID, rs, resName, Gender.Male, null, noColorMap: true);
		raisedPetData.pNoSave = true;
		raisedPetData.Name = petType._Name;
		string attribute = raisedPetData.pStage.ToString()[0] + raisedPetData.pStage.ToString().Substring(1).ToLower() + "Mesh";
		assetName = currentData.GetAttribute(attribute, currentData.AssetName);
		attribute = raisedPetData.pStage.ToString()[0] + raisedPetData.pStage.ToString().Substring(1).ToLower() + "Tex";
		if (currentData.Texture != null)
		{
			textureName = currentData.GetAttribute(attribute, currentData.Texture[0].TextureName);
		}
		else
		{
			textureName = string.Empty;
		}
		SanctuaryManager.CreatePet(raisedPetData, Vector3.zero, Quaternion.identity, base.gameObject, "Basic");
	}

	public virtual void OnPetReady(SanctuaryPet pet)
	{
		if (StorePreview.pInstance != null && StorePreview.pInstance.mLoadingWidget != null)
		{
			StorePreview.pInstance.mLoadingWidget.SetVisibility(inVisible: false);
		}
		pet.gameObject.SetActive(value: true);
		SetPet(pet);
	}

	public void PreviewItemReady(StorePreviewCategory category, StorePreviewPage inPageItem)
	{
		ItemData itemData = null;
		if (inPageItem != null)
		{
			itemData = inPageItem.pCurrentItemData;
		}
		if (itemData != null)
		{
			_StoreUI.ShowPreviewBuyButton(visible: true);
			int rid;
			bool flag = base.pChosenItemData._ItemData.CashCost > _StoreUI._AdDisplayItemCostThreshold || base.pChosenItemData.IsLocked() || (!_StoreUI.pCategoryMenu.pDisableRankCheck && base.pChosenItemData.IsRankLocked(out rid, _StoreUI.pStoreInfo._RankTypeID));
			_StoreUI.ShowAdsButton(itemData.HasAttribute("AdsReward") && !flag);
			bool attribute = itemData.GetAttribute("2D", defaultValue: false);
			_StoreUI.ShowPreviewRotateButtons(!attribute);
			_StoreUI.ShowFullPreviewButton(!attribute);
		}
		switch (category)
		{
		case StorePreviewCategory.Avatar:
			ResetPreviewUI(inVisible: false, StorePreviewCategory.Normal3D);
			ResetPreviewUI(inVisible: false, StorePreviewCategory.RaisedPet);
			break;
		case StorePreviewCategory.RaisedPet:
			ResetPreviewUI(inVisible: false, StorePreviewCategory.Normal3D);
			ResetPreviewUI(inVisible: false, StorePreviewCategory.Avatar);
			break;
		case StorePreviewCategory.Normal3D:
			ResetPreviewUI(inVisible: false, StorePreviewCategory.RaisedPet);
			ResetPreviewUI(inVisible: false, StorePreviewCategory.Avatar);
			break;
		}
		ResetPreviewUI(inVisible: true, category);
		mCurPreviewCategory = category;
		StorePreview.pInstance.pIsDistinctItem = true;
		switch (category)
		{
		case StorePreviewCategory.Avatar:
			if (itemData != null)
			{
				SetZoom();
				_StoreUI.SetupFullPreviewInfo(AvAvatar.mTransform, category, mZoom);
			}
			mChosenIcon.SetTexture(null);
			AvatarData.RestoreDefault();
			mCustomAvatar.FromAvatarData(AvatarData.pInstance);
			mCustomAvatar.UpdateShaders(AvAvatar.pObject);
			mCustomAvatar.RestoreAll();
			if (inPageItem == null || inPageItem.pPreviewItemList == null || inPageItem.pPreviewItemList.pList == null)
			{
				break;
			}
			if (inPageItem.mPreviewItemIndex == -1)
			{
				StorePreview.pInstance.pIsDistinctItem = StorePreview.pInstance.CheckIfDistinctItems(inPageItem.pPreviewItemList);
				StorePreview.pInstance.pLeftLimitScroll = ((!StorePreview.pInstance.pIsDistinctItem) ? 1 : 0);
				if (!StorePreview.pInstance.pIsDistinctItem)
				{
					inPageItem.mPreviewItemIndex = 0;
				}
			}
			if (inPageItem.mPreviewItemIndex == -1)
			{
				itemData = inPageItem.pCurrentItemData;
				foreach (PreviewItemData p in inPageItem.pPreviewItemList.pList)
				{
					if (p != null)
					{
						ApplyItem(p.pItemData);
					}
				}
			}
			else
			{
				itemData = inPageItem.pPreviewItemList.pList[inPageItem.mPreviewItemIndex].pItemData;
				if (itemData != null)
				{
					ApplyItem(itemData);
				}
			}
			break;
		case StorePreviewCategory.RaisedPet:
		{
			if (itemData == null)
			{
				return;
			}
			mSelectedDragonAccType = RaisedPetData.GetAccessoryType(itemData);
			if (mSelectedDragonAccType != 0 && mPet != null)
			{
				GameObject accessoryObject = mPet.GetAccessoryObject(mSelectedDragonAccType);
				if (accessoryObject != null)
				{
					UnityEngine.Object.Destroy(accessoryObject);
				}
			}
			int attribute2 = itemData.GetAttribute("PetTypeID", -1);
			if (mCurrentPet != mPet && mPet != null && mPetTypeID != attribute2)
			{
				UnityEngine.Object.Destroy(mPet.gameObject);
				mPet = null;
			}
			mPetTypeID = attribute2;
			string assetName = itemData.AssetName;
			string textureName = ((itemData.Texture != null) ? itemData.Texture[0].TextureName : string.Empty);
			if (mCurrentPet != null && (attribute2 == -1 || (attribute2 > 0 && SanctuaryManager.pCurPetData != null && SanctuaryManager.pCurPetData.PetTypeID == attribute2 && SanctuaryManager.pCurPetData.pStage != RaisedPetStage.BABY && mSelectedDragonAccType != 0)))
			{
				mCurrentPet.gameObject.SetActive(value: true);
				SetPet(mCurrentPet);
				_StoreUI.SetupFullPreviewInfo(mPet.transform, category);
				string attribute3 = SanctuaryManager.pCurPetData.pStage.ToString()[0] + SanctuaryManager.pCurPetData.pStage.ToString().Substring(1).ToLower() + "Mesh";
				assetName = itemData.GetAttribute(attribute3, itemData.AssetName);
				attribute3 = SanctuaryManager.pCurPetData.pStage.ToString()[0] + SanctuaryManager.pCurPetData.pStage.ToString().Substring(1).ToLower() + "Tex";
				if (itemData.Texture != null)
				{
					textureName = itemData.GetAttribute(attribute3, itemData.Texture[0].TextureName);
				}
			}
			else
			{
				if (mCurrentPet != null)
				{
					mCurrentPet.gameObject.SetActive(value: false);
				}
				if (mPet == mCurrentPet || mPet == null)
				{
					mPet = null;
					SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(attribute2);
					if (sanctuaryPetTypeInfo != null)
					{
						KAUIStore.EnableStoreUIs(inEnable: false);
						if (mSelectedDragonAccType != 0)
						{
							CreatePet(sanctuaryPetTypeInfo, RaisedPetStage.ADULT, itemData, out assetName, out textureName);
						}
						else
						{
							CreatePet(sanctuaryPetTypeInfo, RaisedPetStage.BABY, itemData, out assetName, out textureName);
						}
					}
				}
			}
			m3DData.Init(assetName);
			m3DData.LoadData();
			mTexData.Init(textureName);
			mTexData.LoadData();
			mLoading3DData = true;
			break;
		}
		case StorePreviewCategory.Normal3D:
			if (itemData != null && pPreviewInstance != null)
			{
				_StoreUI.SetupFullPreviewInfo(pPreviewInstance.transform, category);
			}
			break;
		}
		if (mTxtPageItemCount != null)
		{
			if (inPageItem != null && inPageItem.pCount > 1)
			{
				mTxtPageItemCount.SetText(inPageItem.pCount.ToString());
				mTxtPageItemCount.SetVisibility(inVisible: true);
			}
			else
			{
				mTxtPageItemCount.SetVisibility(inVisible: false);
			}
		}
		if (itemData != null)
		{
			if (_StoreUI.pStatCompareMenu != null)
			{
				_StoreUI.pStatCompareMenu.RemoveStatPreview();
			}
			if (_StoreUI.pStatCompareMenu != null && _StoreUI.pStatCompareMenu.ShowStatsOnCategory(itemData))
			{
				_StoreUI.pStatCompareMenu.SetVisibility(inVisible: true);
				_StoreUI.pStatCompareMenu.UpdateStatsCompareData(inPageItem.mPreviewItemIndex, inPageItem.pPreviewItemList.pList);
			}
			if (_StoreUI.pDragonStatMenu != null)
			{
				_StoreUI.pDragonStatMenu.RemoveStatPreview();
			}
			if (_StoreUI.pDragonStatMenu != null && itemData.HasCategory(456) && (itemData.pBundledItems == null || itemData.pBundledItems.Count == 0))
			{
				_StoreUI.pDragonStatMenu.SetVisibility(inVisible: true);
				_StoreUI.pDragonStatMenu.SetDragonStatIcons(itemData);
			}
		}
	}

	private void DestroyAccessoryObj(SanctuaryPet mPet)
	{
		GameObject accessoryObject = mPet.GetAccessoryObject(mSelectedDragonAccType);
		if (accessoryObject != null)
		{
			UnityEngine.Object.Destroy(accessoryObject);
		}
	}

	public void OnCloseCustomizeItem(KAUISelectItemData inItem)
	{
		AvAvatar.mTransform.Find(AvatarSettings.pInstance._AvatarName).gameObject.SetActive(value: true);
		KAUIStore.pInstance.HideStoreUIs(hide: false);
		mCustomAvatar.mIsDirty = true;
		KAUIStore.pInstance._CustomizationPending = false;
		ShowStoreAvatar(show: true);
	}

	public void ShowStoreAvatar(bool show)
	{
		if (mAvatarMale != null)
		{
			mAvatarMale.SetActive(show);
		}
		else if (mAvatarFemale != null)
		{
			mAvatarFemale.SetActive(show);
		}
	}
}
