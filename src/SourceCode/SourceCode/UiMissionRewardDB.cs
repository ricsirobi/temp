using System.Collections.Generic;
using UnityEngine;

public class UiMissionRewardDB : KAUI
{
	public GameObject _MessageObject;

	public string _CloseMessage = "";

	public RewardWidget _MissionRewardWidget;

	public AudioClip _Sound;

	private Transform mToolbar;

	private Transform mMinimap;

	protected override void Start()
	{
		base.Start();
		UiToolbar uiToolbar = null;
		if (AvAvatar.pToolbar != null)
		{
			uiToolbar = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			mToolbar = AvAvatar.pToolbar.transform.Find("Anchor-TopRight");
			if (mToolbar != null)
			{
				mToolbar.parent = base.transform;
			}
		}
		mMinimap = UtUtilities.FindChildTransform(AvAvatar.pToolbar, "MiniMap", inactive: true);
		if (mMinimap != null)
		{
			mMinimap.parent = base.transform;
			UiMiniMap componentInChildren = mMinimap.GetComponentInChildren<UiMiniMap>();
			if (componentInChildren != null)
			{
				componentInChildren.SetState(KAUIState.NOT_INTERACTIVE);
			}
		}
		if (uiToolbar != null)
		{
			uiToolbar.SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!(inWidget.name == "CloseBtn") || _CloseMessage.Length <= 0)
		{
			return;
		}
		AvAvatar.pToolbar.GetComponent<UiToolbar>().SetState(KAUIState.INTERACTIVE);
		if (mToolbar != null)
		{
			mToolbar.parent = AvAvatar.pToolbar.transform;
		}
		if (mMinimap != null)
		{
			UiMiniMap componentInChildren = mMinimap.GetComponentInChildren<UiMiniMap>();
			if (componentInChildren != null)
			{
				componentInChildren.SetState(KAUIState.INTERACTIVE);
			}
			mMinimap.parent = AvAvatar.pToolbar.transform;
		}
		UiRewards.pForceShowRewards = false;
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SetRewards(AchievementReward[] inRewards, List<MissionRewardData> inMissionRewardData)
	{
		UiRewards.pForceShowRewards = true;
		GameUtilities.AddRewards(inRewards);
		_MissionRewardWidget.SetRewards(inRewards, inMissionRewardData, null, OnSetReward);
		if ((bool)_Sound)
		{
			SnChannel.Play(_Sound);
		}
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
}
