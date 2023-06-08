using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class FarmGridManager : GridManager
{
	[Serializable]
	public class GridLevelInfo
	{
		public int _Level;

		public int _Value;

		public int _CostPerSlot = 10;
	}

	public List<string> _BuildModeStateSensitiveMenuItems;

	public List<string> _NonBuildModeStateSensitiveMenuItems;

	public List<string> _BuildModeSelectSensitiveMenuItems;

	public List<string> _NonBuildModeSelectSensitiveMenuItems;

	public int _GridCellItemID;

	public int _StoreID;

	public int _GridCellPrice = 10;

	public GUISkin _GridPriceSkin;

	public LocaleString _FreeFarmSlotText = new LocaleString("Do you want to take this free Farm slot?");

	public LocaleString _CellPurchaseConfirmMsg = new LocaleString("Do you want to buy this farm slot?");

	public LocaleString _GenericErrorMsg = new LocaleString("Sorry!Unable to purchase farm slot now. Please try after some time.");

	public LocaleString _AlreadyPurchasedMsg = new LocaleString("You have already purchased this farm slot. Do you want to remove it?");

	public LocaleString _LevelLockedMsg = new LocaleString("You have already purchased all the farm slots available. Level up to purchase more.");

	public LocaleString _NoMoneyMsg = new LocaleString("You don't have enough money to make this purchase");

	public LocaleString _FarmingDBTitleText = new LocaleString("Farming");

	public LocaleString _SlotBlockedMsg = new LocaleString("To purchase this farm slot, first clear it of other objects.");

	public LocaleString _SlotInfoText = new LocaleString("Farm slots available %unlocked%/%total%@@Next upgrade is level %nextLevel%");

	public GridLevelInfo[] _SlotsAllowed;

	public KAWidget _SlotInfoItem;

	public TextMesh _3DText;

	public Vector2[] _DefaultAllowedGridCell;

	public FarmManager _FarmManager;

	public const string UNLOCKED = "UnlockedGC";

	public int _SeedsCategory = 393;

	public FarmItemContextData _ContextSensitiveData;

	private ObClickable mClickable;

	private GridCell mCurSelectedGridCell;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mIsBuildMode;

	private UserItemData mCurrentUserItemData;

	public KAUIGenericDB pKAUIGenericDB => mKAUIGenericDB;

	public int pCurrentLevel
	{
		get
		{
			if (UserRankData.pIsReady)
			{
				UserRank userRankByType = UserRankData.GetUserRankByType(9);
				if (userRankByType != null)
				{
					return Mathf.Max(1, userRankByType.RankID);
				}
			}
			return 1;
		}
	}

	public ObClickable pClickable
	{
		get
		{
			if (mClickable == null && _GridCellHighlightObject != null)
			{
				mClickable = _GridCellHighlightObject.GetComponent<ObClickable>();
			}
			return mClickable;
		}
	}

	protected override void Start()
	{
		base.Start();
		HideHighlight();
		_UIFollowingTarget = base.gameObject;
		(MyRoomsIntMain.pInstance as FarmManager).UpdateAchievementUI();
	}

	public void SetSlotInfoText()
	{
		GridLevelInfo gridLevelInfo = GetGridLevelInfo(pCurrentLevel);
		if (gridLevelInfo != null && _SlotInfoItem != null)
		{
			int unlockedGridCount = GetUnlockedGridCount();
			string[] array = Regex.Split(_SlotInfoText.GetLocalizedString(), "@@");
			array[0] = array[0].Replace("%unlocked%", unlockedGridCount.ToString());
			array[0] = array[0].Replace("%total%", gridLevelInfo._Value.ToString());
			int nextUpgradedLevel = GetNextUpgradedLevel(gridLevelInfo._Value);
			array[1] = ((nextUpgradedLevel != -1) ? array[1].Replace("%nextLevel%", nextUpgradedLevel.ToString()) : "");
			_SlotInfoItem.SetText(array[0] + "\n" + array[1]);
		}
	}

	public void SetBuildMode(bool inBuildMode)
	{
		mIsBuildMode = inBuildMode;
		if (!mIsBuildMode)
		{
			CloseMenu();
		}
	}

	public void ShowHighlight(GridCell inGridCell)
	{
		if (inGridCell != null && !(_GridCellHighlightObject == null))
		{
			_GridCellHighlightObject.SetActive(value: true);
			_GridCellHighlightObject.transform.position = GetGridCellPosition(inGridCell);
		}
	}

	public void ShowGridLocked3DText(Vector3 inPos)
	{
		GridLevelInfo gridLevelInfo = GetGridLevelInfo(pCurrentLevel);
		if (gridLevelInfo == null)
		{
			Show3DText("Locked", inPos);
		}
		int unlockedGridCount = GetUnlockedGridCount();
		string text = unlockedGridCount + "/" + Mathf.Max(unlockedGridCount, gridLevelInfo._Value);
		Show3DText("Locked. Used " + text, inPos);
	}

	public void ShowGridBuy3DText(Vector3 inPos)
	{
		GridLevelInfo gridLevelInfo = GetGridLevelInfo(pCurrentLevel);
		if (gridLevelInfo == null)
		{
			Show3DText("Take this for free!", inPos);
			return;
		}
		int unlockedGridCount = GetUnlockedGridCount();
		string text = unlockedGridCount + "/" + Mathf.Max(unlockedGridCount, gridLevelInfo._Value);
		Show3DText("Used " + text + "  Take this for free!", inPos);
	}

	public void Hide3DText()
	{
		if (!(_3DText == null))
		{
			_3DText.text = string.Empty;
			_3DText.GetComponent<Renderer>().enabled = false;
			_3DText.transform.parent.position = Vector3.one * -5000f;
		}
	}

	public void Show3DText(string inText, Vector3 inPosition)
	{
		if (!(_3DText == null))
		{
			_3DText.text = inText;
			_3DText.transform.parent.position = inPosition;
			_3DText.GetComponent<Renderer>().enabled = true;
			_3DText.transform.parent.LookAt(AvAvatar.pAvatarCamTransform);
		}
	}

	public void HideHighlight()
	{
		if (!(_GridCellHighlightObject == null))
		{
			_GridCellHighlightObject.SetActive(value: false);
		}
	}

	public GridCell GetGridCellfromPoint(Vector3 inPoint, float inThreasholdHeight)
	{
		GridCell gridCellfromPoint = base.GetGridCellfromPoint(inPoint);
		if (gridCellfromPoint == null)
		{
			return null;
		}
		if (Mathf.Abs(GetGridCellPosition(gridCellfromPoint).y - inPoint.y) > inThreasholdHeight)
		{
			return null;
		}
		return gridCellfromPoint;
	}

	public void OnClick(GameObject inObject)
	{
		if (inObject == _GridCellHighlightObject)
		{
			mCurSelectedGridCell = GetGridCellfromPoint(_GridCellHighlightObject.transform.position);
		}
	}

	private void UpdatePlantSubmenu()
	{
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_SeedsCategory);
		List<string> list = new List<string>();
		if (items != null && items.Length != 0)
		{
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				list.Add(userItemData.Item.ItemName);
				ContextData contextData = new ContextData();
				contextData._Name = userItemData.Item.ItemName;
				contextData._DisplayName = new LocaleString(userItemData.Item.ItemName);
				contextData._DisplayName._Text = userItemData.Item.ItemName + "(" + userItemData.Quantity + ")";
				contextData._DeactivateOnClick = true;
				if (!_DataList.Contains(contextData))
				{
					AddIntoDataList(contextData);
				}
			}
		}
		foreach (ContextData data in _DataList)
		{
			if (data._Name.Equals("Plant"))
			{
				data._ChildrenNames = list.ToArray();
			}
		}
		UpdateChildrenData();
	}

	public bool CanUnlockGridCellOnLevel()
	{
		GridLevelInfo gridLevelInfo = GetGridLevelInfo(pCurrentLevel);
		if (gridLevelInfo != null)
		{
			return GetUnlockedGridCount() < gridLevelInfo._Value;
		}
		return false;
	}

	public bool IsRemovableOccupied(GridCell inCell)
	{
		if (inCell == null || inCell._ItemOnGrids == null || inCell._ItemOnGrids.Count <= 0)
		{
			return false;
		}
		foreach (GridItemData itemOnGrid in inCell._ItemOnGrids)
		{
			if (itemOnGrid._Object.GetComponent<Removable>() != null)
			{
				return true;
			}
		}
		foreach (GridItemData itemOnGrid2 in inCell._ItemOnGrids)
		{
			if (itemOnGrid2._Object.GetComponent<AnimalPenFarmItem>() != null)
			{
				return true;
			}
		}
		return false;
	}

	public GridLevelInfo GetGridLevelInfo(int inLevel)
	{
		if (_SlotsAllowed == null || _SlotsAllowed.Length == 0)
		{
			return null;
		}
		GridLevelInfo[] slotsAllowed = _SlotsAllowed;
		foreach (GridLevelInfo gridLevelInfo in slotsAllowed)
		{
			if (gridLevelInfo != null && gridLevelInfo._Level == inLevel)
			{
				return gridLevelInfo;
			}
		}
		return null;
	}

	private int GetNextUpgradedLevel(int curSlotValue)
	{
		if (_SlotsAllowed == null || _SlotsAllowed.Length == 0)
		{
			return -1;
		}
		GridLevelInfo[] slotsAllowed = _SlotsAllowed;
		foreach (GridLevelInfo gridLevelInfo in slotsAllowed)
		{
			if (gridLevelInfo != null && gridLevelInfo._Value > curSlotValue)
			{
				return gridLevelInfo._Level;
			}
		}
		return -1;
	}

	private void IsCropSelected(string inActionName)
	{
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_SeedsCategory);
		if (items == null || items.Length == 0)
		{
			return;
		}
		UserItemData[] array = items;
		foreach (UserItemData userItemData in array)
		{
			if (userItemData.Item.ItemName.Equals(inActionName))
			{
				mCurrentUserItemData = userItemData;
				string[] separator = new string[1] { "/" };
				string[] array2 = userItemData.Item.AssetName.Split(separator, StringSplitOptions.None);
				RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], CropEventHandler, typeof(GameObject));
				CloseMenu();
			}
		}
	}

	private void CropEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			Vector3 position = new Vector3(mCurSelectedGridCell._GridCellRect.center.x, base.transform.position.y, mCurSelectedGridCell._GridCellRect.center.y);
			MyRoomsIntMain.pInstance.ObjectCreatedCallback(gameObject, mCurrentUserItemData, inSaved: false);
			UtUtilities.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Furniture"));
			_FarmManager.AddRoomObject(gameObject, mCurrentUserItemData, null, isUpdateLocalList: true);
			gameObject.transform.position = position;
			MyRoomsIntMain.pInstance.UpdateRoomObject(gameObject, mCurrentUserItemData, null);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Item could not be downloaded.");
			break;
		}
	}

	protected virtual void OnContextAction(string inActionName)
	{
		IsCropSelected(inActionName);
	}
}
