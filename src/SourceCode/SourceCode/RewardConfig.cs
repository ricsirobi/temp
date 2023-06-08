using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardConfig", menuName = "PlayFab/RewardConfig", order = 1)]
public class RewardConfig : ScriptableObject
{
	private static RewardConfig mInstance;

	public List<RewardInfo> _RewardInfo = new List<RewardInfo>();

	public static RewardConfig pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = RsResourceManager.LoadAssetFromResources("RewardConfig.asset", isPrefab: false) as RewardConfig;
			}
			return mInstance;
		}
	}

	public RewardInfo GetRewardInfo(int typeID)
	{
		foreach (RewardInfo item in _RewardInfo)
		{
			if (item._TypeID == typeID)
			{
				return item;
			}
		}
		return null;
	}
}
