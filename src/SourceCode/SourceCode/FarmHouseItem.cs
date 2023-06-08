using System;
using UnityEngine;

public class FarmHouseItem : FarmItem
{
	[Serializable]
	public class FarmHouseUpdgradeData
	{
		public int ItemID;

		public int MinUserRank;
	}

	public FarmHouseUpdgradeData[] _FarmHouseUpdgradeInfo;

	public TextMesh _3dText;

	public int _FarmHouseLevel = 1;

	private int mUpgradeFarmHouseItemId;

	private bool mIsUpgradeAvailable;

	private bool mIsTextSet;

	public override void ProcessCurrentStage()
	{
	}

	public override void ProcessClick()
	{
		if (mIsUpgradeAvailable)
		{
			base.pFarmManager.RemoveRoomObject(base.gameObject, isDestroy: false);
			UserItemPositionList.Save(base.pFarmManager.pCurrentRoomID, OnUserItemPositionEvent);
		}
	}

	private bool IsNextFarmHouseLevelAvailable(int currentFarmHouseLevel, out int nextFarmhouseItemId)
	{
		nextFarmhouseItemId = -1;
		int rankID = UserRankData.GetUserRankByType(9).RankID;
		int num = currentFarmHouseLevel + 1;
		if (num < _FarmHouseUpdgradeInfo.Length && rankID >= _FarmHouseUpdgradeInfo[num].MinUserRank)
		{
			nextFarmhouseItemId = _FarmHouseUpdgradeInfo[num].ItemID;
			return true;
		}
		return false;
	}

	private void OnUserItemPositionEvent(WsServiceType inType, WsServiceEvent inEvent, string inRoomID)
	{
		CommonInventoryData.pInstance.AddItem(mUpgradeFarmHouseItemId, updateServer: false);
		CommonInventoryData.pInstance.Save(ReplaceFarmHouseInventoryHandler, null);
	}

	public void ReplaceFarmHouseInventoryHandler(bool success, object inUserData)
	{
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(mUpgradeFarmHouseItemId);
		if (userItemData != null)
		{
			base.pFarmManager.AddRoomObject(new GameObject(), userItemData, new Vector3(-5f, 0f, 2f), new Vector3(0f, 0f, 0f), null, isUpdateLocalList: true);
			CommonInventoryData.pInstance.RemoveItem(userItemData, 1);
		}
		UserItemPositionList.Save(base.pFarmManager._Rooms[0]._RoomID, OnNewUserItemPositionEvent);
	}

	public void OnNewUserItemPositionEvent(WsServiceType inType, WsServiceEvent inEvent, string inRoomID)
	{
		if (inEvent != WsServiceEvent.COMPLETE)
		{
			return;
		}
		switch (inType)
		{
		case WsServiceType.SET_USER_ITEM_POSITION_LIST:
			UserItemPositionList.Init(UserInfo.pInstance.UserID, inRoomID, OnNewUserItemPositionEvent);
			break;
		case WsServiceType.GET_USER_ITEM_POSITION_LIST:
			if (base.pFarmManager.pDictRoomData.ContainsKey(inRoomID))
			{
				base.pFarmManager.pDictRoomData[inRoomID].RecreateRoomItems();
			}
			UnityEngine.Object.Destroy(base.gameObject);
			break;
		}
	}

	protected override bool CanProcessUpdateData()
	{
		return CanActivate();
	}
}
