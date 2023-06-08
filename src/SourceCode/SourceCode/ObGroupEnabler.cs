using System;
using UnityEngine;

public class ObGroupEnabler : MonoBehaviour
{
	public int _NumItemsInGroup = 20;

	public string _ItemNameRoot = "GroupItem";

	public bool _ForceGroupUpdate;

	public bool _ForceAllActive;

	private TreasureChestData mTCGroupData;

	private UserTreasureChestData mTCData;

	private DateTime mServerTime;

	private GameObject mMessageBackObject;

	private void Start()
	{
		mTCGroupData = null;
		mTCData = null;
		WsWebService.GetTreasureChest(RsResourceManager.pCurrentLevel, base.gameObject.name, ServiceEventHandler, null);
	}

	private void DetermineGroupStatus()
	{
		if (mTCGroupData == null)
		{
			UtDebug.LogError("Group " + base.gameObject.name + " mTCGroupData is null!!");
			return;
		}
		if (!DateTime.TryParse(mTCGroupData.ServerTime, out mServerTime))
		{
			mServerTime = DateTime.Today;
		}
		if (mTCGroupData.StartDate != null && mTCGroupData.EndDate != null && DateTime.TryParse(mTCGroupData.StartDate, out var result) && DateTime.TryParse(mTCGroupData.EndDate, out var result2) && result <= mServerTime && mServerTime <= result2)
		{
			ActivateGroup();
		}
	}

	private void ActivateGroup()
	{
		WsWebService.GetUserTreasureChest(RsResourceManager.pCurrentLevel, base.gameObject.name, ServiceEventHandler, null);
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_TREASURE_CHEST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mTCGroupData = (TreasureChestData)inObject;
				DetermineGroupStatus();
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetTreasureChest FAILED!!!");
				mTCGroupData = null;
				break;
			}
			break;
		case WsServiceType.GET_USER_TREASURE_CHEST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					mTCData = (UserTreasureChestData)inObject;
				}
				else
				{
					mTCData = new UserTreasureChestData();
				}
				mTCData.InitChestList();
				InitGroupItems();
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetTreasureChest FAILED!!!");
				mTCData = new UserTreasureChestData();
				mTCData.InitChestList();
				InitGroupItems();
				break;
			}
			break;
		case WsServiceType.SET_USER_CHEST_FOUND:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (mMessageBackObject != null)
				{
					mMessageBackObject.SendMessage("OnSetChestFoundDone", null, SendMessageOptions.DontRequireReceiver);
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL SetTreasureChestFound FAILED!!!");
				break;
			}
			break;
		}
	}

	protected virtual void InitGroupItems()
	{
		DateTime result;
		if (mTCData.pChestList.Count == 0)
		{
			SetUpNewItems();
		}
		else if (DateTime.TryParse(mTCData.Time, out result))
		{
			DateTime dateTime = result.AddDays(mTCGroupData.RespawnTime);
			if (_ForceGroupUpdate)
			{
				SetUpNewItems();
			}
			else if (mServerTime <= dateTime)
			{
				ResetExistingItems();
			}
			else
			{
				SetUpNewItems();
			}
		}
		else
		{
			SetUpNewItems();
		}
	}

	protected virtual void SetUpNewItems()
	{
		bool flag = false;
		mTCData.ClearChestList();
		mTCData.TreasureChestId = mTCGroupData.TreasureChestId;
		mTCData.Time = mServerTime.ToString();
		mTCData.SceneName = RsResourceManager.pCurrentLevel;
		mTCData.GroupName = base.gameObject.name;
		int num = UnityEngine.Random.Range(mTCGroupData.ChestMin, mTCGroupData.ChestMax + 1);
		if (num == 0)
		{
			return;
		}
		if (num > _NumItemsInGroup || _ForceAllActive)
		{
			num = _NumItemsInGroup;
		}
		Transform transform = base.gameObject.transform;
		for (int i = 1; i <= _NumItemsInGroup; i++)
		{
			string text = _ItemNameRoot + i.ToString("d2");
			Transform transform2 = transform.Find(text);
			if (transform2 == null)
			{
				UtDebug.LogError("Couldn't find chest " + text);
			}
			else
			{
				transform2.gameObject.SetActive(value: false);
			}
		}
		int num2 = 0;
		int num3 = 0;
		int[] array = UtUtilities.GenerateShuffledInts(_NumItemsInGroup);
		bool flag2 = false;
		if (mTCGroupData.ItemId != null && mTCGroupData.ItemId.Length != 0)
		{
			flag2 = true;
			if (mTCGroupData.ItemId.Length > 1)
			{
				UtUtilities.Shuffle(mTCGroupData.ItemId);
			}
		}
		if (num == _NumItemsInGroup && !flag2 && !mTCGroupData.GameCurrencyMin.HasValue)
		{
			flag = true;
		}
		while (num > 0)
		{
			int num4 = array[num2++] + 1;
			string text2 = _ItemNameRoot + num4.ToString("d2");
			GameObject gameObject = null;
			Transform transform3 = transform.Find(text2);
			if (transform3 == null)
			{
				UtDebug.LogError("Couldn't find item " + text2);
				continue;
			}
			gameObject = transform3.gameObject;
			if (gameObject.activeInHierarchy)
			{
				continue;
			}
			UserTreasureChestDataChest userTreasureChestDataChest = new UserTreasureChestDataChest();
			userTreasureChestDataChest.Found = false;
			userTreasureChestDataChest.Name = text2;
			ObGroupItem component = gameObject.GetComponent<ObGroupItem>();
			if (component == null)
			{
				UtDebug.LogError("!_!_!_!_ Can't find Item script in turned off item object");
				continue;
			}
			if (flag2 && (num3 < mTCGroupData.ItemId.Length || !mTCGroupData.GameCurrencyMin.HasValue))
			{
				component.pRewardItemID = mTCGroupData.ItemId[num3 % mTCGroupData.ItemId.Length];
				num3++;
				userTreasureChestDataChest.ItemId = component.pRewardItemID;
			}
			else if (mTCGroupData.GameCurrencyMin.HasValue)
			{
				component.pRewardAmount = UnityEngine.Random.Range(mTCGroupData.GameCurrencyMin.Value, mTCGroupData.GameCurrencyMax.Value + 1);
				userTreasureChestDataChest.GameCurrency = component.pRewardAmount;
			}
			else
			{
				component.pRewardAmount = 0;
				userTreasureChestDataChest.GameCurrency = component.pRewardAmount;
			}
			if (component.pEnable)
			{
				gameObject.SetActive(value: true);
			}
			if (!flag)
			{
				mTCData.NewChest(userTreasureChestDataChest);
			}
			num--;
		}
		mTCData.Save(ServiceEventHandler);
	}

	protected virtual void ResetExistingItems()
	{
		if (mTCData.pChestList.Count == 0)
		{
			SetUpNewItems();
			return;
		}
		Transform transform = base.gameObject.transform;
		if (mTCGroupData.ChestMin == _NumItemsInGroup && mTCGroupData.ChestMax == _NumItemsInGroup && (mTCGroupData.ItemId == null || mTCGroupData.ItemId.Length == 0) && !mTCGroupData.GameCurrencyMin.HasValue)
		{
			for (int i = 1; i <= _NumItemsInGroup; i++)
			{
				string text = _ItemNameRoot + i.ToString("d2");
				Transform transform2 = transform.Find(text);
				if (transform2 == null)
				{
					UtDebug.LogError("Couldn't find chest " + text);
					continue;
				}
				ObGroupItem component = transform2.GetComponent<ObGroupItem>();
				if (component == null)
				{
					UtDebug.LogError("!_!_!_!_ Can't find Item script in turned off item object");
					continue;
				}
				if (component.pEnable)
				{
					transform2.gameObject.SetActive(value: true);
				}
				component.pRewardAmount = 0;
			}
		}
		foreach (UserTreasureChestDataChest pChest in mTCData.pChestList)
		{
			GameObject gameObject = null;
			Transform transform3 = transform.Find(pChest.Name);
			if (transform3 == null)
			{
				UtDebug.LogError("Couldn't find chest " + pChest.Name);
				continue;
			}
			gameObject = transform3.gameObject;
			ObGroupItem component2 = gameObject.GetComponent<ObGroupItem>();
			if (component2 == null)
			{
				UtDebug.LogError("!_!_!_!_ Can't find Treasure Chest script in turned off chest object");
				continue;
			}
			component2.pFound = pChest.Found;
			if (pChest.GameCurrency.HasValue)
			{
				component2.pRewardAmount = pChest.GameCurrency.Value;
			}
			else if (pChest.ItemId.HasValue)
			{
				component2.pRewardItemID = pChest.ItemId.Value;
			}
			else
			{
				component2.pRewardAmount = 0;
			}
			if (component2.pEnable)
			{
				gameObject.SetActive(!component2.pFound);
			}
		}
	}

	public virtual void ItemFound(GameObject chest)
	{
		if (mTCData != null)
		{
			mTCData.SetChestFound(chest.name, ServiceEventHandler);
			mMessageBackObject = chest;
		}
	}
}
