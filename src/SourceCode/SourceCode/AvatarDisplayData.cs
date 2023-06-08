using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AvatarDisplayData", Namespace = "", IsNullable = false)]
public class AvatarDisplayData
{
	[XmlElement(ElementName = "AvatarData", IsNullable = true)]
	public AvatarData AvatarData;

	[XmlElement(Namespace = "http://api.jumpstart.com/")]
	public UserInfo UserInfo;

	[XmlElement(ElementName = "UserSubscriptionInfo")]
	public UserSubscriptionInfo UserSubscriptionInfo;

	public UserAchievementInfo AchievementInfo;

	[XmlElement(ElementName = "Achievements")]
	public UserAchievementInfo[] Achievements;

	[XmlElement(ElementName = "RewardMultipliers", IsNullable = true)]
	public RewardMultiplier[] RewardMultipliers;

	public int RankID;

	public string GetDisplayName()
	{
		return AvatarData.DisplayName;
	}

	public bool IsMember()
	{
		return UserSubscriptionInfo.SubscriptionTypeID == 1;
	}

	public RewardMultiplier GetRewardMultiplier(int inMultiplierType)
	{
		if (RewardMultipliers == null)
		{
			return null;
		}
		RewardMultiplier inRewardMultiplier = Array.Find(RewardMultipliers, (RewardMultiplier x) => x != null && x.PointTypeID == inMultiplierType);
		return RemoveRewardMultiplierIfExpired(inRewardMultiplier);
	}

	public List<RewardMultiplier> GetRewardMultipliers()
	{
		if (RewardMultipliers == null)
		{
			return null;
		}
		List<RewardMultiplier> list = new List<RewardMultiplier>(RewardMultipliers);
		list.RemoveAll((RewardMultiplier t) => t.MultiplierEffectTime < ServerTime.pCurrentTime);
		return list;
	}

	private RewardMultiplier RemoveRewardMultiplierIfExpired(RewardMultiplier inRewardMultiplier)
	{
		if (inRewardMultiplier != null && inRewardMultiplier.MultiplierEffectTime < ServerTime.pCurrentTime)
		{
			List<RewardMultiplier> list = new List<RewardMultiplier>(RewardMultipliers);
			list.Remove(inRewardMultiplier);
			RewardMultipliers = list.ToArray();
			return null;
		}
		return inRewardMultiplier;
	}
}
