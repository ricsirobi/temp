using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardManager
{
	public static int pMoneyToDeduct = 0;

	public static bool pDisabled = false;

	private static List<AchievementReward> mRewards = new List<AchievementReward>();

	private static Dictionary<int, Vector2> mDynamicStartPosition = new Dictionary<int, Vector2>();

	private static GameObject mUiRewards = null;

	private static void AddReward(AchievementReward inReward)
	{
		AddReward(inReward.PointTypeID.Value, inReward.Amount.Value, inReward.ItemID);
	}

	private static void AddReward(int pointType, int amount, int itemID)
	{
		foreach (AchievementReward mReward in mRewards)
		{
			if (mReward.PointTypeID.Value == pointType && (itemID <= 0 || itemID == mReward.ItemID))
			{
				mReward.Amount += amount;
				return;
			}
		}
		AchievementReward achievementReward = new AchievementReward();
		achievementReward.PointTypeID = pointType;
		achievementReward.Amount = amount;
		achievementReward.ItemID = itemID;
		mRewards.Add(achievementReward);
	}

	public static void SetReward(AchievementReward[] inRewards, bool inImmediateShow)
	{
		if (pDisabled)
		{
			AchievementReward[] array = inRewards;
			foreach (AchievementReward achievementReward in array)
			{
				if (achievementReward.PointTypeID.Value == 2)
				{
					Money.AddToGameCurrency(achievementReward.Amount.Value);
				}
				else if (achievementReward.PointTypeID.Value == 5)
				{
					Money.AddToCashCurrency(achievementReward.Amount.Value);
				}
			}
		}
		else
		{
			AchievementReward[] array = inRewards;
			for (int i = 0; i < array.Length; i++)
			{
				AddReward(array[i]);
			}
			if (inImmediateShow)
			{
				ShowRewards(null);
			}
		}
	}

	public static void SetReward(int inCoins, bool inImmediateShow, GameObject inMessageObject)
	{
		if (pDisabled)
		{
			Money.AddToGameCurrency(inCoins);
			return;
		}
		if (inCoins > 0)
		{
			AddReward(2, inCoins, 0);
		}
		if (inImmediateShow)
		{
			ShowRewards(inMessageObject);
		}
	}

	public static void SetReward(int inCoins, int inPoints, bool inImmediateShow)
	{
		if (pDisabled)
		{
			Money.AddToGameCurrency(inCoins);
			return;
		}
		if (inCoins > 0)
		{
			AddReward(2, inCoins, 0);
		}
		if (inPoints > 0)
		{
			AddReward(1, inPoints, 0);
		}
		if (inImmediateShow)
		{
			ShowRewards(null);
		}
	}

	public static void SetReward(string inMissionReward, bool inImmediateShow, bool deduct = false)
	{
		SetReward(inMissionReward, inImmediateShow, Vector2.zero, deduct);
	}

	public static void SetReward(string inMissionReward, bool inImmediateShow, Vector2 inDynamicPosition, bool deduct = false)
	{
		if (string.IsNullOrEmpty(inMissionReward) || !(UtUtilities.DeserializeFromXml(inMissionReward, typeof(RewardData)) is RewardData rewardData) || rewardData.Rewards == null)
		{
			return;
		}
		Reward[] rewards = rewardData.Rewards;
		foreach (Reward reward in rewards)
		{
			int result = 0;
			int itemID = 0;
			if (!int.TryParse(reward.Type, out result))
			{
				if (reward.Type == "Coins")
				{
					result = 2;
				}
				else if (reward.Type == "Points")
				{
					result = 1;
				}
			}
			if (pDisabled)
			{
				switch (result)
				{
				case 2:
					Money.AddToGameCurrency(reward.Amount);
					break;
				case 5:
					Money.AddToCashCurrency(reward.Amount);
					break;
				}
				continue;
			}
			if (reward.ItemID.HasValue)
			{
				itemID = reward.ItemID.Value;
			}
			if (inDynamicPosition != Vector2.zero)
			{
				SetDynamicStartPosition(result, inDynamicPosition);
			}
			if (deduct && result == 2)
			{
				pMoneyToDeduct += reward.Amount;
			}
			AddReward(result, reward.Amount, itemID);
		}
		if (inImmediateShow)
		{
			ShowRewards(null);
		}
	}

	public static void SetDynamicStartPosition(int inType, Vector2 inStartPos)
	{
		if (!mDynamicStartPosition.ContainsKey(inType))
		{
			mDynamicStartPosition.Add(inType, inStartPos);
		}
	}

	public static void ShowRewards(GameObject inMessageObject)
	{
		if (mRewards.Count > 0)
		{
			if (mUiRewards == null)
			{
				mUiRewards = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiRewards"));
				mUiRewards.name = "PfUiRewards";
			}
			if (mDynamicStartPosition.Count > 0)
			{
				mUiRewards.SendMessage("SetDynamicStartPosition", mDynamicStartPosition.ToList(), SendMessageOptions.DontRequireReceiver);
				mDynamicStartPosition.Clear();
			}
			mUiRewards.SendMessage("DisplayRewards", mRewards.ToArray(), SendMessageOptions.DontRequireReceiver);
			if (inMessageObject != null)
			{
				mUiRewards.SendMessage("SetMessageObject", inMessageObject, SendMessageOptions.DontRequireReceiver);
			}
			mRewards.Clear();
		}
	}

	public static void OnWaitListCompleted()
	{
		if (mRewards.Count > 0)
		{
			ShowRewards(null);
		}
	}
}
