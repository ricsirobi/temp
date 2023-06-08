using System.Collections.Generic;
using UnityEngine;

public class UiUDTRewardPopUp : KAUI
{
	public GameObject _MessageObject;

	public RewardWidget _RewardWidget;

	public void SetRewards(AchievementReward[] inRewards, List<MissionRewardData> inMissionRewardData)
	{
		UiRewards.pForceShowRewards = true;
		_RewardWidget.SetRewards(inRewards, inMissionRewardData, null, OnSetReward);
	}

	public void OnSetReward(RewardWidget.SetRewardStatus inSetRewardStatus)
	{
		bool closeBtnInteractive = ((inSetRewardStatus != 0) ? true : false);
		SetCloseBtnInteractive(closeBtnInteractive);
	}

	private void SetCloseBtnInteractive(bool isInteractive)
	{
		KAWidget kAWidget = FindItem("CloseBtn");
		if (kAWidget != null)
		{
			kAWidget.SetInteractive(isInteractive);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "CloseBtn")
		{
			UiRewards.pForceShowRewards = false;
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnRewardClose", null, SendMessageOptions.DontRequireReceiver);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
