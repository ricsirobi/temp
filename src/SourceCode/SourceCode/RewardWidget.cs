using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardWidget : KAWidget
{
	public class RewardItemBundleData : CoBundleItemData
	{
		public delegate void ResourceLoadCallback();

		public KAWidget _TxtRewardWidget;

		public int _Quantity;

		public ResourceLoadCallback _OnResourceLoaded;

		public override void OnAllDownloaded()
		{
			base.OnAllDownloaded();
			if (_OnResourceLoaded != null)
			{
				_OnResourceLoaded();
			}
		}
	}

	[Serializable]
	public class RewardPositionsData
	{
		public Vector2[] _Positions;
	}

	[Serializable]
	public class SpriteSizeData
	{
		public int _PointType;

		public Vector2 _SpriteSize;
	}

	public delegate void RewardItemCreated(KAWidget inRewardParentWidget, AchievementReward inAchievementReward);

	public enum SetRewardStatus
	{
		LOADING,
		COMPLETE
	}

	public delegate void SetRewardCallback(SetRewardStatus inRewardStatus);

	public RewardPositionsData[] _RewardPositions;

	public SpriteSizeData[] _SpriteSizes;

	public RewardItemCreated _OnRewardItemCreated;

	public SetRewardCallback _OnSetRewardCallback;

	public bool _ShowItemNames = true;

	private int mNumResourceLoadCalls;

	private int mAddRewardCallIndex;

	private List<KAWidget> mWidgetsCreated = new List<KAWidget>();

	public virtual void SetRewards(AchievementReward[] inRewards, List<MissionRewardData> inRewardData, RewardItemCreated inRewardItemCreated = null, SetRewardCallback inSetRewardCallback = null)
	{
		ClearWidgetsCreated();
		if (inRewards == null || _RewardPositions == null || _RewardPositions.Length == 0)
		{
			inSetRewardCallback?.Invoke(SetRewardStatus.COMPLETE);
			return;
		}
		_OnRewardItemCreated = inRewardItemCreated;
		_OnSetRewardCallback = inSetRewardCallback;
		Array.Sort(inRewards, (AchievementReward a, AchievementReward b) => a.PointTypeID.Value - b.PointTypeID.Value);
		Dictionary<int, object> dictionary = new Dictionary<int, object>();
		int num = 0;
		foreach (AchievementReward reward in inRewards)
		{
			if (!dictionary.ContainsKey(reward.PointTypeID.Value))
			{
				if (reward.PointTypeID.Value == 6)
				{
					List<AchievementReward> list = new List<AchievementReward>();
					list.Add(reward);
					dictionary.Add(reward.PointTypeID.Value, list);
				}
				else
				{
					dictionary.Add(reward.PointTypeID.Value, reward);
				}
			}
			else if (reward.PointTypeID.Value == 6)
			{
				if (dictionary[reward.PointTypeID.Value] is List<AchievementReward> list2)
				{
					AchievementReward achievementReward = list2.Find((AchievementReward r) => r.ItemID == reward.ItemID);
					if (achievementReward != null)
					{
						achievementReward.Amount += reward.Amount.Value;
						continue;
					}
					list2.Add(reward);
					num++;
				}
			}
			else if (dictionary[reward.PointTypeID.Value] is AchievementReward achievementReward2)
			{
				achievementReward2.Amount += reward.Amount.Value;
			}
		}
		int num2 = Mathf.Clamp(num + dictionary.Keys.Count - 1, 0, _RewardPositions.Length - 1);
		RewardPositionsData inRewardPositionData = _RewardPositions[num2];
		mNumResourceLoadCalls = 0;
		mAddRewardCallIndex = 0;
		if (_OnSetRewardCallback != null)
		{
			_OnSetRewardCallback(SetRewardStatus.LOADING);
		}
		foreach (KeyValuePair<int, object> item in dictionary)
		{
			if (item.Key != 6)
			{
				AchievementReward achievementReward3 = item.Value as AchievementReward;
				AddRewardItem(inRewardData, inRewardPositionData, achievementReward3);
				continue;
			}
			foreach (AchievementReward item2 in item.Value as List<AchievementReward>)
			{
				AddRewardItem(inRewardData, inRewardPositionData, item2);
			}
		}
		CheckForRewardComplete();
	}

	public virtual void SetRewards(AchievementTaskReward[] inRewards, List<MissionRewardData> inRewardData, RewardItemCreated inRewardItemCreated = null, SetRewardCallback inSetRewardCallback = null)
	{
		ClearWidgetsCreated();
		if (inRewards == null)
		{
			inSetRewardCallback?.Invoke(SetRewardStatus.COMPLETE);
			return;
		}
		AchievementReward[] array = new AchievementReward[inRewards.Length];
		for (int i = 0; i < inRewards.Length; i++)
		{
			array[i] = new AchievementReward();
			array[i].Amount = inRewards[i].RewardQuantity;
			array[i].PointTypeID = inRewards[i].PointTypeID;
			array[i].ItemID = (inRewards[i].ItemID.HasValue ? inRewards[i].ItemID.Value : 0);
		}
		SetRewards(array, inRewardData, inRewardItemCreated, inSetRewardCallback);
	}

	protected virtual void AddRewardItem(List<MissionRewardData> inMissionRewardData, RewardPositionsData inRewardPositionData, AchievementReward achievementReward)
	{
		MissionRewardData rewardData = inMissionRewardData.Find((MissionRewardData r) => r._PointType == achievementReward.PointTypeID.Value);
		if (rewardData == null)
		{
			return;
		}
		KAWidget kAWidget = FindChildItem("RewardTemplate");
		if (!(kAWidget != null))
		{
			return;
		}
		GameObject obj = UnityEngine.Object.Instantiate(kAWidget.gameObject, kAWidget.transform.position, Quaternion.identity);
		obj.name = "RewardWidget" + mAddRewardCallIndex;
		KAWidget component = obj.GetComponent<KAWidget>();
		mWidgetsCreated.Add(component);
		AddChild(component);
		Vector2 vector = inRewardPositionData._Positions[mAddRewardCallIndex];
		component.SetPosition(vector.x, vector.y);
		component.SetVisibility(inVisible: true);
		KAWidget kAWidget2 = component.FindChildItem("TxtReward");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: true);
			string text = ((achievementReward.Amount.Value == 1) ? rewardData._DescriptionFor1.GetLocalizedString() : rewardData._Description.GetLocalizedString());
			if (text.Length > 0)
			{
				kAWidget2.SetText(achievementReward.Amount.Value + " " + text);
			}
			else
			{
				kAWidget2.SetText(achievementReward.Amount.Value.ToString());
			}
		}
		KAWidget kAWidget3;
		if (achievementReward.PointTypeID.Value == 6)
		{
			kAWidget3 = component.FindChildItem("IcoTexture");
			RewardItemBundleData rewardItemBundleData = new RewardItemBundleData();
			rewardItemBundleData._TxtRewardWidget = kAWidget2;
			rewardItemBundleData._Quantity = achievementReward.Amount.Value;
			kAWidget3.SetUserData(rewardItemBundleData);
			mNumResourceLoadCalls++;
			ItemData.Load(achievementReward.ItemID, OnLoadItemDataReady, kAWidget3);
		}
		else
		{
			kAWidget3 = component.FindChildItem("IcoSprite");
			kAWidget3.SetSprite(rewardData._SpriteName);
		}
		SpriteSizeData spriteSizeData = Array.Find(_SpriteSizes, (SpriteSizeData x) => x._PointType == rewardData._PointType);
		if (spriteSizeData != null)
		{
			UIWidget componentInChildren = kAWidget3.GetComponentInChildren<UIWidget>();
			if (componentInChildren != null)
			{
				componentInChildren.width = (int)spriteSizeData._SpriteSize.x;
				componentInChildren.height = (int)spriteSizeData._SpriteSize.y;
			}
		}
		kAWidget3.SetVisibility(inVisible: true);
		if (_OnRewardItemCreated != null)
		{
			_OnRewardItemCreated(component, achievementReward);
		}
		mAddRewardCallIndex++;
	}

	protected virtual void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		if (inUserData != null && ((KAWidget)inUserData).GetUserData() is RewardItemBundleData rewardItemBundleData)
		{
			KAWidget txtRewardWidget = rewardItemBundleData._TxtRewardWidget;
			if (txtRewardWidget != null)
			{
				string text = rewardItemBundleData._Quantity.ToString();
				if (_ShowItemNames)
				{
					text = ((rewardItemBundleData._Quantity <= 1 || string.IsNullOrEmpty(dataItem.ItemNamePlural)) ? (text + " " + dataItem.ItemName) : (text + " " + dataItem.ItemNamePlural));
				}
				txtRewardWidget.SetText(text);
			}
			if (!string.IsNullOrEmpty(dataItem.IconName))
			{
				rewardItemBundleData._ItemTextureData.Init(dataItem.IconName);
				rewardItemBundleData._OnResourceLoaded = (RewardItemBundleData.ResourceLoadCallback)Delegate.Combine(rewardItemBundleData._OnResourceLoaded, new RewardItemBundleData.ResourceLoadCallback(OnRewardItemResourceLoaded));
				rewardItemBundleData.LoadResource();
				return;
			}
		}
		OnRewardItemResourceLoaded();
	}

	private void OnRewardItemResourceLoaded()
	{
		mNumResourceLoadCalls--;
		CheckForRewardComplete();
	}

	private void CheckForRewardComplete()
	{
		if (mNumResourceLoadCalls == 0 && _OnSetRewardCallback != null)
		{
			_OnSetRewardCallback(SetRewardStatus.COMPLETE);
		}
	}

	public void ClearWidgetsCreated()
	{
		for (int i = 0; i < mWidgetsCreated.Count; i++)
		{
			RemoveChildItem(mWidgetsCreated[i]);
			UnityEngine.Object.Destroy(mWidgetsCreated[i].gameObject);
		}
		mWidgetsCreated.Clear();
	}
}
