using UnityEngine;

public class UiLandscapeMode : UiMyRoomBuilder
{
	public int _SheepPenInventoryID = 7233;

	public Vector3 _PlaceablePosition = new Vector3(-6.205931f, 0f, -4.564052f);

	public GameObject _ObjectCamera;

	public float _MoveChangeOffset = 1f;

	public float _MoveDelayForObjDrag = 4000f;

	public GameObject[] _BuildModeColliderDisableObjs;

	private int mSheepPenInventoryCount;

	private float mObjectDragDelayTimer;

	public Vector3 _BoundingBoxCenter = new Vector3(-5.2f, 0f, -4f);

	public Vector3 _BoundingBoxSize = new Vector3(30f, 1f, 21f);

	public float _PanBoundaryInPixels = 200f;

	public float _PanSpeed = 8f;

	public LocaleString _RemoveAllItemsWarningText = new LocaleString("Are you sure you want to remove all farm items ?");

	private KAWidget mBtnReset;

	public FarmGridManager pGridManager
	{
		get
		{
			if (pFarmManager == null)
			{
				return null;
			}
			return pFarmManager._GridManager;
		}
	}

	public FarmManager pFarmManager
	{
		get
		{
			if (MyRoomsIntMain.pInstance == null)
			{
				return null;
			}
			return (FarmManager)MyRoomsIntMain.pInstance;
		}
	}

	public override GameObject pSelectedObject
	{
		get
		{
			return base.pSelectedObject;
		}
		set
		{
			base.pSelectedObject = value;
			OnSetSelectedObject(value);
		}
	}

	private void OnSetSelectedObject(GameObject inSelectedObject)
	{
		if (inSelectedObject != null && inSelectedObject == mPrevSelectedObject)
		{
			return;
		}
		if (inSelectedObject != null)
		{
			EnableCamera(isEnableMovEffects: true, isEnableCamControls: false);
			Transform inSourceTransform = null;
			if (mPrevSelectedObject == null)
			{
				inSourceTransform = _AvatarCamera.transform;
			}
			_ObjectCamera.GetComponent<ObjectCam>().MoveTo(inSourceTransform, inSelectedObject.transform, base.gameObject);
		}
		else
		{
			EnableCamera(isEnableMovEffects: true, isEnableCamControls: false);
			_ObjectCamera.GetComponent<ObjectCam>().MoveTo(_AvatarCamera.transform, base.gameObject, isMoveBack: true, calculateRelativeOffset: true);
		}
	}

	public override void ShowUI()
	{
		base.ShowUI();
		mBtnReset = FindItem("BtnReset");
		UpdateCreativePointsProgress();
	}

	private void OnTargetReached(bool isMoveBack)
	{
		if (isMoveBack)
		{
			EnableCamera(isEnableMovEffects: false, isEnableCamControls: false);
		}
		else
		{
			EnableCamera(isEnableMovEffects: false, isEnableCamControls: true);
		}
	}

	private void EnableCamera(bool isEnableMovEffects, bool isEnableCamControls)
	{
		if (_ObjectCamera != null)
		{
			_ObjectCamera.SetActive(isEnableMovEffects || isEnableCamControls);
			_ObjectCamera.GetComponent<ObjectCam>().enabled = isEnableMovEffects;
			ObjectCamController component = _ObjectCamera.GetComponent<ObjectCamController>();
			if (pSelectedObject != null)
			{
				component._LookAtTransform = pSelectedObject.transform;
			}
			component.enabled = isEnableCamControls;
		}
		if (_AvatarCamera != null)
		{
			_AvatarCamera.SetActive(!isEnableMovEffects && !isEnableCamControls);
			AvAvatar.pObject.GetComponent<AvAvatarController>().enabled = _AvatarCamera.activeSelf;
		}
	}

	public override void GetSelectedObject(GameObject inObject)
	{
		base.GetSelectedObject(inObject);
		HighlightSelectedFarmItem();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (CommonInventoryData.pInstance != null)
		{
			mSheepPenInventoryCount = CommonInventoryData.pInstance.GetQuantity(_SheepPenInventoryID);
		}
		if (InteractiveTutManager._CurrentActiveTutorialObject != null && InteractiveTutManager._CurrentActiveTutorialObject == pFarmManager._AnimalTutorial.gameObject && CommonInventoryData.pInstance != null)
		{
			KAWidget kAWidget = FindItem("StoreBtn");
			if (kAWidget != null)
			{
				kAWidget.SetState(KAUIState.DISABLED);
			}
		}
		if (pGridManager != null)
		{
			pGridManager.SetSlotInfoText();
		}
		GameObject[] buildModeColliderDisableObjs = _BuildModeColliderDisableObjs;
		for (int i = 0; i < buildModeColliderDisableObjs.Length; i++)
		{
			Collider component = buildModeColliderDisableObjs[i].GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
		if (SocialBoxManager.pInstance != null && SocialBoxManager.pInstance._SocialBox != null)
		{
			Collider component2 = SocialBoxManager.pInstance._SocialBox.GetComponent<Collider>();
			if (component2 != null)
			{
				component2.enabled = false;
			}
		}
	}

	public override bool CanPlaceDragObject(GameObject inObj, UserItemData inUserItemData)
	{
		if (inObj == null || inUserItemData == null || pGridManager == null)
		{
			return false;
		}
		FarmItem component = inObj.GetComponent<FarmItem>();
		if (component == null)
		{
			return true;
		}
		return pGridManager.CanPlaceItem(inObj, !component._CanPlaceOnNotPurchasedSlot, !component._CanPlaceOnPurchasedSlot);
	}

	public override bool IsDependentObject(GameObject inObj)
	{
		if (pFarmManager == null || inObj == null)
		{
			return false;
		}
		FarmItem component = inObj.GetComponent<FarmItem>();
		if (!(component == null))
		{
			return component.IsDependent();
		}
		return false;
	}

	private bool CanPlaceDependentObject(GameObject inDependentObj, GameObject inObj)
	{
		if (inDependentObj == null)
		{
			return false;
		}
		FarmItem component = inDependentObj.GetComponent<FarmItem>();
		if (component == null || !component.IsDependent())
		{
			return false;
		}
		FarmItem component2 = inObj.GetComponent<FarmItem>();
		if (component2 == null || !component2.CanPlaceDependentObject(component))
		{
			return false;
		}
		return true;
	}

	public override bool HandleFloorItems(MyRoomObject roomObj, GameObject dragObject, ref bool isObjectStacked)
	{
		if (!base.HandleFloorItems(roomObj, dragObject, ref isObjectStacked))
		{
			return false;
		}
		FarmItem component = dragObject.GetComponent<FarmItem>();
		if (component == null || !component._SnapToGrids)
		{
			return true;
		}
		isObjectStacked = false;
		return pGridManager.CanPlaceItem(dragObject, !component._CanPlaceOnNotPurchasedSlot, !component._CanPlaceOnPurchasedSlot);
	}

	public override bool HandleDependentObject(GameObject dragObject, ref bool isObjectStacked)
	{
		isObjectStacked = false;
		mParentStackObject = null;
		if (pGridManager == null)
		{
			return false;
		}
		GridCell gridCellfromPoint = pGridManager.GetGridCellfromPoint(dragObject.transform.position, 0.25f);
		if (gridCellfromPoint == null)
		{
			return false;
		}
		foreach (GridItemData itemOnGrid in gridCellfromPoint._ItemOnGrids)
		{
			if (itemOnGrid == null || itemOnGrid._Object == null)
			{
				return false;
			}
			if (CanPlaceDependentObject(dragObject, itemOnGrid._Object))
			{
				mParentStackObject = itemOnGrid._Object.transform;
				isObjectStacked = true;
				return true;
			}
			FarmItem component = itemOnGrid._Object.GetComponent<FarmItem>();
			if (component != null && component.pParent != null && CanPlaceDependentObject(dragObject, component.pParent.gameObject))
			{
				mParentStackObject = component.pParent.transform;
				isObjectStacked = true;
				return true;
			}
		}
		return false;
	}

	public override bool CheckStacking(GameObject inRoomObj, GameObject inUnderlyingObj)
	{
		if (inRoomObj == null || inUnderlyingObj == null)
		{
			return false;
		}
		FarmItem component = inRoomObj.GetComponent<FarmItem>();
		FarmItem component2 = inUnderlyingObj.GetComponent<FarmItem>();
		if (component == null || component2 == null)
		{
			return base.CheckStacking(inRoomObj, inUnderlyingObj);
		}
		if (component2.pUserItemData == null || component2.pUserItemData.Item == null || component._StackableItems == null || component._StackableItems.Length == 0)
		{
			return base.CheckStacking(inRoomObj, inUnderlyingObj);
		}
		int[] stackableItems = component._StackableItems;
		foreach (int num in stackableItems)
		{
			if (component2.pUserItemData.Item.ItemID == num)
			{
				return true;
			}
		}
		return base.CheckStacking(inRoomObj, inUnderlyingObj);
	}

	private void HandleUnlockedGridCells()
	{
		if (pGridManager == null || Camera.main == null)
		{
			return;
		}
		if (mDragObject != null || pFarmManager == null || !pFarmManager.pIsBuildMode || KAUI.GetGlobalMouseOverItem() != null)
		{
			if (pGridManager != null)
			{
				pGridManager.HideHighlight();
				pGridManager.Hide3DText();
			}
		}
		else
		{
			if (mPreviousMousePosition == KAInput.mousePosition || pGridManager.pKAUIGenericDB != null)
			{
				return;
			}
			int layerMask = ~((1 << LayerMask.NameToLayer("Avatar")) | (1 << LayerMask.NameToLayer("MMOAvatar")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("DraggedObject")) | (1 << LayerMask.NameToLayer("IgnoreGroundRay")));
			if (!Physics.Raycast(Camera.main.ScreenPointToRay(KAInput.mousePosition), out var hitInfo, float.PositiveInfinity, layerMask))
			{
				pGridManager.HideHighlight();
				pGridManager.Hide3DText();
				return;
			}
			if (hitInfo.collider == null || hitInfo.collider.gameObject.layer != LayerMask.NameToLayer("Floor"))
			{
				pGridManager.HideHighlight();
				pGridManager.Hide3DText();
				return;
			}
			if (KAUICamera.IsHovering() && UICamera.hoveredObject.layer == LayerMask.NameToLayer("3DNGUI"))
			{
				pGridManager.HideHighlight();
				pGridManager.Hide3DText();
				return;
			}
			GridCell gridCellfromPoint = pGridManager.GetGridCellfromPoint(hitInfo.point);
			if (gridCellfromPoint == null || gridCellfromPoint._IsUnlocked || (gridCellfromPoint._ItemOnGrids != null && gridCellfromPoint._ItemOnGrids.Count > 0))
			{
				pGridManager.HideHighlight();
				pGridManager.Hide3DText();
			}
			else if (gridCellfromPoint._IsUnlocked)
			{
				if (gridCellfromPoint._ItemOnGrids != null && gridCellfromPoint._ItemOnGrids.Count > 0)
				{
					pGridManager.HideHighlight();
				}
				else
				{
					ShowGridHighlight(gridCellfromPoint);
				}
				pGridManager.Hide3DText();
			}
			else if (!pGridManager.CanUnlockGridCellOnLevel())
			{
				ShowGridHighlight(gridCellfromPoint);
			}
			else if (gridCellfromPoint._IsUnlocked)
			{
				pGridManager.HideHighlight();
				pGridManager.Hide3DText();
			}
			else
			{
				ShowGridHighlight(gridCellfromPoint);
			}
		}
	}

	private void ShowGridHighlight(GridCell inGridCell)
	{
		if (pGridManager != null && _ObjectCamera != null && !_ObjectCamera.activeSelf)
		{
			pGridManager.ShowHighlight(inGridCell);
		}
	}

	public void OnDisable()
	{
		if (pGridManager != null)
		{
			pGridManager.HideHighlight();
			pGridManager.Hide3DText();
		}
		GetSelectedObject(null);
		EnableCamera(isEnableMovEffects: false, isEnableCamControls: false);
		GameObject[] buildModeColliderDisableObjs = _BuildModeColliderDisableObjs;
		for (int i = 0; i < buildModeColliderDisableObjs.Length; i++)
		{
			Collider component = buildModeColliderDisableObjs[i].GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
		if (SocialBoxManager.pInstance != null && SocialBoxManager.pInstance._SocialBox != null)
		{
			Collider component2 = SocialBoxManager.pInstance._SocialBox.GetComponent<Collider>();
			if (component2 != null)
			{
				component2.enabled = true;
			}
		}
	}

	public void OnOutOfBuildMode(bool saveSuccess)
	{
		if (saveSuccess && InteractiveTutManager._CurrentActiveTutorialObject != null && InteractiveTutManager._CurrentActiveTutorialObject == pFarmManager._AnimalTutorial.gameObject && CommonInventoryData.pInstance != null && CommonInventoryData.pInstance.GetQuantity(_SheepPenInventoryID) < mSheepPenInventoryCount)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "PlacedSheepPen");
		}
	}

	protected override void OnDragStart()
	{
		ObjectCam component = _ObjectCamera.GetComponent<ObjectCam>();
		if (component != null && component.IsMoving())
		{
			component.SetMoveDone(isDone: true);
		}
		base.OnDragStart();
		mObjectDragDelayTimer = _MoveDelayForObjDrag;
		_ObjectCamera.GetComponent<ObjectCamController>().enabled = false;
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		_ObjectCamera.GetComponent<ObjectCamController>().enabled = true;
		ObjectCam component = _ObjectCamera.GetComponent<ObjectCam>();
		if (component != null && pSelectedObject != null && mObjectDragDelayTimer <= 0f)
		{
			EnableCamera(isEnableMovEffects: true, isEnableCamControls: false);
			bool calculateRelativeOffset = pSelectedObject != mPrevSelectedObject;
			component.MoveTo(pSelectedObject.transform, base.gameObject, isMoveBack: false, calculateRelativeOffset);
		}
	}

	protected override void Update()
	{
		PanCamera();
		if (mObjectDragDelayTimer > 0f)
		{
			mObjectDragDelayTimer -= Time.deltaTime * 1000f;
		}
		if (mObjectDragDelayTimer < 0f && mDragObject == null && pSelectedObject != null)
		{
			EnableCamera(isEnableMovEffects: true, isEnableCamControls: false);
			_ObjectCamera.GetComponent<ObjectCam>().MoveTo(pSelectedObject.transform, base.gameObject, isMoveBack: false, calculateRelativeOffset: false);
			mObjectDragDelayTimer = 0f;
		}
		HandleUnlockedGridCells();
		base.Update();
		if (base.pInitialized && pFarmManager != null && pFarmManager._BuildmodeTutorial != null && pFarmManager.pIsBuildMode)
		{
			pFarmManager._BuildmodeTutorial.enabled = true;
			if (!pFarmManager._BuildmodeTutorial.TutorialComplete() && InteractiveTutManager._CurrentActiveTutorialObject == null)
			{
				pFarmManager._BuildmodeTutorial.ShowTutorial();
				pFarmManager._BuildmodeTutorial.StartNextTutorial();
			}
		}
	}

	public override void HandleObjectPlacement(bool canPlace, GameObject dragObject, bool isObjectStacked)
	{
		if (!KAUICamera.IsHovering())
		{
			base.HandleObjectPlacement(canPlace, dragObject, isObjectStacked);
		}
	}

	private void PanCamera()
	{
		ObjectCam component = _ObjectCamera.GetComponent<ObjectCam>();
		if (!(pSelectedObject == null) && mDragObject != null && !component.IsMoving())
		{
			Vector3 inCamPosition = component.transform.position;
			Vector3 forward = component.transform.forward;
			forward.y = 0f;
			Vector3 right = component.transform.right;
			right.y = 0f;
			if (KAInput.mousePosition.x > 0f && KAInput.mousePosition.x < _PanBoundaryInPixels)
			{
				inCamPosition -= right * Time.deltaTime * _PanSpeed;
			}
			else if (KAInput.mousePosition.x >= (float)Screen.width - _PanBoundaryInPixels)
			{
				inCamPosition += right * Time.deltaTime * _PanSpeed;
			}
			else if (KAInput.mousePosition.y > 0f && KAInput.mousePosition.y <= _PanBoundaryInPixels)
			{
				inCamPosition -= forward * Time.deltaTime * _PanSpeed;
			}
			else if (KAInput.mousePosition.y >= (float)Screen.height - _PanBoundaryInPixels)
			{
				inCamPosition += forward * Time.deltaTime * _PanSpeed;
			}
			PushCameraInsideBounds(ref inCamPosition);
			_ObjectCamera.GetComponent<ObjectCamController>().enabled = false;
			_ObjectCamera.transform.position = inCamPosition;
			_ObjectCamera.GetComponent<ObjectCamController>().enabled = true;
		}
	}

	private void PushCameraInsideBounds(ref Vector3 inCamPosition)
	{
		Vector3 vector = _BoundingBoxCenter - _BoundingBoxSize / 2f;
		Vector3 vector2 = _BoundingBoxCenter + _BoundingBoxSize / 2f;
		inCamPosition.x = Mathf.Clamp(inCamPosition.x, vector.x, vector2.x);
		inCamPosition.z = Mathf.Clamp(inCamPosition.z, vector.z, vector2.z);
	}

	private void OnDrawGizmos()
	{
		if (Application.isEditor)
		{
			Gizmos.color = new Color(255f, 0f, 0f);
			Gizmos.DrawWireCube(_BoundingBoxCenter, _BoundingBoxSize);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnReset)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _RemoveAllItemsWarningText.GetLocalizedString(), "", base.gameObject, "OnRemoveAllItems", "OnPressNO", "", "", inDestroyOnClick: true);
		}
	}

	public void OnRemoveAllItems()
	{
		if (mDragObject != null)
		{
			DeleteObject(mDragObject, mDragData, addToInv: true, 0);
		}
		pFarmManager.RemoveAllItems(FarmManager.pCurrentFarmData.RoomID);
		if (pSelectedObject != null)
		{
			pSelectedObject = null;
		}
	}

	public override void OnClickAction(string inActionName)
	{
		base.OnClickAction(inActionName);
		switch (inActionName)
		{
		case "ObjectMoveBtn":
			if (mDragObject != null && pGridManager != null)
			{
				pGridManager.ClearItemFromGridCells((GameObject)mDragObject);
			}
			break;
		case "ObjectRotateLtBtn":
		case "ObjectRotateRtBtn":
			if (mSelectedObject != null && pGridManager != null)
			{
				pGridManager.ClearItemFromGridCells(mSelectedObject);
			}
			break;
		case "ObjectPickUpBtn":
			EnableCamera(isEnableMovEffects: true, isEnableCamControls: false);
			_ObjectCamera.GetComponent<ObjectCam>().MoveTo(_AvatarCamera.transform, base.gameObject, isMoveBack: true, calculateRelativeOffset: true);
			break;
		}
	}

	public void UpdateItemQuantity()
	{
		if (mBuildModeMenu == null)
		{
			return;
		}
		for (int i = 0; i < mBuildModeMenu.GetNumItems(); i++)
		{
			KAWidget kAWidget = mBuildModeMenu.FindItemAt(i);
			if (!(kAWidget == null) && kAWidget.GetUserData() != null)
			{
				((MyRoomBuilderItemData)kAWidget.GetUserData())?.UpdateQuantity();
			}
		}
	}

	public override void CreateDragObject(ItemData itemData, ItemDataTexture[] texture)
	{
		base.CreateDragObject(itemData, texture);
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "PlacedDecorationItem");
		}
	}

	protected override void RotateObject(GameObject inObj, float yRotOffset)
	{
		if (!(inObj == null))
		{
			Collider component = inObj.GetComponent<Collider>();
			if (!(component == null))
			{
				inObj.transform.RotateAround(component.bounds.center, Vector3.up, yRotOffset);
			}
		}
	}

	protected override void OnClick(GameObject inObject)
	{
		if (inObject != null)
		{
			FarmItemBase component = inObject.GetComponent<FarmItemBase>();
			if (component != null && component.pParent != null)
			{
				base.OnClick(component.pParent.gameObject);
				return;
			}
		}
		base.OnClick(inObject);
	}

	public void HighlightSelectedFarmItem()
	{
		if (pFarmManager != null)
		{
			foreach (FarmItem pFarmItem in pFarmManager.pFarmItems)
			{
				if (pFarmItem != null)
				{
					pFarmItem.HighlightObject(canShowHightlight: false);
				}
			}
		}
		if (mSelectedObject != null)
		{
			FarmItemBase component = mSelectedObject.GetComponent<FarmItemBase>();
			if (component != null)
			{
				component.HighlightObject(canShowHightlight: true);
			}
		}
	}

	protected override void OnPress(GameObject inObject)
	{
		if (inObject == pSelectedObject)
		{
			return;
		}
		if (inObject != null)
		{
			FarmItemBase component = inObject.GetComponent<FarmItemBase>();
			if (component != null && component.pParent != null)
			{
				base.OnPress(component.pParent.gameObject);
				return;
			}
		}
		base.OnPress(inObject);
	}
}
