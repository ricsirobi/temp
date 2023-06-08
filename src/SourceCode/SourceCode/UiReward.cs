using UnityEngine;

public class UiReward : KAUI
{
	public const string PAYOUT_MEMBER_POSTFIX = "Member";

	public LocaleString _CoinText = new LocaleString("Coin");

	public UiHighScores _HighScores;

	protected KAWidget mLoading;

	protected KAWidget mScrollRtBtn;

	private int mCoins;

	private bool mWebServiceCompleted = true;

	protected override void Start()
	{
		base.Start();
		if (HighScores.pInstance != null && HighScores.GetDisplayLaunchPage() != eLaunchPage.HIGHSCORE)
		{
			GetPayout();
		}
		mLoading = FindItem("Loading");
		mLoading.SetVisibility(inVisible: true);
		mScrollRtBtn = FindItem("ScrollRtBtn");
		mScrollRtBtn.SetVisibility(inVisible: false);
	}

	public void GetPayout()
	{
		GameDataUserData gameData = HighScores.GetGameData(0);
		if (gameData != null)
		{
			int points = int.Parse(gameData.Item);
			if (HighScores.pInstance != null && HighScores.pInstance.ModuleName != null)
			{
				string text = HighScores.pInstance.ModuleName;
				if (SubscriptionInfo.pIsMember)
				{
					text += "Member";
				}
				WsWebService.GetPayout(text, points, ServiceEventHandler, null);
			}
		}
		else
		{
			mWebServiceCompleted = true;
			mLoading.SetVisibility(inVisible: false);
			mScrollRtBtn.SetVisibility(inVisible: true);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE)
		{
			mWebServiceCompleted = false;
		}
		else if (inType == WsServiceType.GET_PAYOUT)
		{
			mWebServiceCompleted = true;
			SetRewardsData((int)inObject);
			Money.AddMoney(mCoins, bForceUpdate: true);
			mLoading.SetVisibility(inVisible: false);
			mScrollRtBtn.SetVisibility(inVisible: true);
		}
	}

	public void SetRewardsData(int coins)
	{
		mCoins = coins;
		KAWidget kAWidget = FindItem("Reward");
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.FindChildItem("TxtAmount").SetText(mCoins.ToString());
		if (mCoins == 1)
		{
			kAWidget.FindChildItem("TxtItemName").SetTextByID(_CoinText._ID, _CoinText._Text);
		}
		if (AvatarData.pInstance != null)
		{
			kAWidget.FindChildItem("TxtMyName").SetText(AvatarData.pInstance.DisplayName);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "CloseBtn" && mWebServiceCompleted && _HighScores.IsWebServiceDone())
		{
			HighScores.ClearGameData();
			if (HighScores.pInstance != null && HighScores.pInstance.mMessageObject != null)
			{
				HighScores.pInstance.mMessageObject.SendMessage("OnHighScoresDone");
			}
			Object.Destroy(base.gameObject);
		}
		else if (item == mScrollRtBtn)
		{
			KAInput.ResetInputAxes();
			SetVisibility(inVisible: false);
			_HighScores.SetCurrentPage();
		}
	}
}
