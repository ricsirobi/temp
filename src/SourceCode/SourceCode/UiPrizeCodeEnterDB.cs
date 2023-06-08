using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiPrizeCodeEnterDB : KAUI
{
	public GameObject _MessageObject;

	public LocaleString _InvalidCodeText = new LocaleString("Sorry, the code you have entered is invalid. Please re-enter your code.");

	public LocaleString _AlreadyRedeemedCodeText = new LocaleString("Sorry, this code has been redeemed before. Please enter a different code.");

	public LocaleString _AlreadyRedeemedPrizeText = new LocaleString("Sorry, you have already redeemed this dragon. Please enter a different code.");

	public LocaleString _RedeemInWebsiteText = new LocaleString("Sorry, this code must be redeemed on the School of Dragons website at: www.schoolofdragons.com");

	public LocaleString _ErrorOnRedeemingText = new LocaleString("An error occured while redeeming code.");

	private KAWidget mTxtCode;

	private KAWidget mTxtMessage;

	private KAWidget mCloseBtn;

	private KAWidget mBtnApply;

	private int mCodeCharLimit;

	private List<int> mPendingDragonRewardList;

	protected override void Start()
	{
		base.Start();
		mCloseBtn = FindItem("CloseBtn");
		mTxtCode = FindItem("TxtCode");
		mTxtMessage = FindItem("TxtMessage");
		mBtnApply = FindItem("BtnApply");
		UIInput component = mTxtCode.gameObject.GetComponent<UIInput>();
		if (component != null)
		{
			mCodeCharLimit = component.characterLimit;
		}
		mBtnApply.SetDisabled(mCodeCharLimit > 0);
		KAUI.SetExclusive(this);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			KAUI.RemoveExclusive(this);
			Object.Destroy(base.gameObject);
		}
		else if (inWidget == mBtnApply)
		{
			ApplyCode();
		}
	}

	public override void OnInput(KAWidget inWidget, string inText)
	{
		base.OnInput(inWidget, inText);
		if (inWidget == mTxtCode && mCodeCharLimit > 0)
		{
			mBtnApply.SetDisabled(inText.Length != mCodeCharLimit);
		}
	}

	private void ApplyCode()
	{
		WsWebService.SubmitPrizeCode(new PrizeCodeSubmitRequest
		{
			Code = mTxtCode.GetText(),
			RedeemFromGame = true
		}, ServiceEventHandler, null);
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			PrizeCodeSubmitResponse prizeCodeSubmitResponse = (PrizeCodeSubmitResponse)inObject;
			SubmissionResult? status = prizeCodeSubmitResponse.Status;
			UtDebug.Log("Prize code Response result:" + status.ToString());
			if (prizeCodeSubmitResponse.Status == SubmissionResult.InvalidRedeemtionSource)
			{
				ShowAlert(_RedeemInWebsiteText.GetLocalizedString());
			}
			else if (prizeCodeSubmitResponse.Status == SubmissionResult.UsedCode)
			{
				ShowAlert(_AlreadyRedeemedCodeText.GetLocalizedString());
			}
			else if (prizeCodeSubmitResponse.Status == SubmissionResult.PrizeEarned)
			{
				ShowAlert(_AlreadyRedeemedPrizeText.GetLocalizedString());
			}
			else if (prizeCodeSubmitResponse.Status != SubmissionResult.Success)
			{
				ShowAlert(_InvalidCodeText.GetLocalizedString());
			}
			else
			{
				ShowRewards(prizeCodeSubmitResponse.Rewards);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			OnClick(mCloseBtn);
			break;
		}
	}

	private void ShowAlert(string inAlert)
	{
		mTxtMessage.SetText(inAlert);
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
	}

	private void ShowRewards(AchievementReward[] inRewards)
	{
		if (inRewards != null && inRewards.Length != 0)
		{
			List<AchievementReward> list = new List<AchievementReward>();
			foreach (AchievementReward achievementReward in inRewards)
			{
				if (achievementReward.PointTypeID.Value == 6)
				{
					list.Add(achievementReward);
				}
			}
			if (list.Count > 0)
			{
				StartCoroutine(CheckRewards(list));
			}
			else
			{
				ShowAlert(_ErrorOnRedeemingText.GetLocalizedString());
			}
		}
		else
		{
			ShowAlert(_ErrorOnRedeemingText.GetLocalizedString());
		}
	}

	private IEnumerator CheckRewards(List<AchievementReward> rewardItemList)
	{
		CommonInventoryData.Reset();
		CommonInventoryData.Init();
		while (!CommonInventoryData.pIsReady)
		{
			yield return null;
		}
		List<UserItemData> list = new List<UserItemData>();
		mPendingDragonRewardList = new List<int>();
		for (int num = rewardItemList.Count - 1; num >= 0; num--)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(rewardItemList[num].ItemID);
			if (userItemData != null)
			{
				if (userItemData.Item.HasCategory(454))
				{
					mPendingDragonRewardList.Add(rewardItemList[num].ItemID);
				}
				else
				{
					list.Add(userItemData);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			SetVisibility(inVisible: false);
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources("PfUiRewardsDB");
			if (gameObject != null)
			{
				Object.Instantiate(gameObject).GetComponent<UiRewardsDB>().Show(list, base.gameObject);
			}
		}
		else if (mPendingDragonRewardList.Count > 0)
		{
			SetVisibility(inVisible: false);
			ShowDragonReward();
		}
		else
		{
			ShowAlert(_ErrorOnRedeemingText.GetLocalizedString());
		}
	}

	private void ShowDragonReward()
	{
		if (UserNotifyDragonTicket.pInstance != null)
		{
			UserNotifyDragonTicket.pInstance.CheckTickets(mPendingDragonRewardList, OnRewardShown);
		}
		mPendingDragonRewardList.Clear();
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (!inVisible)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			KAUI.RemoveExclusive(this);
			_MessageObject.SendMessage("OnRedeemPrizeCode", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnRewardShown(bool success)
	{
		if (mPendingDragonRewardList.Count > 0)
		{
			ShowDragonReward();
			return;
		}
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		Object.Destroy(base.gameObject);
	}
}
