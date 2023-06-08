using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardMultiplierManager : MonoBehaviour
{
	public float _FlashRemainingTime;

	public List<Color> _MultiplierColors;

	public static bool pIsReady;

	public static ArrayOfRewardTypeMultiplier _ArrayOfRewardTypeMultiplier;

	private InventorySaveEventHandler mSaveEventHandler;

	private static RewardMultiplierManager mInstance;

	public static RewardMultiplierManager pInstance
	{
		get
		{
			return mInstance;
		}
		set
		{
			mInstance = value;
		}
	}

	private void Awake()
	{
		if (pInstance == null)
		{
			pInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static void Init()
	{
		if (!pIsReady)
		{
			WsWebService.GetAllRewardTypeMultiplier(WsGetEventHandler, null);
		}
	}

	public static void WsGetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			pIsReady = true;
			_ArrayOfRewardTypeMultiplier = (ArrayOfRewardTypeMultiplier)inObject;
		}
	}

	public void UseRewardMultiplier(UserItemData inUserItemData, InventorySaveEventHandler inEventHandler)
	{
		if (CommonInventoryData.pInstance != null)
		{
			mSaveEventHandler = inEventHandler;
			CommonInventoryData.pInstance.UseItem(inUserItemData, 1, OnRewardMultiplierUse, inUserItemData.Item);
		}
		else
		{
			inEventHandler?.Invoke(success: false, inUserItemData.Item);
		}
	}

	private void OnRewardMultiplierUse(bool success, object inUserData)
	{
		if (!success || inUserData == null)
		{
			mSaveEventHandler?.Invoke(success, inUserData);
			return;
		}
		ItemData itemData = (ItemData)inUserData;
		if (UserProfile.pProfileData.AvatarInfo.RewardMultipliers == null)
		{
			UserProfile.pProfileData.AvatarInfo.RewardMultipliers = new RewardMultiplier[0];
		}
		int attribute = itemData.GetAttribute("MultiplierRewardType", 0);
		RewardMultiplier rewardMultiplier = UserProfile.pProfileData.AvatarInfo.GetRewardMultiplier(attribute);
		if (rewardMultiplier == null)
		{
			rewardMultiplier = new RewardMultiplier();
			rewardMultiplier.PointTypeID = attribute;
			List<RewardMultiplier> list = new List<RewardMultiplier>(UserProfile.pProfileData.AvatarInfo.RewardMultipliers);
			list.Add(rewardMultiplier);
			UserProfile.pProfileData.AvatarInfo.RewardMultipliers = list.ToArray();
			UpdateRewardMultiplier(itemData, rewardMultiplier, ServerTime.pCurrentTime);
		}
		else
		{
			UpdateRewardMultiplier(itemData, rewardMultiplier, rewardMultiplier.MultiplierEffectTime);
		}
		mSaveEventHandler?.Invoke(success, inUserData);
	}

	public static void UpdateRewardMultiplier(ItemData inItemData, RewardMultiplier inRewardMultiplier, DateTime srcTime)
	{
		int attribute = inItemData.GetAttribute("MultiplierFactor", 0);
		inRewardMultiplier.MultiplierFactor = attribute;
		int attribute2 = inItemData.GetAttribute("MultiplierEffectTime", 0);
		TimeSpan value = new TimeSpan(0, 0, attribute2);
		inRewardMultiplier.MultiplierEffectTime = srcTime.Add(value);
	}
}
