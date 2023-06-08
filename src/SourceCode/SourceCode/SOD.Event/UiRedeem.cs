using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SOD.Event;

public class UiRedeem : KAUI
{
	public delegate void OnRedeemed(List<int> failList);

	[Header("Widgets")]
	[SerializeField]
	private KAWidget m_BtnRedeem;

	private List<AchievementTaskInfo> mAchievementTaskInfoList;

	private int mRewardsCount;

	private List<int> mRedeemFailedList;

	private List<int> mRedeemedInfoIdList;

	private event OnRedeemed mRedeemFinished;

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == m_BtnRedeem)
		{
			RedeemReward();
		}
	}

	public void SetRedeemData(List<AchievementTaskInfo> achTaskInfo, OnRedeemed onRedeemed = null)
	{
		this.mRedeemFinished = onRedeemed;
		mAchievementTaskInfoList = achTaskInfo;
	}

	private void RedeemReward()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mRewardsCount = mAchievementTaskInfoList.Count;
		mRedeemFailedList = new List<int>();
		mRedeemedInfoIdList = new List<int>();
		foreach (AchievementTaskInfo mAchievementTaskInfo in mAchievementTaskInfoList)
		{
			WsWebService.RedeemUserAchievementTaskReward(mAchievementTaskInfo.AchievementInfoID, RedeemServiceEventHandler, mAchievementTaskInfo.AchievementInfoID);
		}
	}

	private void RedeemServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inType != WsServiceType.REDEEM_USER_ACHIEVEMENT_TASK_REWARD)
			{
				break;
			}
			if (inObject != null)
			{
				RedeemUserAchievementTaskResponse redeemUserAchievementTaskResponse = (RedeemUserAchievementTaskResponse)inObject;
				if (redeemUserAchievementTaskResponse != null)
				{
					if (inUserData != null)
					{
						ProcessRedeemResponse(success: true, (int)inUserData);
					}
					AchievementReward[] achievementRewards = redeemUserAchievementTaskResponse.AchievementRewards;
					if (achievementRewards != null)
					{
						GameUtilities.AddRewards(achievementRewards, inUseRewardManager: false, inImmediateShow: false);
					}
					if (MissionManager.pInstance != null)
					{
						MissionManager.pInstance.RefreshMissions();
					}
				}
			}
			else
			{
				ProcessRedeemResponse(success: false);
			}
			break;
		case WsServiceEvent.ERROR:
			UtDebug.LogError("ERROR: Unable to fetch achievements!");
			KAUICursorManager.SetDefaultCursor("Arrow");
			ProcessRedeemResponse(success: false);
			break;
		}
	}

	private void ProcessRedeemResponse(bool success, int infoId = 0)
	{
		mRewardsCount--;
		if (success && infoId != 0)
		{
			mRedeemedInfoIdList.Add(infoId);
		}
		if (mRewardsCount == 0 && this.mRedeemFinished != null)
		{
			mRedeemFailedList = GetRedeemedFailedList(mRedeemedInfoIdList);
			this.mRedeemFinished(mRedeemFailedList);
			KAUICursorManager.SetDefaultCursor("Arrow");
			KAUI.RemoveExclusive(this);
			Object.Destroy(base.gameObject);
		}
	}

	private List<int> GetRedeemedFailedList(List<int> redeemedItems)
	{
		List<int> list = new List<int>();
		foreach (AchievementTaskInfo mAchievementTaskInfo in mAchievementTaskInfoList)
		{
			list.Add(mAchievementTaskInfo.AchievementInfoID);
		}
		return list.Except(redeemedItems).ToList();
	}
}
