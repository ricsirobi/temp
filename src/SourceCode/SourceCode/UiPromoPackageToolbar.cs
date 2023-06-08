using System;
using System.Collections.Generic;
using UnityEngine;

public class UiPromoPackageToolbar : KAUI
{
	public string _PromoOfferResName = "RS_DATA/PfUiDragonPackagesDO.unity3d/PfUiDragonPackagesDO";

	public LocaleString _PromoPackageDisplayText = new LocaleString("Promo Package Available [[duration]] [[input]] [-]HERE[c] to see the offer");

	public LocaleString _DaysText = new LocaleString("Days");

	public List<PromoPackageTriggerType> _PromoTypes = new List<PromoPackageTriggerType>();

	private UiPromoOffer mUiPromoOffer;

	private List<PromoPackage> mPackagesToShow = new List<PromoPackage>();

	private bool mCheckForOffers;

	private TimeSpan mMinTimeSpan = TimeSpan.MaxValue;

	private float mCountDownTimer = 1f;

	private int mPackagesToLoad;

	private bool mIsNewOfferAvailable;

	private MessageInfo mPromoPackageInfo;

	protected override void Start()
	{
		base.Start();
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		mCheckForOffers = true;
		mIsNewOfferAvailable = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	protected override void Update()
	{
		base.Update();
		if (CommonInventoryData.pIsReady && DailyBonusAndPromo.pInstance != null && mCheckForOffers)
		{
			mCheckForOffers = false;
			CheckForOffers();
		}
		if (mPackagesToShow.Count <= 0)
		{
			return;
		}
		if (mPromoPackageInfo == null && mPackagesToLoad <= 0)
		{
			ShowOffer();
		}
		if (mIsNewOfferAvailable)
		{
			mIsNewOfferAvailable = false;
			ProductData.Save();
		}
		mCountDownTimer += Time.deltaTime;
		if (mCountDownTimer > 1f)
		{
			mCountDownTimer = 0f;
			mMinTimeSpan -= TimeSpan.FromSeconds(1.0);
			if (mMinTimeSpan <= new TimeSpan(0, 0, 0))
			{
				mCheckForOffers = true;
				RemoveOffer();
			}
		}
	}

	private void CheckForOffers()
	{
		mPackagesToShow.Clear();
		foreach (PromoPackage item in DailyBonusAndPromo.pInstance.CheckForPromoPackageOffers(_PromoTypes))
		{
			if (item.IsNewPackage())
			{
				mIsNewOfferAvailable = true;
				item.StartPackage();
			}
			mPackagesToLoad++;
			item.LoadPackageContent(ItemsInPackage);
		}
	}

	public void ItemsInPackage(int itemID, bool userHasItem)
	{
		if (!userHasItem)
		{
			PromoPackage[] promoPackages = DailyBonusAndPromo.pInstance.PromoPackages;
			foreach (PromoPackage package in promoPackages)
			{
				if (package.ItemID == itemID && mPackagesToShow.Find((PromoPackage p) => p.ItemID == package.ItemID) == null)
				{
					mPackagesToShow.Add(package);
					UpdateMinTime(mPackagesToShow);
				}
			}
		}
		mPackagesToLoad--;
	}

	private void ShowOffer()
	{
		mPromoPackageInfo = new MessageInfo();
		UiChatHistory.AddSystemNotification(GetDisplayMessage(mMinTimeSpan), mPromoPackageInfo, OnMessageSelected);
	}

	private void RemoveOffer()
	{
		UiChatHistory.SystemMessageAccepted(mPromoPackageInfo);
		mPromoPackageInfo = null;
		mPackagesToLoad = 0;
		mPackagesToShow.Clear();
	}

	private void OnMessageSelected(object messageObject)
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = _PromoOfferResName.Split('/');
		if (mUiPromoOffer == null)
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnOfferBundleLoaded, typeof(GameObject));
		}
	}

	private void OnOfferBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			mUiPromoOffer = gameObject.GetComponent<UiPromoOffer>();
			mUiPromoOffer.PackagesToShow(new List<PromoPackage>(mPackagesToShow));
			RemoveOffer();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatar.pPrevState;
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent == MissionEvent.TASK_COMPLETE || inEvent == MissionEvent.MISSION_COMPLETE)
		{
			mCheckForOffers = true;
		}
	}

	private string GetDisplayMessage(TimeSpan time)
	{
		int days = time.Days;
		int hours = time.Hours;
		int minutes = time.Minutes;
		int seconds = time.Seconds;
		string localizedString = _PromoPackageDisplayText.GetLocalizedString();
		string text = "";
		text = ((days < 1) ? (hours.ToString("d2") + ":" + minutes.ToString("d2") + ":" + seconds.ToString("d2")) : (Mathf.CeilToInt((float)time.TotalDays).ToString("d2") + " " + _DaysText.GetLocalizedString()));
		return localizedString.Replace("[[duration]]", text);
	}

	private void UpdateMinTime(List<PromoPackage> inPackagesToShow)
	{
		DateTime result = DateTime.MinValue;
		DateTime minValue = DateTime.MinValue;
		TimeSpan maxValue = TimeSpan.MaxValue;
		foreach (PromoPackage item in inPackagesToShow)
		{
			if (DateTime.TryParse(item.GetOfferStartTime(), out result))
			{
				if (item.Duration.HasValue)
				{
					maxValue = result.AddHours(item.Duration.Value).Subtract(ServerTime.pCurrentTime);
					if (maxValue < mMinTimeSpan)
					{
						mMinTimeSpan = maxValue;
					}
				}
			}
			else
			{
				UtDebug.LogError("Could not read form PairData");
			}
		}
	}
}
