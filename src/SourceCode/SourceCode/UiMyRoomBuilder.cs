using System.Collections.Generic;
using UnityEngine;

public class UiMyRoomBuilder : KAUI
{
	public UiMyRoomsInt _MyRoomsInt;

	public TextAsset _TbTutBuild;

	public string _LongIntroName;

	public string _ShortIntroName;

	public Color _ObjectNonPlaceableColor = new Color(0.8f, 0.3f, 0.3f);

	public float _RayCastHeightTolerance = 0.02f;

	public float _ObjectRotationY = 45f;

	public int _MaxRoomObjectCount = 10;

	public AudioClip _MaxRoomObjectVO;

	public bool _OCM_ObjectCenter = true;

	public Vector2 _ObjectControlUISize = new Vector2(180f, 110f);

	public Rect _ObjectControlUIVisibleArea = new Rect(155f, 0f, 635f, 560f);

	public MyRoomBuilderCategory[] _CategoryList;

	public AudioClip _SndBuildOpen;

	public AudioClip _SndBuildReturn;

	public AudioClip _SndBuildPlaceImage;

	public AudioClip _SndBuildPlaceObject;

	public LocaleString _ItemsText = new LocaleString(" items");

	public string _MyRoomsStoreName = "Rooms Store";

	public string _TaskBuildSpot;

	public bool _DisableObjectsClickableOnBack = true;

	public GameObject _BuildmodeCamera;

	public GameObject _AvatarCamera;

	public BuildModeMenuScroller _Scroller;

	public LocaleString _CreativePointsExceededText = new LocaleString("Placing this item will exceed the max Creative Points Limit");

	public string _CreativePointsProgressFullSprite = "AniDWDragonsMeterBarCreativePointsFull";

	public string _CreativePointsProgressSprite = "AniDWDragonsMeterBarCreativePoints";

	protected Vector3 mAvatarCamPos = Vector3.zero;

	protected Camera25D mBuildmodeCam;

	protected CaAvatarCam mAvatarCam;

	protected KAWidget mTxtRoomCount;

	protected KAWidget mCoinAmountLabel;

	protected KAWidget mGemAmountLabel;

	protected KAWidget mBtnGemCounter;

	protected KAWidget mTxtCreativePointsLabel;

	protected KAWidget mAniCreativePointsProgress;

	private KAWidget mSelectedWallPaper;

	private KAWidget mNonplaceableItem;

	protected UiMyRoomBuilderMenu mBuildModeMenu;

	private UiMyRoomBuilderCategoryMenu mBuildModeCategoryMenu;

	private bool mCanPlaceDragObject;

	protected bool mIsObjectPlacedFirstTime;

	private bool mInitialized;

	protected ItemData mDragData;

	private ItemData mSelectedObjectData;

	protected object mDragObject;

	private GameObject mDragProxy;

	protected GameObject mSelectedObject;

	protected GameObject mPrevSelectedObject;

	private GameObject mStoreObject;

	protected Transform mParentStackObject;

	protected Vector3 mWallHitNormal = Vector3.zero;

	protected bool mCanPlace;

	private Vector3 mWalkModeCameraOffset;

	private Vector3 mDragObjectPreviousPosition = Vector3.zero;

	protected Vector3 mPreviousMousePosition = Vector3.zero;

	protected Vector3 mPrevBuildmodeCamPos = Vector3.zero;

	private Quaternion mDragObjectPreviousRotation = Quaternion.identity;

	private AvAvatarControlMode mPrevAvatarControlMode;

	protected UserItemData mCurrentUserItemData;

	private CoIdleManager mIdle;

	private Rect mObjectControlUIVisibleArea;

	protected bool mCameraSwitched;

	protected int mPreviousSelectedCategory = -1;

	protected KAWidget mExpandBtn;

	protected KAWidget mMinimizeBtn;

	protected KAWidget mTxtCategory;

	protected KAWidget mCategoryIcon;

	public UiMyRoomBuilderMenu pBuildModeMenu => mBuildModeMenu;

	protected bool pInitialized => mInitialized;

	public object pDragObject
	{
		get
		{
			return mDragObject;
		}
		set
		{
			mDragObject = value;
		}
	}

	public virtual GameObject pSelectedObject
	{
		get
		{
			return mSelectedObject;
		}
		set
		{
			mPrevSelectedObject = mSelectedObject;
			mSelectedObject = value;
			if (!(mPrevSelectedObject != null))
			{
				return;
			}
			if (mPrevSelectedObject == (GameObject)pDragObject)
			{
				DetachDragProxy(mPrevSelectedObject);
				pDragObject = null;
			}
			MyRoomItem component = mPrevSelectedObject.GetComponent<MyRoomItem>();
			if (!(component != null))
			{
				return;
			}
			component.HighlightObject(canShowHightlight: false);
			if (!KAInput.pInstance.IsTouchInput())
			{
				return;
			}
			if (mSelectedObject == null)
			{
				component.RemoveMenuOfType(ContextSensitiveStateType.ONCLICK);
				return;
			}
			MyRoomItem component2 = mSelectedObject.GetComponent<MyRoomItem>();
			if (component2 != null)
			{
				component2.HighlightObject(canShowHightlight: true);
			}
		}
	}

	public virtual void Initialize()
	{
	}

	public virtual void OnEnable()
	{
		mExpandBtn = FindItem("SocialBarExpandBtn");
		mMinimizeBtn = FindItem("SocialBarMinimizeBtn");
		mTxtCategory = FindItem("TxtCategory");
		mCategoryIcon = FindItem("AniActiveCategoryIcon");
		if (mExpandBtn != null)
		{
			mExpandBtn.SetVisibility(inVisible: false);
		}
		if (mMinimizeBtn != null)
		{
			mMinimizeBtn.SetVisibility(inVisible: true);
		}
	}

	protected virtual void Disable()
	{
	}

	protected override void Start()
	{
		base.Start();
		mBuildModeMenu = GetComponentInChildren<UiMyRoomBuilderMenu>();
		mBuildModeCategoryMenu = GetComponentInChildren<UiMyRoomBuilderCategoryMenu>();
		mIdle = GetComponent<CoIdleManager>();
		mTxtRoomCount = FindItem("RoomCountBkg");
		mTxtRoomCount.SetState(KAUIState.NOT_INTERACTIVE);
		PlayIntroTutorial();
		mObjectControlUIVisibleArea = _ObjectControlUIVisibleArea;
		mObjectControlUIVisibleArea.width += mObjectControlUIVisibleArea.x;
		mObjectControlUIVisibleArea.height += mObjectControlUIVisibleArea.y;
		if (_BuildmodeCamera != null)
		{
			mBuildmodeCam = _BuildmodeCamera.GetComponent<Camera25D>();
		}
		_BuildmodeCamera.SetActive(value: false);
		if (_AvatarCamera != null)
		{
			mAvatarCam = _AvatarCamera.GetComponent<CaAvatarCam>();
		}
		SetVisibility(inVisible: false);
		mCoinAmountLabel = FindItem("TxtCoinAmount");
		mGemAmountLabel = FindItem("TxtGemAmount");
		mBtnGemCounter = FindItem("BtnGemCounter");
		mAniCreativePointsProgress = FindItem("AniCreativeBar");
		mTxtCreativePointsLabel = FindItem("TxtCreativePoints");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void PlayIntroTutorial()
	{
		if (MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			if (_TbTutBuild != null && TutorialManager.HasTutorialPlayed(_LongIntroName))
			{
				TutorialManager.StartTutorial(_TbTutBuild, _ShortIntroName, bMarkDone: false, 12u, null);
			}
		}
		else if (_TbTutBuild != null && !TutorialManager.StartTutorial(_TbTutBuild, _LongIntroName, bMarkDone: true, 2u, null))
		{
			TutorialManager.StartTutorial(_TbTutBuild, _ShortIntroName, bMarkDone: false, 2u, null);
		}
	}

	private bool HandleWallHangings(MyRoomObject roomObj, GameObject dragObject, Vector3 wallHitNormal)
	{
		bool flag = false;
		int num = 0;
		int layerMask = ~((1 << LayerMask.NameToLayer("Avatar")) | (1 << LayerMask.NameToLayer("MMOAvatar")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("DraggedObject")) | (1 << LayerMask.NameToLayer("IgnoreGroundRay")));
		if (mParentStackObject != null && mParentStackObject.gameObject.layer == LayerMask.NameToLayer("Default"))
		{
			flag = false;
		}
		else if (roomObj.pRayCastPoints != null && roomObj.pRayCastPoints.Length == 3)
		{
			RaycastHit hitInfo;
			for (int i = 0; i < 2; i++)
			{
				if (Physics.Raycast(new Ray(roomObj.pRayCastPoints[i].transform.position, -dragObject.transform.forward), out hitInfo, 0.29f, layerMask))
				{
					if (hitInfo.collider.gameObject.layer != LayerMask.NameToLayer("Wall"))
					{
						break;
					}
					num++;
				}
			}
			if (num == 2)
			{
				flag = true;
			}
			if (Physics.Raycast(new Ray(roomObj.pRayCastPoints[2].transform.position, -dragObject.transform.up), out hitInfo, 0.15f, layerMask))
			{
				if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Floor"))
				{
					flag = false;
				}
			}
			else if (wallHitNormal != Vector3.zero)
			{
				dragObject.transform.forward = wallHitNormal;
			}
		}
		return flag && !roomObj.IsObjectColliding(null);
	}

	public virtual bool HandleFloorItems(MyRoomObject roomObj, GameObject dragObject, ref bool isObjectStacked)
	{
		bool flag = false;
		List<GameObject> list = new List<GameObject>();
		int layerMask = ~((1 << LayerMask.NameToLayer("Avatar")) | (1 << LayerMask.NameToLayer("MMOAvatar")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("IgnoreGroundRay")) | (1 << LayerMask.NameToLayer("DraggedObject")));
		if (roomObj.pRayCastPoints != null)
		{
			GameObject gameObject = null;
			float num = 0f;
			int num2 = 0;
			int num3 = 1;
			for (int i = 0; i < num3; i++)
			{
				Ray ray = new Ray(roomObj.pRayCastPoints[i].transform.position, -Vector3.up);
				if (Physics.Raycast(ray, out var hitInfo, 0.2f, layerMask))
				{
					Debug.DrawLine(ray.origin, hitInfo.point);
					if (i == 0)
					{
						gameObject = hitInfo.collider.gameObject;
						num = hitInfo.distance;
					}
					else if (gameObject != hitInfo.collider.gameObject || hitInfo.distance < num - _RayCastHeightTolerance || hitInfo.distance > num + _RayCastHeightTolerance)
					{
						break;
					}
					num2++;
				}
			}
			if (num2 == num3)
			{
				if (gameObject.layer == LayerMask.NameToLayer("Floor"))
				{
					if (CanPlaceDragObject(dragObject, mCurrentUserItemData))
					{
						flag = true;
					}
				}
				else if (gameObject.layer == LayerMask.NameToLayer("Furniture") && CheckStacking(roomObj.gameObject, gameObject))
				{
					list.Add(gameObject);
					flag = true;
					isObjectStacked = true;
				}
			}
			MyRoomObject.ChildData[] pChildList = roomObj.pChildList;
			foreach (MyRoomObject.ChildData childData in pChildList)
			{
				list.Add(childData._Child);
			}
		}
		return flag && !roomObj.IsObjectColliding(list.ToArray());
	}

	public virtual bool HandleDependentObject(GameObject dragObject, ref bool isObjectStacked)
	{
		return false;
	}

	public virtual void HandleObjectPlacement(bool canPlace, GameObject dragObject, bool isObjectStacked)
	{
		if (canPlace)
		{
			mCanPlaceDragObject = true;
			ChangeObjectColor(dragObject, new Color(1f, 1f, 1f));
			if (KAInput.GetMouseButtonUp(0))
			{
				if (isObjectStacked)
				{
					UseDragObject(dragObject, mParentStackObject);
				}
				else
				{
					UseDragObject(dragObject);
				}
				OnDragObject(isDragging: false);
			}
		}
		else
		{
			mCanPlaceDragObject = false;
			ChangeObjectColor(dragObject, _ObjectNonPlaceableColor);
		}
		if (mSelectedObject != null)
		{
			MyRoomItem component = mSelectedObject.GetComponent<MyRoomItem>();
			if (component != null)
			{
				component.UpdateContextItemState(canPlace);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!KAInput.pInstance.IsTouchInput() && KAInput.GetMouseButtonUp(0) && pSelectedObject != null && pSelectedObject != ObClickable.pMouseOverObject && mDragObject == null)
		{
			GetSelectedObject(null);
		}
		if (!mInitialized && !MyRoomsIntMain.pDisableBuildMode)
		{
			mInitialized = true;
			EnableBuildMode(isEnabled: true);
		}
		if (mCameraSwitched)
		{
			mCameraSwitched = false;
		}
		bool flag = true;
		GameObject gameObject = null;
		if (mDragObject == null)
		{
			if (mSelectedObject != null && mSelectedObject.GetComponent<MyRoomObject>()._IsDraggable)
			{
				flag = false;
				gameObject = mSelectedObject;
			}
		}
		else if (mDragObject.GetType().Equals(typeof(GameObject)))
		{
			gameObject = (GameObject)mDragObject;
		}
		if (gameObject != null)
		{
			Ray ray = Camera.main.ScreenPointToRay(KAInput.mousePosition);
			mWallHitNormal = Vector3.zero;
			bool isObjectStacked = false;
			int num = 1 << LayerMask.NameToLayer("Floor");
			MyRoomObject component = gameObject.GetComponent<MyRoomObject>();
			if (component._ObjectType == MyRoomObjectType.WallHanging)
			{
				num = 1 << LayerMask.NameToLayer("Wall");
			}
			if (component._IsStackableObject)
			{
				num |= 1 << LayerMask.NameToLayer("Furniture");
			}
			if (gameObject.layer != LayerMask.NameToLayer("DraggedObject"))
			{
				UtUtilities.SetLayerRecursively(gameObject, LayerMask.NameToLayer("DraggedObject"));
			}
			if (flag && (mPreviousMousePosition != KAInput.mousePosition || mBuildmodeCam.gameObject.transform.position != mPrevBuildmodeCamPos))
			{
				if (Physics.Raycast(ray, out var hitInfo, float.PositiveInfinity, num))
				{
					GameObject gameObject2 = hitInfo.collider.gameObject;
					Vector3 position = CalculatePosition(hitInfo.point, gameObject.transform);
					if (component != null)
					{
						position = new Vector3(position.x, position.y + component._DragYOffset, position.z);
					}
					gameObject.transform.position = position;
					mParentStackObject = hitInfo.collider.transform;
					if (gameObject2.layer == LayerMask.NameToLayer("Wall"))
					{
						mWallHitNormal = hitInfo.normal;
					}
					else if (gameObject2.layer != LayerMask.NameToLayer("Furniture"))
					{
					}
				}
				else
				{
					mParentStackObject = null;
				}
			}
			MyRoomObject component2 = gameObject.GetComponent<MyRoomObject>();
			if (component2 != null)
			{
				if (IsDependentObject(gameObject))
				{
					mCanPlace = HandleDependentObject(gameObject, ref isObjectStacked);
				}
				else if (component2._ObjectType == MyRoomObjectType.WallHanging)
				{
					mCanPlace = HandleWallHangings(component2, gameObject, mWallHitNormal);
				}
				else if (component2._ObjectType == MyRoomObjectType.OnFloor)
				{
					mCanPlace = HandleFloorItems(component2, gameObject, ref isObjectStacked);
					if (!isObjectStacked)
					{
						mParentStackObject = null;
					}
				}
				HandleObjectPlacement(mCanPlace, gameObject, isObjectStacked);
			}
		}
		if (mSelectedWallPaper != null && !MyRoomsIntMain.pInstance.pApplyWallpaperAllWalls)
		{
			Ray ray2 = Camera.main.ScreenPointToRay(KAInput.mousePosition);
			RaycastHit hitInfo2 = default(RaycastHit);
			int layerMask = 1 << LayerMask.NameToLayer("Wall");
			if (Physics.Raycast(ray2, out hitInfo2, 300f, layerMask) && KAUI.GetGlobalMouseOverItem() == null)
			{
				SetWallPaperAllowed(isAllowed: true);
				MyRoomsIntMain.pInstance.ApplyWallPaper(hitInfo2.collider.gameObject, mSelectedWallPaper.GetTexture(), isTemp: true);
			}
			else
			{
				SetWallPaperAllowed(isAllowed: false);
				MyRoomsIntMain.pInstance.RevertToPrevWallPaper();
			}
			if (KAInput.GetMouseButtonUp(0))
			{
				if (MyRoomsIntMain.pInstance.pWallPaperObject != null)
				{
					GameObject gameObject3 = MyRoomsIntMain.pInstance.ReturnSelectedWall(MyRoomsIntMain.pInstance.pWallPaperObject);
					if (gameObject3 != null)
					{
						RemoveWallPaperTexture(gameObject3, inDestroy: false);
					}
					MyRoomObject component3 = gameObject3.GetComponent<MyRoomObject>();
					if (component3 != null)
					{
						component3.pUserItemData = mCurrentUserItemData;
					}
					MyRoomsIntMain.pInstance.AddRoomObject(gameObject3, mCurrentUserItemData, gameObject3.transform.localPosition, gameObject3.transform.rotation.eulerAngles, null, isUpdateLocalList: false);
					MyRoomsIntMain.pInstance.ApplyWallPaper(MyRoomsIntMain.pInstance.pWallPaperObject, mSelectedWallPaper.GetTexture(), isTemp: false);
				}
				else
				{
					AddItemToInventory(mCurrentUserItemData);
				}
				MyRoomsIntMain.EnableRoomObjectsClickable(isEnable: true);
				ClearWallPaperData();
				mDragData = null;
				mCurrentUserItemData = null;
			}
		}
		mPreviousMousePosition = KAInput.mousePosition;
		mPrevBuildmodeCamPos = mBuildmodeCam.gameObject.transform.position;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mExpandBtn)
		{
			mExpandBtn.SetVisibility(inVisible: false);
			_Scroller.ToggleBuildModeMenu();
		}
		else if (inWidget == mMinimizeBtn)
		{
			mMinimizeBtn.SetVisibility(inVisible: false);
			_Scroller.ToggleBuildModeMenu();
		}
		else if (inWidget == mBtnGemCounter)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
		}
		else if (mBuildModeCategoryMenu.GetItems().Contains(inWidget))
		{
			UpdateUI(inWidget);
		}
		OnClickAction(inWidget.name);
		KAInput.ResetInputAxes();
	}

	private void UpdateUI(KAWidget widget)
	{
		if (!(mCategoryIcon != null) || !(mTxtCategory != null))
		{
			return;
		}
		MyRoomBuilderCategory[] categoryList = _CategoryList;
		foreach (MyRoomBuilderCategory myRoomBuilderCategory in categoryList)
		{
			if (myRoomBuilderCategory._Category.ToString() == widget.name)
			{
				mCategoryIcon.SetTexture(myRoomBuilderCategory._IconTex);
				mTxtCategory.SetText(myRoomBuilderCategory._CategoryDispName.GetLocalizedString());
				break;
			}
		}
	}

	public void OnIAPStoreClosed()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
	}

	private void OnScrollDone(bool isShow)
	{
		if (isShow)
		{
			mMinimizeBtn.SetVisibility(inVisible: true);
		}
		else
		{
			mExpandBtn.SetVisibility(inVisible: true);
		}
	}

	public virtual void OnExit()
	{
		SetInteractive(interactive: true);
		if (mDragObject != null)
		{
			if ((GameObject)mDragObject == mSelectedObject)
			{
				OnRelease();
				mCanPlace = false;
			}
			else
			{
				DeleteObject(mDragObject, mDragData, addToInv: true, 0);
			}
		}
		HandleCurrentSelection();
		HideUI();
	}

	public virtual void OnClickAction(string inActionString)
	{
		MyRoomItem myRoomItem = null;
		if (mSelectedObject != null)
		{
			myRoomItem = mSelectedObject.GetComponent<MyRoomItem>();
		}
		switch (inActionString)
		{
		case "BtnBmBack":
			SetInteractive(interactive: false);
			if (MyRoomsIntMain.pInstance != null)
			{
				MyRoomsIntMain.pInstance.Save();
			}
			break;
		case "BtnBmHelp":
			if (mIdle != null)
			{
				mIdle.OnIdlePlay();
			}
			break;
		case "Accept":
			HandleCurrentSelection();
			break;
		case "ObjectRotateLtBtn":
			if (myRoomItem != null && !myRoomItem.pItemPlacementDirty)
			{
				MakeObjectPlacementDirty(mSelectedObject);
				AttachDragProxy(mSelectedObject);
			}
			RotateObject(mSelectedObject, _ObjectRotationY);
			break;
		case "ObjectRotateRtBtn":
			if (myRoomItem != null && !myRoomItem.pItemPlacementDirty)
			{
				MakeObjectPlacementDirty(mSelectedObject);
				AttachDragProxy(mSelectedObject);
			}
			RotateObject(mSelectedObject, 0f - _ObjectRotationY);
			break;
		case "ObjectMoveBtn":
			if (mSelectedObject != null)
			{
				MakeObjectPlacementDirty(mSelectedObject);
				mDragObject = mSelectedObject;
				mDragData = mSelectedObjectData;
				AttachDragProxy((GameObject)mDragObject);
				if (MyRoomsIntMain.pInstance != null)
				{
					MyRoomsIntMain.pInstance._CameraConfigUpdated = false;
				}
				mBuildmodeCam.pObjectToFollow = (GameObject)mDragObject;
				CheckForDecorateTaskCompletion();
				KAInput.ResetInputAxes();
			}
			break;
		case "ObjectPickUpBtn":
			DeleteObject(mSelectedObject, mSelectedObjectData, addToInv: true, 0);
			CheckForDecorateTaskCompletion();
			break;
		case "ObjectDestroyBtn":
			if (mSelectedObject != null && myRoomItem != null)
			{
				if (myRoomItem is Removable)
				{
					mSelectedObject.SendMessage("RemoveObject");
				}
				else
				{
					MyRoomsIntMain.pInstance.RemoveRoomObject(mSelectedObject, isDestroy: true);
				}
				pSelectedObject = null;
				mSelectedObjectData = null;
			}
			break;
		case "StoreBtn":
			HandleCurrentSelection();
			if (MyRoomsIntMain.pInstance != null)
			{
				MyRoomsIntMain.pInstance.SetBuildMode(inBuildMode: false);
			}
			HideUI();
			_MyRoomsInt.BuildModeClosed();
			StoreLoader.Load(setDefaultMenuItem: true, "", _MyRoomsStoreName, base.gameObject);
			if (UtPlatform.IsMobile())
			{
				UiMyRoomsInt._SwitchToBuildMode = true;
			}
			break;
		}
	}

	private void UpdateCurrencyDisplay()
	{
		if (mCoinAmountLabel != null)
		{
			mCoinAmountLabel.SetText(Money.pGameCurrency.ToString());
		}
		if (mGemAmountLabel != null)
		{
			mGemAmountLabel.SetText(Money.pCashCurrency.ToString());
		}
	}

	protected void OnStoreClosed()
	{
		ShowUI();
		mBuildmodeCam._EnableCameraMovement = true;
	}

	private void EnableBuildMode(bool isEnabled)
	{
		ObClickable.pGlobalActive = isEnabled;
		SetState((!isEnabled) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
		if (isEnabled)
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		}
		else
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		}
	}

	public virtual void ShowUI()
	{
		if (_BuildmodeCamera != null && _AvatarCamera != null && KAInput.pInstance.IsTouchInput())
		{
			UICamera.selectedObject = null;
		}
		SetVisibility(inVisible: true);
		mBuildModeMenu.SetVisibility(inVisible: true);
		mBuildModeCategoryMenu.SetVisibility(inVisible: true);
		mCameraSwitched = true;
		if (MyRoomsIntMain.pDisableBuildMode)
		{
			mInitialized = false;
			AvAvatar.SetUIActive(inActive: false);
			EnableBuildMode(isEnabled: false);
			return;
		}
		mInitialized = true;
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: false);
			MainStreetMMOClient.pInstance.pPauseRemoteAvatar = true;
			MainStreetMMOClient.pInstance.SetBusy(busy: true);
		}
		AvAvatar.SetUIActive(inActive: false);
		if (KAInput.pInstance != null && KAInput.pInstance.IsTouchInput())
		{
			KAInput.pInstance.ShowInputs(inShow: true);
		}
		ObClickable.pGlobalActive = true;
		MyRoomsIntMain.EnableRoomObjectsClickable(isEnable: true);
		if (MyRoomsIntMain.pInstance != null)
		{
			MyRoomsIntMain.pInstance.SetBuildMode(inBuildMode: true);
		}
		SetCategoryMenu(-1);
		if ((bool)_SndBuildOpen)
		{
			SnChannel.Play(_SndBuildOpen, "SFX_Pool", 0, inForce: true);
		}
		KAInput.ResetInputAxes();
		UpdateRoomObjectCountText();
		UpdateCurrencyDisplay();
		if (mBuildModeCategoryMenu.GetItemCount() > 1)
		{
			OnClick(mBuildModeCategoryMenu.GetItems()[0]);
		}
	}

	private void SetEnableAvatarControls(bool inEnable)
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, SanctuaryManager.pCurPetInstance.IsMountAllowed());
		}
		KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable);
		KAInput.ShowJoystick(JoyStickPos.BOTTOM_LEFT, inEnable);
	}

	public void HideUI()
	{
		SetVisibility(inVisible: false);
		mBuildModeMenu.SetVisibility(inVisible: false);
		mBuildModeCategoryMenu.SetVisibility(inVisible: false);
		HandleCurrentSelection();
		MyRoomsIntMain.EnableRoomObjectsClickable(!_DisableObjectsClickableOnBack);
		if (mSelectedWallPaper != null)
		{
			DeleteAndClearWallpaper();
		}
		CommonInventoryData.pInstance.ClearSaveCache();
		mBuildModeMenu.ClearItems();
		AvAvatar.SetUIActive(inActive: true);
		TutorialManager.StopTutorials();
		SnChannel.StopPool("VO_Pool");
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
			MainStreetMMOClient.pInstance.pPauseRemoteAvatar = false;
			MainStreetMMOClient.pInstance.SetBusy(busy: false);
		}
		mPreviousSelectedCategory = -1;
	}

	public void SetPage(KAWidget inButton)
	{
		if (int.TryParse(inButton.name, out var result) && mPreviousSelectedCategory != result)
		{
			UpdateCategory(result);
			mPreviousSelectedCategory = result;
		}
	}

	public void UpdateCategory(int category)
	{
		mBuildModeMenu.ClearItems();
		AddItems(category);
		mBuildModeMenu.mNeedPageCheck = true;
		mBuildModeMenu.SetVisibility(inVisible: true);
	}

	private void AddItems(int category)
	{
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(category);
		if (items == null)
		{
			return;
		}
		int num = 0;
		UserItemData[] array = items;
		foreach (UserItemData userItemData in array)
		{
			ItemData item = userItemData.Item;
			if (item != null)
			{
				num++;
				MyRoomBuilderItemData myRoomBuilderItemData = new MyRoomBuilderItemData(mBuildModeMenu, this, item);
				KAWidget kAWidget = DuplicateWidget(mBuildModeMenu._Template);
				kAWidget.gameObject.SetActive(value: true);
				kAWidget.SetUserData(myRoomBuilderItemData);
				string text = item.ItemName;
				if (CommonInventoryData.pShowItemID)
				{
					text = text + "(" + item.ItemID + ")";
				}
				kAWidget.SetToolTipText(text);
				KAWidget kAWidget2 = kAWidget.FindChildItem("Quantity");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(userItemData.Quantity.ToString());
				}
				KAWidget kAWidget3 = kAWidget.FindChildItem("CreativePoints");
				if (kAWidget3 != null)
				{
					kAWidget3.SetText(item.CreativePoints.ToString());
				}
				mBuildModeMenu.AddWidget(kAWidget);
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
				if (num > 0 && num <= mBuildModeMenu.GetNumItemsPerPage())
				{
					myRoomBuilderItemData.LoadResource();
				}
			}
		}
	}

	protected virtual void OnClick(GameObject go)
	{
	}

	protected virtual void OnPress(GameObject go)
	{
		GetSelectedObject(go);
	}

	protected virtual void OnDragStart()
	{
		if (!(mSelectedObject != null))
		{
			return;
		}
		MyRoomObject component = mSelectedObject.GetComponent<MyRoomObject>();
		if (component != null && component._IsDraggable)
		{
			MakeObjectPlacementDirty(mSelectedObject);
			mDragObject = mSelectedObject;
			mDragData = mSelectedObjectData;
			if (mDragProxy == null || mDragProxy.transform.parent == null)
			{
				AttachDragProxy((GameObject)mDragObject);
			}
			mCanPlace = false;
			mParentStackObject = null;
			mWallHitNormal = Vector3.zero;
			mBuildmodeCam.pObjectToFollow = mSelectedObject;
		}
	}

	protected virtual void OnRelease()
	{
		if (mDragObject != null)
		{
			if (mSelectedObject.GetComponent<MyRoomObject>() != null)
			{
				MyRoomsIntMain.pInstance.pSelectedObjectType = MyRoomObjectType.Default;
			}
			pSelectedObject = (GameObject)mDragObject;
			mSelectedObjectData = mDragData;
		}
		if (pSelectedObject != null)
		{
			DetachDragProxy(pSelectedObject);
		}
		mDragObject = null;
		mDragData = null;
		mBuildmodeCam.pObjectToFollow = null;
	}

	private void RemoveSkyBox()
	{
		if (MyRoomsIntMain.pInstance != null)
		{
			GameObject pSkyBoxObject = MyRoomsIntMain.pInstance.pSkyBoxObject;
			if (RemoveObject(pSkyBoxObject, inDestroy: true))
			{
				MyRoomsIntMain.pInstance.RemoveSkyBox();
			}
		}
	}

	private void RemoveWallPaperTexture(GameObject inObject, bool inDestroy)
	{
		RemoveObject(inObject, inDestroy);
	}

	private void RemoveFloorTexture()
	{
		if (MyRoomsIntMain.pInstance != null)
		{
			RemoveObject(MyRoomsIntMain.pInstance.pFloorObject, inDestroy: true);
		}
	}

	private void RemoveCushionTexture()
	{
		if (MyRoomsIntMain.pInstance != null)
		{
			RemoveObject(MyRoomsIntMain.pInstance.pCushionObject, inDestroy: true);
		}
	}

	protected bool RemoveObject(GameObject inObject, bool inDestroy)
	{
		if (inObject != null)
		{
			MyRoomObject component = inObject.GetComponent<MyRoomObject>();
			if (component != null)
			{
				AddItemToInventory(component.pUserItemData);
				if (MyRoomsIntMain.pInstance != null)
				{
					MyRoomsIntMain.pInstance.pCategoryIgnoreList.Remove(component.pUserItemData.Item.ItemID);
				}
				MyRoomsIntMain.pInstance.RemoveRoomObject(inObject, inDestroy);
				return true;
			}
		}
		return false;
	}

	public virtual void Selected(KAWidget item, int index)
	{
		MyRoomBuilderItemData myRoomBuilderItemData = (MyRoomBuilderItemData)item.GetUserData();
		if (myRoomBuilderItemData._Locked)
		{
			if (mBuildModeMenu._LockVO != null)
			{
				SnChannel.Play(mBuildModeMenu._LockVO, "VO_Pool", inForce: false);
			}
			return;
		}
		if (mDragObject != null)
		{
			DeleteObject(mDragObject, mDragData, addToInv: true, index);
			MyRoomsIntMain.EnableRoomObjectsClickable(isEnable: true);
		}
		else if (mSelectedObject != null)
		{
			UseSelectedObject(useOldPosition: false);
		}
		else if (mSelectedWallPaper != null)
		{
			DeleteAndClearWallpaper();
		}
		else
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(myRoomBuilderItemData._ItemID);
			mDragData = userItemData.Item;
			int additionalPoints = 0;
			if (mDragData.CreativePoints > 0)
			{
				additionalPoints = mDragData.CreativePoints;
			}
			if (MyRoomsIntMain.pInstance.HasCreativePointsLimitReached(additionalPoints))
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _CreativePointsExceededText.GetLocalizedString(), null, "");
				mDragData = null;
				return;
			}
			mCurrentUserItemData = userItemData;
			if ((mDragData.HasCategory(249) || mDragData.HasCategory(248) || mDragData.HasCategory(247) || mDragData.HasCategory(367)) && MyRoomsIntMain.pInstance != null && !MyRoomsIntMain.pInstance.pCategoryIgnoreList.Contains(mDragData.ItemID))
			{
				MyRoomsIntMain.pInstance.pCategoryIgnoreList.Add(mDragData.ItemID);
			}
			if (CommonInventoryData.pInstance.RemoveItem(mDragData.ItemID, updateServer: false) == 0)
			{
				mBuildModeMenu.RemoveWidget(item);
			}
			myRoomBuilderItemData.UpdateQuantity();
			CreateRoomObject(myRoomBuilderItemData);
		}
		UpdateRoomObjectCountText();
	}

	protected virtual void CreateRoomObject(MyRoomBuilderItemData itemData)
	{
		if (mDragData.HasCategory(249))
		{
			if (MyRoomsIntMain.pInstance != null)
			{
				if (MyRoomsIntMain.pInstance.pApplyWallpaperAllWalls)
				{
					RemoveWallPaperTexture(MyRoomsIntMain.pInstance.pWallPaperObject, inDestroy: true);
					MyRoomsIntMain.pInstance.AddWallPaperObject(mCurrentUserItemData, mDragData.AssetName);
					mDragData = null;
					mCurrentUserItemData = null;
				}
				else
				{
					SetWallPaperData(mDragData, itemData);
				}
				if ((bool)_SndBuildPlaceImage)
				{
					SnChannel.Play(_SndBuildPlaceImage, "SFX_Pool", 0, inForce: false);
				}
			}
		}
		else if (mDragData.HasCategory(329))
		{
			if (MyRoomsIntMain.pInstance != null)
			{
				RemoveSkyBox();
				MyRoomsIntMain.pInstance.AddSkyBox(mCurrentUserItemData, mDragData.AssetName);
				if ((bool)_SndBuildPlaceImage)
				{
					SnChannel.Play(_SndBuildPlaceImage, "SFX_Pool", 0, inForce: false);
				}
				mDragData = null;
				mCurrentUserItemData = null;
			}
		}
		else if (mDragData.HasCategory(248))
		{
			bool flag = false;
			if (mCurrentUserItemData != null)
			{
				flag = mCurrentUserItemData.Item.GetAttribute("2D", defaultValue: false);
			}
			if (flag)
			{
				if (MyRoomsIntMain.pInstance != null)
				{
					RemoveFloorTexture();
					MyRoomsIntMain.pInstance.AddFloorObject(mCurrentUserItemData, mDragData.AssetName);
					if ((bool)_SndBuildPlaceImage)
					{
						SnChannel.Play(_SndBuildPlaceImage, "SFX_Pool", 0, inForce: false);
					}
					mDragData = null;
					mCurrentUserItemData = null;
				}
			}
			else
			{
				mBuildModeMenu.mNeedPageCheck = true;
				CreateDragObject(mDragData, mDragData.Texture);
			}
		}
		else if (mDragData.HasCategory(367))
		{
			if (MyRoomsIntMain.pInstance != null)
			{
				RemoveCushionTexture();
				MyRoomsIntMain.pInstance.AddCushionObject(mCurrentUserItemData, mDragData.Texture[0].TextureName);
				if ((bool)_SndBuildPlaceImage)
				{
					SnChannel.Play(_SndBuildPlaceImage, "SFX_Pool", 0, inForce: false);
				}
				mDragData = null;
				mCurrentUserItemData = null;
			}
		}
		else if (mDragData.HasCategory(247))
		{
			if (MyRoomsIntMain.pInstance != null)
			{
				MyRoomsIntMain.pInstance.AddWindow(isUpdateServer: true, null, mCurrentUserItemData);
				if ((bool)_SndBuildPlaceImage)
				{
					SnChannel.Play(_SndBuildPlaceImage, "SFX_Pool", 0, inForce: false);
				}
				mDragData = null;
				mCurrentUserItemData = null;
			}
		}
		else if (mDragData.HasCategory(330))
		{
			GameObject pPetBedObject = MyRoomsIntMain.pInstance.pPetBedObject;
			if (pPetBedObject != null)
			{
				UtUtilities.SetLayerRecursively(pPetBedObject, LayerMask.NameToLayer("Ignore Raycast"));
				RemoveObject(pPetBedObject, inDestroy: true);
			}
			if (MyRoomsIntMain.pInstance != null)
			{
				MyRoomsIntMain.pInstance.AddPetBedObject(mCurrentUserItemData, mDragData.AssetName, mDragData.Texture);
			}
		}
		else
		{
			mBuildModeMenu.mNeedPageCheck = true;
			CreateDragObject(mDragData, mDragData.Texture);
		}
	}

	public virtual void CreateDragObject(ItemData itemData, ItemDataTexture[] texture)
	{
		mDragObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromBundle(itemData.AssetName));
		GameObject gameObject = (GameObject)mDragObject;
		if (texture != null && texture.Length != 0)
		{
			Texture t = (Texture)RsResourceManager.LoadAssetFromBundle(texture[0].TextureName);
			UtUtilities.SetObjectTexture(gameObject, 0, t);
		}
		MyRoomsIntMain.pInstance.RemoveUnwantedLayers(gameObject);
		gameObject.name = itemData.AssetName;
		Vector3 mousePosition = KAInput.mousePosition;
		gameObject.transform.position = mousePosition;
		gameObject.transform.rotation = Quaternion.identity;
		MyRoomObject myRoomObject = gameObject.GetComponent<MyRoomObject>();
		if (myRoomObject == null)
		{
			myRoomObject = gameObject.AddComponent<MyRoomObject>();
		}
		BoxCollider component = gameObject.GetComponent<BoxCollider>();
		MeshCollider component2 = gameObject.GetComponent<MeshCollider>();
		if (component != null && component2 != null)
		{
			component.enabled = true;
			component2.enabled = false;
			myRoomObject.collider = component;
		}
		myRoomObject.EnableTrigger(isEnable: true);
		ObClickable obClickable = gameObject.GetComponent<ObClickable>();
		if (obClickable == null)
		{
			obClickable = gameObject.AddComponent<ObClickable>();
			obClickable._AvatarWalkTo = false;
		}
		obClickable._MessageObject = base.gameObject;
		obClickable._Active = false;
		MyRoomsIntMain.EnableRoomObjectsClickable(isEnable: false);
		mPreviousMousePosition = KAInput.mousePosition;
		mIsObjectPlacedFirstTime = true;
		MyRoomsIntMain.pInstance.pSelectedObjectType = myRoomObject._ObjectType;
		myRoomObject.pUserItemData = mCurrentUserItemData;
		MyRoomsIntMain.pInstance.AttachRaycastPoints(gameObject);
		AttachDragProxy(gameObject);
		mParentStackObject = null;
		mWallHitNormal = Vector3.zero;
		mCanPlace = false;
	}

	public GameObject GetDragProxy()
	{
		if (mDragProxy == null)
		{
			mDragProxy = new GameObject("DragProxy");
			mDragProxy.AddComponent<BoxCollider>();
			mDragProxy.GetComponent<Collider>().isTrigger = true;
			mDragProxy.AddComponent<Rigidbody>().isKinematic = true;
			mDragProxy.layer = LayerMask.NameToLayer("DraggedObject");
			mDragProxy.AddComponent<DragProxyTrigger>();
		}
		return mDragProxy;
	}

	private void AttachDragProxy(GameObject inItemObject)
	{
		UtDebug.Assert(inItemObject != null, "ROOM ITEM OBJECT IS NULL!!");
		Collider component = inItemObject.GetComponent<Collider>();
		UtDebug.Assert(component != null, "ROOM ITEM OBJECT COLLIDER IS NULL!!");
		GameObject dragProxy = GetDragProxy();
		Collider component2 = dragProxy.GetComponent<Collider>();
		if (component.GetType().Equals(typeof(MeshCollider)))
		{
			Bounds bounds = ((MeshCollider)component).sharedMesh.bounds;
			((BoxCollider)component2).center = bounds.center;
			((BoxCollider)component2).size = bounds.size;
		}
		else if (component.GetType().Equals(typeof(BoxCollider)))
		{
			((BoxCollider)component2).center = ((BoxCollider)component).center;
			((BoxCollider)component2).size = ((BoxCollider)component).size;
		}
		else
		{
			Debug.LogError("ERROR: COLLIDER IS NOT A MESH OR BOX: " + component.name);
		}
		dragProxy.transform.parent = inItemObject.transform;
		dragProxy.transform.localPosition = Vector3.zero;
		dragProxy.transform.localRotation = Quaternion.identity;
		dragProxy.transform.localScale = Vector3.one;
		dragProxy.SetActive(value: true);
	}

	private void DetachDragProxy(GameObject inItemObject)
	{
		UtDebug.Assert(inItemObject != null, "ROOM ITEM OBJECT IS NULL!!");
		if (mDragProxy != null && mDragProxy.transform.parent == inItemObject.transform)
		{
			mDragProxy.transform.parent = null;
			mDragProxy.SetActive(value: false);
		}
	}

	public void DeleteObject(object inObject, ItemData inData, bool addToInv, int index)
	{
		if (inObject == null)
		{
			return;
		}
		GameObject gameObject = null;
		KAWidget kAWidget = null;
		if (inObject.GetType().Equals(typeof(GameObject)))
		{
			gameObject = (GameObject)inObject;
			MyRoomObject component = gameObject.GetComponent<MyRoomObject>();
			if (component != null)
			{
				if (addToInv)
				{
					AddItemToInventory(mCurrentUserItemData);
					MyRoomObject.ChildData[] pChildList = component.pChildList;
					for (int i = 0; i < pChildList.Length; i++)
					{
						MyRoomObject component2 = pChildList[i]._Child.GetComponent<MyRoomObject>();
						AddItemToInventory(component2.pUserItemData);
					}
				}
				MyRoomsIntMain.pInstance.RemoveRoomObject(gameObject, isDestroy: true);
			}
		}
		DetachDragProxy(gameObject);
		if (gameObject != null)
		{
			Object.Destroy(gameObject);
		}
		else if (kAWidget != null)
		{
			RemoveWidget(kAWidget);
		}
		mDragObject = null;
		mSelectedObject = null;
		if ((bool)_SndBuildReturn)
		{
			SnChannel.Play(_SndBuildReturn, "SFX_Pool", 0, inForce: true);
		}
		UpdateRoomObjectCountText();
	}

	public void AddItemToInventory(UserItemData inData)
	{
		CommonInventoryData.pInstance.AddItem(inData, updateServer: false, 1);
		UpdateCategoryMenu();
	}

	private void UpdateCategoryMenu()
	{
		if (!(mBuildModeCategoryMenu == null))
		{
			int pSelectedItemIndex = mBuildModeCategoryMenu.pSelectedItemIndex;
			int num = mBuildModeMenu.GetTopItemIdx();
			SetCategoryMenu(pSelectedItemIndex);
			UpdateCategory(mPreviousSelectedCategory);
			int numItems = mBuildModeMenu.GetNumItems();
			int numItemsPerPage = mBuildModeMenu.GetNumItemsPerPage();
			int num2 = numItems - numItemsPerPage;
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num > num2)
			{
				num = num2;
			}
			mBuildModeMenu.SetTopItemIdx(num);
		}
	}

	public void MakeObjectPlacementDirty(GameObject go)
	{
		if (go == null)
		{
			UtDebug.LogWarning("!!!!!!!!!!MakeObjectPlacementDirty() - Object passed is null");
			return;
		}
		MyRoomItem component = go.GetComponent<MyRoomItem>();
		if (component == null)
		{
			UtDebug.LogWarning("!!!!!!!!!!MakeObjectPlacementDirty() - Object passed is not a MyRoomItem");
		}
		else
		{
			component.pItemPlacementDirty = true;
		}
	}

	protected void HandleCurrentSelection()
	{
		if (mSelectedObject == null)
		{
			return;
		}
		MyRoomObject component = mSelectedObject.GetComponent<MyRoomObject>();
		if (component == null)
		{
			UtDebug.LogWarning("!!!!!!!!!!HandleCurrentSelection() - Selected Object is not a roomObject");
			return;
		}
		MyRoomItem component2 = mSelectedObject.GetComponent<MyRoomItem>();
		if (component2 == null)
		{
			UtDebug.LogWarning("!!!!!!!!!!HandleCurrentSelection() - Selected Object is not a MyRoomItem");
			return;
		}
		bool flag = false;
		if (component2.pItemPlacementDirty && !mCanPlace)
		{
			if (component2.pItemJustCreated)
			{
				DeleteObject(mSelectedObject, mSelectedObjectData, addToInv: true, 0);
				pSelectedObject = null;
				mSelectedObjectData = null;
				mParentStackObject = null;
				mWallHitNormal = Vector3.zero;
				return;
			}
			flag = AllowUseOldPosition();
		}
		if (component2.pItemJustCreated && MyRoomsIntMain.pInstance.pRoomObjectCount >= _MaxRoomObjectCount)
		{
			if (_MaxRoomObjectVO != null)
			{
				SnChannel.Play(_MaxRoomObjectVO, "VO_Pool", 0, inForce: false);
			}
			return;
		}
		DetachDragProxy(mSelectedObject);
		UtUtilities.SetLayerRecursively(mSelectedObject, LayerMask.NameToLayer("Furniture"));
		component.EnableTrigger(isEnable: false);
		component.ClearCollisionList();
		Transform transform = mParentStackObject;
		GameObject gameObject = null;
		if (transform != null)
		{
			mSelectedObject.transform.parent = transform;
			gameObject = transform.gameObject;
			if (gameObject != null)
			{
				component.pParentObject = gameObject;
				MyRoomObject component3 = gameObject.GetComponent<MyRoomObject>();
				if (component3 != null)
				{
					component3.AddChildReference(mSelectedObject);
				}
			}
			else
			{
				component.pParentObject = null;
			}
		}
		if (component2.pItemJustCreated)
		{
			component2.pItemJustCreated = false;
			MyRoomsIntMain.pInstance.AddRoomObject(mSelectedObject, mCurrentUserItemData, gameObject, isUpdateLocalList: true);
			UpdateRoomObjectCountText();
		}
		else
		{
			if (flag)
			{
				mSelectedObject.transform.position = mDragObjectPreviousPosition;
				mSelectedObject.transform.rotation = mDragObjectPreviousRotation;
				mBuildmodeCam.pGoToPosition = mSelectedObject.transform.position;
			}
			MyRoomsIntMain.pInstance.UpdateRoomObject(mSelectedObject, component.pUserItemData, component.pParentObject);
		}
		ChangeObjectColor(mSelectedObject, new Color(1f, 1f, 1f));
		component2.pItemPlacementDirty = false;
		mParentStackObject = null;
		mWallHitNormal = Vector3.zero;
		mCanPlace = false;
	}

	protected virtual bool AllowUseOldPosition()
	{
		return true;
	}

	public void UseDragObject(GameObject go)
	{
		UseDragObject(go, null);
	}

	public virtual void UseDragObject(GameObject go, Transform parent)
	{
		if (MyRoomsIntMain.pInstance.pRoomObjectCount >= _MaxRoomObjectCount && mIsObjectPlacedFirstTime)
		{
			if (_MaxRoomObjectVO != null)
			{
				SnChannel.Play(_MaxRoomObjectVO, "VO_Pool", 0, inForce: false);
			}
			return;
		}
		if (mDragObject != null && mDragObject.GetType().Equals(typeof(GameObject)))
		{
			GameObject gameObject = (GameObject)mDragObject;
			MyRoomObject component = gameObject.GetComponent<MyRoomObject>();
			if (component != null)
			{
				DetachDragProxy(gameObject);
				UtUtilities.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Furniture"));
				component.EnableTrigger(isEnable: false);
				component.ClearCollisionList();
				go.transform.parent = parent;
				mDragObject = null;
				if (KAInput.pInstance.IsTouchInput())
				{
					mBuildmodeCam.pObjectToFollow = null;
				}
				GameObject gameObject2 = null;
				if (parent != null)
				{
					gameObject2 = parent.gameObject;
				}
				if (gameObject2 != null)
				{
					component.pParentObject = gameObject2;
					gameObject2.GetComponent<MyRoomObject>().AddChildReference(gameObject);
				}
				else
				{
					component.pParentObject = null;
				}
				if (mIsObjectPlacedFirstTime)
				{
					MyRoomsIntMain.pInstance.ObjectCreatedCallback(gameObject, mCurrentUserItemData, inSaved: false);
					MyRoomsIntMain.pInstance.AddRoomObject(gameObject, mCurrentUserItemData, gameObject2, isUpdateLocalList: true);
				}
				else
				{
					MyRoomsIntMain.pInstance.UpdateRoomObject(gameObject, mCurrentUserItemData, gameObject2);
				}
				MyRoomsIntMain.EnableRoomObjectsClickable(isEnable: true);
			}
		}
		if ((bool)_SndBuildPlaceObject)
		{
			SnChannel.Play(_SndBuildPlaceObject, "SFX_Pool", 0, inForce: false);
		}
		UpdateRoomObjectCountText();
		if (go != null)
		{
			GetSelectedObject(go);
			MyRoomItemClickable component2 = go.GetComponent<MyRoomItemClickable>();
			if (component2 != null)
			{
				component2.ProcessMouseUp();
			}
		}
	}

	public virtual void GetSelectedObject(GameObject inObject)
	{
		mIsObjectPlacedFirstTime = false;
		HandleCurrentSelection();
		bool flag = false;
		if (inObject != null)
		{
			MyRoomObject component = inObject.GetComponent<MyRoomObject>();
			if (component != null)
			{
				if (mDragObject == null)
				{
					mDragObjectPreviousPosition = inObject.transform.position;
					mDragObjectPreviousRotation = inObject.transform.rotation;
				}
				component.EnableTrigger(isEnable: true);
				AttachDragProxy(inObject);
				MyRoomsIntMain.pInstance.pSelectedObjectType = component._ObjectType;
				pSelectedObject = inObject;
				flag = true;
				if (component.pUserItemData != null)
				{
					mSelectedObjectData = component.pUserItemData.Item;
					mCurrentUserItemData = component.pUserItemData;
				}
			}
		}
		if (!flag)
		{
			pSelectedObject = null;
		}
	}

	protected void UseSelectedObject(bool useOldPosition)
	{
		if (mSelectedObject != null)
		{
			DetachDragProxy(mSelectedObject);
			UtUtilities.SetLayerRecursively(mSelectedObject, LayerMask.NameToLayer("Furniture"));
			Collider component = mSelectedObject.GetComponent<Collider>();
			if (component != null)
			{
				component.isTrigger = false;
			}
			MyRoomObject component2 = mSelectedObject.GetComponent<MyRoomObject>();
			if (component2 != null)
			{
				component2.ClearCollisionList();
			}
			if (!mCanPlaceDragObject || useOldPosition)
			{
				ChangeObjectColor(mSelectedObject, new Color(1f, 1f, 1f));
				mSelectedObject.transform.position = mDragObjectPreviousPosition;
				mSelectedObject.transform.rotation = mDragObjectPreviousRotation;
				mBuildmodeCam.pGoToPosition = mSelectedObject.transform.position;
			}
			else if (component2 != null)
			{
				MyRoomsIntMain.pInstance.UpdateRoomObject(mSelectedObject, component2.pUserItemData, component2.pParentObject);
			}
			pSelectedObject = null;
			mSelectedObjectData = null;
		}
	}

	private void ChangeObjectColor(GameObject inObj, Color inColor)
	{
		Component[] componentsInChildren = inObj.GetComponentsInChildren<Renderer>();
		componentsInChildren = componentsInChildren;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = ((Renderer)componentsInChildren[i]).materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].color = inColor;
			}
		}
	}

	protected virtual void RotateObject(GameObject inObj, float yRotOffset)
	{
		if (!(inObj != null))
		{
			return;
		}
		MyRoomObject component = inObj.GetComponent<MyRoomObject>();
		if (component != null)
		{
			if (component._ObjectType != 0)
			{
				inObj.transform.Rotate(Vector3.up, yRotOffset);
			}
		}
		else
		{
			inObj.transform.Rotate(Vector3.up, yRotOffset);
		}
	}

	public void CategorySelected(KAWidget item, int index)
	{
		SetPage(item);
	}

	private void SetCategoryMenu(int inSelectedIndex)
	{
		mBuildModeCategoryMenu.ClearItems();
		KAWidget kAWidget = null;
		int num = 0;
		for (int i = 0; i < _CategoryList.Length; i++)
		{
			MyRoomBuilderCategory myRoomBuilderCategory = _CategoryList[i];
			KAWidget kAWidget2 = DuplicateWidget(mBuildModeCategoryMenu._Template);
			kAWidget2.gameObject.SetActive(value: true);
			kAWidget2.SetVisibility(inVisible: true);
			kAWidget2.SetState(KAUIState.INTERACTIVE);
			kAWidget2.name = myRoomBuilderCategory._Category.ToString();
			kAWidget2.SetToolTipTextByID(myRoomBuilderCategory._RollOverText._ID, myRoomBuilderCategory._RollOverText._Text);
			KAWidget kAWidget3 = kAWidget2.FindChildItem("TxtCatName");
			if (kAWidget3 != null)
			{
				kAWidget3.SetTextByID(myRoomBuilderCategory._CategoryDispName._ID, myRoomBuilderCategory._CategoryDispName._Text);
			}
			KAWidget kAWidget4 = kAWidget2.FindChildItem("Icon");
			if (myRoomBuilderCategory._IconTex != null && kAWidget4 != null)
			{
				kAWidget4.SetTexture(myRoomBuilderCategory._IconTex);
			}
			if (myRoomBuilderCategory._RollOverVO != null)
			{
				kAWidget2._TooltipInfo._Sound._AudioClip = myRoomBuilderCategory._RollOverVO;
			}
			mBuildModeCategoryMenu.AddWidget(kAWidget2);
			if (kAWidget == null)
			{
				kAWidget = kAWidget2;
			}
			if (num == inSelectedIndex)
			{
				kAWidget = kAWidget2;
			}
			num++;
		}
		if (inSelectedIndex == -1 || inSelectedIndex >= mBuildModeCategoryMenu.GetNumItems())
		{
			inSelectedIndex = 0;
		}
		if (kAWidget != null)
		{
			SetPage(kAWidget);
			mBuildModeCategoryMenu.OnClick(kAWidget);
			kAWidget.OnClick();
		}
	}

	protected void UpdateRoomObjectCountText()
	{
		if (mTxtRoomCount != null)
		{
			int num = 0;
			if (MyRoomsIntMain.pInstance != null)
			{
				num = MyRoomsIntMain.pInstance.pRoomObjectCount;
			}
			string text = num + "/" + _MaxRoomObjectCount + StringTable.GetStringData(_ItemsText._ID, _ItemsText._Text);
			UILabel componentInChildren = mTxtRoomCount.GetComponentInChildren<UILabel>();
			if (componentInChildren != null)
			{
				componentInChildren.text = text;
			}
		}
	}

	private void SetWallPaperData(ItemData inDragdata, MyRoomBuilderItemData inItemData)
	{
		if (mSelectedWallPaper != null)
		{
			ClearWallPaperData();
		}
		Texture inTexture = (Texture)RsResourceManager.LoadAssetFromBundle(inDragdata.AssetName);
		KAWidget kAWidget = FindItem("WallpaperTemplate");
		mSelectedWallPaper = kAWidget.FindChildItem("PlaceableItem");
		if (mNonplaceableItem == null)
		{
			mNonplaceableItem = kAWidget.FindChildItem("NonplaceableItem");
		}
		mSelectedWallPaper.SetTexture(inTexture);
		if (mSelectedWallPaper != null)
		{
			mSelectedWallPaper.SetUserData(inItemData);
			mSelectedWallPaper.SetVisibility(inVisible: true);
			mSelectedWallPaper.AttachToCursor(0f, 0f);
		}
	}

	private void ClearWallPaperData()
	{
		mSelectedWallPaper.SetTexture(null);
		mSelectedWallPaper.DetachFromCursor();
		mSelectedWallPaper.ClearChildItems();
		mSelectedWallPaper = null;
		mNonplaceableItem.SetVisibility(inVisible: false);
		mNonplaceableItem.SetPosition(0f, 0f);
	}

	private void DeleteAndClearWallpaper()
	{
		if (!MyRoomsIntMain.pInstance.pApplyWallpaperAllWalls)
		{
			if (mCurrentUserItemData != null)
			{
				AddItemToInventory(mCurrentUserItemData);
			}
			ClearWallPaperData();
			mDragData = null;
			mCurrentUserItemData = null;
		}
	}

	private void SetWallPaperAllowed(bool isAllowed)
	{
		if (!isAllowed)
		{
			mNonplaceableItem.SetPosition(mSelectedWallPaper.GetPosition().x, mSelectedWallPaper.GetPosition().y);
		}
		mNonplaceableItem.SetVisibility(!isAllowed);
	}

	public void CheckForDecorateTaskCompletion()
	{
		if ((bool)MissionManager.pInstance)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Build", RsResourceManager.pCurrentLevel, _TaskBuildSpot);
		}
	}

	public Vector3 CalculatePosition(Vector3 inPos, Transform inObj)
	{
		if (MyRoomsIntMain.pInstance == null)
		{
			return inPos;
		}
		return MyRoomsIntMain.pInstance.CalculatePosition(inPos, inObj);
	}

	public Vector3 CalculatePosition(Vector3 inPos)
	{
		return CalculatePosition(inPos, null);
	}

	public virtual bool CanPlaceDragObject(GameObject inObj, UserItemData inUserItemData)
	{
		return true;
	}

	public virtual bool IsDependentObject(GameObject inObj)
	{
		return false;
	}

	public void UpdateCreativePointsProgress()
	{
		int pCurrentCreativePoints = MyRoomsIntMain.pInstance.pCurrentCreativePoints;
		int pMaxCreativePoints = MyRoomsIntMain.pInstance.pMaxCreativePoints;
		if (mTxtCreativePointsLabel != null && mAniCreativePointsProgress != null)
		{
			if (pCurrentCreativePoints >= pMaxCreativePoints)
			{
				mAniCreativePointsProgress.GetProgressBar().UpdateSprite(_CreativePointsProgressFullSprite);
			}
			else
			{
				mAniCreativePointsProgress.GetProgressBar().UpdateSprite(_CreativePointsProgressSprite);
			}
			mAniCreativePointsProgress.SetProgressLevel((float)pCurrentCreativePoints / (float)pMaxCreativePoints);
			mTxtCreativePointsLabel.SetText(pCurrentCreativePoints + "/" + pMaxCreativePoints);
		}
	}

	public virtual bool CheckStacking(GameObject inRoomObj, GameObject inUnderlyingObj)
	{
		MyRoomObject component = inRoomObj.GetComponent<MyRoomObject>();
		if (component == null)
		{
			return false;
		}
		MyRoomObject component2 = inUnderlyingObj.GetComponent<MyRoomObject>();
		if (component2 == null)
		{
			return false;
		}
		if (component._IsStackableObject && component2 != null)
		{
			return component2._CanStackObject;
		}
		return false;
	}

	public virtual void OnDragObject(bool isDragging)
	{
		mBuildmodeCam._EnableCameraMovement = !isDragging;
	}

	public virtual void OnDrag(GameObject inObj)
	{
	}
}
