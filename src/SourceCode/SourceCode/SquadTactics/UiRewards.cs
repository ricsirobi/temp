using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiRewards : KAWidget
{
	public RewardWidget _XPRewardWidget;

	public GameObject _LootBox;

	public float _Timer = 3f;

	public Color _RewardColor;

	public Color _UpdateRewardColor;

	public void ShowRewards(List<AchievementReward> rewards, List<AchievementReward> displayRewards, bool updateReward = true)
	{
		if (_LootBox != null)
		{
			_LootBox.SetActive(value: true);
		}
		SetVisibility(inVisible: true);
		if (updateReward)
		{
			GameUtilities.AddRewards(rewards.ToArray(), inUseRewardManager: false);
			base.pBackground.color = _UpdateRewardColor;
		}
		else
		{
			base.pBackground.color = _RewardColor;
		}
		if (_XPRewardWidget != null)
		{
			rewards.AddRange(displayRewards);
			_XPRewardWidget.SetRewards(rewards.ToArray(), MissionManager.pInstance._RewardData);
		}
		StartCoroutine(DelayClose());
	}

	private IEnumerator DelayClose()
	{
		yield return new WaitForSeconds(_Timer);
		SetVisibility(inVisible: false);
		if (_LootBox != null)
		{
			_LootBox.SetActive(value: false);
		}
	}
}
