using UnityEngine;

public class UiHatchingSpeedUp : KAUI
{
	[SerializeField]
	private Vector2 m_TicketButtonAlternatePosition;

	[SerializeField]
	private Vector2 m_GemsButtonAlternatePosition;

	private GameObject mMessageObject;

	private Incubator mIncubator;

	private KAWidget mTicketBtn;

	private KAWidget mAdWatchBtn;

	private KAWidget mGemsPurchaseBtn;

	private string mAdWatchBtnOriginalTxt;

	private string mGemsBtnOriginalTxt;

	protected override void Start()
	{
		base.Start();
		mTicketBtn = FindItem("HatchTicketBtn");
		mAdWatchBtn = FindItem("AdWatchBtn");
		mGemsPurchaseBtn = FindItem("GemsBtn");
		mAdWatchBtnOriginalTxt = mAdWatchBtn.GetText();
		mGemsBtnOriginalTxt = mGemsPurchaseBtn.GetText();
	}

	public void Init(GameObject messageObject, Incubator incubator)
	{
		mMessageObject = messageObject;
		mIncubator = incubator;
		mGemsPurchaseBtn.SetText(string.Format(mGemsBtnOriginalTxt, mIncubator.pSpeedUpTimerTicketCost));
		mTicketBtn.SetDisabled(CommonInventoryData.pInstance.FindItem(mIncubator.pSpeedUpTimerTicketItemID) == null && CommonInventoryData.pInstance.FindItem(SanctuaryData.pInstance._CommonHatchTicketInfo._InstantHatchTicketItemID) == null && (!ParentData.pIsReady || (ParentData.pIsReady && ParentData.pInstance.pInventory.pData.FindItem(mIncubator.pSpeedUpTimerTicketItemID) == null && ParentData.pInstance.pInventory.pData.FindItem(SanctuaryData.pInstance._CommonHatchTicketInfo._InstantHatchTicketItemID) == null)));
		mAdWatchBtn.SetVisibility(AdManager.pInstance.AdSupported(mIncubator._AdEventType, AdType.REWARDED_VIDEO));
		if (mAdWatchBtn.GetVisibility())
		{
			mAdWatchBtn.SetDisabled(!mIncubator.IsAdWatchAvailable());
			AdManager.pInstance.GetAvailableAndDailyLimitCount(mIncubator._AdEventType, out var dailyLimit, out var available);
			mAdWatchBtn.SetText(string.Format(mAdWatchBtnOriginalTxt, available, dailyLimit));
		}
		else
		{
			RepositionWidgets();
		}
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "CloseBtn")
		{
			Exit();
			return;
		}
		SetVisibility(inVisible: false);
		KAUI.RemoveExclusive(this);
		if (inWidget == mGemsPurchaseBtn)
		{
			mIncubator.SpeedUpHatching(OnSpeedUpDone);
		}
		else if (inWidget == mTicketBtn)
		{
			mIncubator.UseSpeedUpTicket(OnSpeedUpDone);
		}
		else if (inWidget == mAdWatchBtn)
		{
			mIncubator.ShowAd(OnSpeedUpDone);
		}
	}

	private void OnSpeedUpDone(bool success, Incubator incubator)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnSpeedUpDone", success, SendMessageOptions.DontRequireReceiver);
		}
		Exit();
	}

	private void Exit()
	{
		KAUI.RemoveExclusive(this);
		mMessageObject = null;
		mIncubator = null;
		SetVisibility(inVisible: false);
	}

	private void RepositionWidgets()
	{
		mTicketBtn.SetPosition(m_TicketButtonAlternatePosition.x, m_TicketButtonAlternatePosition.y);
		mGemsPurchaseBtn.SetPosition(m_GemsButtonAlternatePosition.x, m_GemsButtonAlternatePosition.y);
	}
}
