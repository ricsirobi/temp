using System;
using System.Collections.Generic;
using UnityEngine;

public class UiPromoPanel : UiPromoOffer
{
	[Serializable]
	public class ActionData
	{
		public PromoActionType _ActionType;

		public LocaleString _ActionText;

		public List<KAWidget> _WidgetsToHide;
	}

	[Serializable]
	public class RankLockedData
	{
		public int _RankId;

		public LocaleString _RankLockedText;
	}

	public List<ActionData> _ActionData = new List<ActionData>();

	public List<RankLockedData> _RankLockedData = new List<RankLockedData>();

	public LocaleString _PointLockedText;

	private KAWidget mActionBtn;

	private KAWidget mImageLoadingGear;

	private KAWidget mImageFullLoadingGear;

	private DateTime mEndTime = DateTime.MinValue;

	private readonly float mTimeUpdateInterval = 0.5f;

	private AvAvatarState mPreviousAvatarState;

	private PromoActionType mActionType;

	private GameObject mMessageObject;

	protected override void Start()
	{
		base.Start();
		AvAvatar.SetUIActive(inActive: false);
		if (AvAvatar.pState != AvAvatarState.PAUSED)
		{
			mPreviousAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		KAUI.SetExclusive(this);
	}

	private void SetActionData(PromoActionType actionType)
	{
		if (mActionBtn == null)
		{
			mActionBtn = FindItem("ActionBtn");
		}
		if (mActionBtn != null && _ActionData != null)
		{
			mActionBtn.SetDisabled(isDisabled: true);
			UpdateAction(reset: true);
			mActionType = actionType;
			UpdateAction();
		}
	}

	private void UpdateAction(bool reset = false)
	{
		ActionData actionData = _ActionData.Find((ActionData data) => data._ActionType == mActionType);
		if (actionData == null)
		{
			return;
		}
		mActionBtn.SetText(reset ? string.Empty : actionData._ActionText.GetLocalizedString());
		if (actionData._WidgetsToHide == null || actionData._WidgetsToHide.Count <= 0)
		{
			return;
		}
		foreach (KAWidget item in actionData._WidgetsToHide)
		{
			item.SetVisibility(reset);
		}
	}

	public override void OnDataReady(List<ItemData> itemDataList, int inItemID)
	{
		ShowLoadingGear(show: false);
		if (mActionBtn != null)
		{
			mActionBtn.SetDisabled(isDisabled: false);
		}
		if (!string.IsNullOrEmpty(mPackagesToShow[mPackageIndex].ImageRes))
		{
			LoadMarketingImage(mPackagesToShow[mPackageIndex].ImageRes);
		}
		else if (mCurrentPackageItemData != null)
		{
			LoadMarketingImage(mCurrentPackageItemData.IconName);
		}
		else
		{
			SetImage(null);
		}
		SetPackageInfo();
		ShowTimeLeft();
	}

	private void LoadMarketingImage(string inURL)
	{
		ShowLoadingGear(show: true);
		if (string.IsNullOrEmpty(inURL))
		{
			OnImageLoaded(string.Empty, RsResourceLoadEvent.ERROR, 0f, null, null);
		}
		else if (inURL.StartsWith("http"))
		{
			string text = inURL;
			if (!UtUtilities.GetLocaleLanguage().Equals("en-US", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Replace("en-US", UtUtilities.GetLocaleLanguage());
			}
			RsResourceManager.Load(text, OnImageLoaded, RsResourceType.IMAGE, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: false, null, ignoreReferenceCount: true);
		}
		else
		{
			string[] array = inURL.Split('/');
			if (array.Length >= 3)
			{
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnImageLoaded, typeof(Texture));
			}
		}
	}

	private void OnImageLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			ShowLoadingGear(show: false);
			SetImage(inFile as Texture);
			break;
		case RsResourceLoadEvent.ERROR:
			ShowLoadingGear(show: false);
			SetImage(null);
			break;
		}
	}

	private void SetImage(Texture texture)
	{
		if (mPackageBanner != null)
		{
			mPackageBanner.SetTexture(texture);
		}
		if (mPackageBannerFull != null)
		{
			mPackageBannerFull.SetTexture(texture);
		}
	}

	private void ShowLoadingGear(bool show)
	{
		if (mImageLoadingGear == null && mPackageBanner != null)
		{
			mImageLoadingGear = mPackageBanner.FindChildItem("Loading");
		}
		if (mImageFullLoadingGear == null && mPackageBannerFull != null)
		{
			mImageFullLoadingGear = mPackageBannerFull.FindChildItem("Loading");
		}
		if (mImageLoadingGear != null)
		{
			mImageLoadingGear.SetVisibility(show);
		}
		if (mImageFullLoadingGear != null)
		{
			mImageFullLoadingGear.SetVisibility(show);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (!(item == mActionBtn) || mPackagesToShow.Count <= 0)
		{
			return;
		}
		if (mActionType == PromoActionType.BuyItem)
		{
			foreach (RankLockedData rankLockedDatum in _RankLockedData)
			{
				int rank = 0;
				if (IsRankLocked(out rank, mCurrentPackageItemData, rankLockedDatum._RankId))
				{
					string localizedString = rankLockedDatum._RankLockedText.GetLocalizedString();
					if (mCurrentPackageItemData.Points.HasValue && mCurrentPackageItemData.Points.Value > 0)
					{
						localizedString = _PointLockedText.GetLocalizedString();
						localizedString = localizedString.Replace("{{Points}}", rank.ToString());
						localizedString = localizedString.Replace("{{Item}}", mCurrentPackageItemData.ItemName);
					}
					else
					{
						localizedString = localizedString.Replace("{{RANK}}", rank.ToString());
					}
					GameUtilities.DisplayOKMessage("PfKAUIGenericDB", localizedString, null, "");
					return;
				}
			}
			ProcessPurchase();
		}
		else if (mActionType == PromoActionType.LoadScene)
		{
			string scene = mPackagesToShow[mPackageIndex].Scene;
			if (!string.IsNullOrEmpty(scene) && !RsResourceManager.pCurrentLevel.Equals(scene))
			{
				Exit();
				RsResourceManager.LoadLevel(scene);
			}
		}
		else if (mActionType == PromoActionType.LoadStore)
		{
			Exit();
			PromoPackage promoPackage = mPackagesToShow[mPackageIndex];
			if (int.TryParse(promoPackage.CategoryID, out var result) && int.TryParse(promoPackage.StoreID, out var result2))
			{
				StoreLoader.Load(setDefaultMenuItem: true, result, result2, null);
			}
			else
			{
				StoreLoader.Load(setDefaultMenuItem: true, promoPackage.CategoryID, promoPackage.StoreID, null);
			}
		}
		else if (mActionType == PromoActionType.View)
		{
			Exit();
		}
	}

	private bool IsRankLocked(out int rank, ItemData itemData, int rankType)
	{
		rank = 0;
		if (itemData.RewardTypeID > 0)
		{
			rankType = itemData.RewardTypeID;
		}
		if (itemData.Points.HasValue && itemData.Points.Value > 0)
		{
			rank = itemData.Points.Value;
			UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(rankType);
			if (userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue)
			{
				return rank > userAchievementInfoByType.AchievementPointTotal.Value;
			}
			return true;
		}
		if (itemData.RankId.HasValue && itemData.RankId.Value > 0)
		{
			rank = itemData.RankId.Value;
			UserRank userRank = ((rankType == 8) ? PetRankData.GetUserRank(SanctuaryManager.pCurPetData) : UserRankData.GetUserRankByType(rankType));
			if (userRank != null)
			{
				return rank > userRank.RankID;
			}
			return true;
		}
		return false;
	}

	public void ShowPackages(int currentIndex, List<PromoPackage> promoPackages, GameObject messageObject)
	{
		mMessageObject = messageObject;
		mPackagesToShow = promoPackages;
		Init();
		ShowNextOffer(currentIndex);
		mPromoOfferStoreName = ItemPurchaseSource.PROMO_PANEL.ToString();
	}

	protected override void ShowCurrentOffer()
	{
		SetImage(null);
		ShowLoadingGear(show: true);
		if (mTxtPackageDes != null)
		{
			mTxtPackageDes.SetVisibility(inVisible: true);
		}
		if (mTxtPackageTitle != null)
		{
			mTxtPackageTitle.SetVisibility(inVisible: true);
		}
		SetActionData(mPackagesToShow[mPackageIndex].ActionType.GetValueOrDefault(PromoActionType.BuyItem));
		if (mTxtPackageDes != null && mPackagesToShow[mPackageIndex].HideInfo)
		{
			mTxtPackageDes.SetVisibility(inVisible: false);
		}
		if (mTxtPackageTitle != null && mPackagesToShow[mPackageIndex].HideInfo)
		{
			mTxtPackageTitle.SetVisibility(inVisible: false);
		}
		base.ShowCurrentOffer();
	}

	protected override void ShowTimeLeft()
	{
		mTimerAvailable = false;
		if (mPackagesToShow[mPackageIndex].EndDate != DateTime.MinValue && mPackagesToShow[mPackageIndex].EndDate >= ServerTime.pCurrentTime)
		{
			mEndTime = mPackagesToShow[mPackageIndex].EndDate;
			mTimerAvailable = true;
		}
		mTxtTimeLeft.SetVisibility(mTimerAvailable);
		mIcoTimer.SetVisibility(mTimerAvailable);
		UpdateDisplayTime();
	}

	protected override void Update()
	{
		if (!mTimerAvailable)
		{
			return;
		}
		mTimer += Time.deltaTime;
		if (mTimer > mTimeUpdateInterval)
		{
			mTimer = 0f;
			UpdateDisplayTime();
			if (mActionBtn != null && mActionBtn.GetState() == KAUIState.INTERACTIVE && mEndTime < ServerTime.pCurrentTime)
			{
				mActionBtn.SetDisabled(isDisabled: true);
			}
		}
	}

	private void UpdateDisplayTime()
	{
		if (mEndTime < ServerTime.pCurrentTime)
		{
			mTxtTimeLeft.SetText(UtUtilities.GetFormattedTime(TimeSpan.Zero, "D ", "H ", "M ", "S"));
		}
		else
		{
			mTxtTimeLeft.SetText(UtUtilities.GetFormattedTime(mEndTime - ServerTime.pCurrentTime, "D ", "H ", "M ", "S"));
		}
	}

	protected override void Exit()
	{
		if (mPreviousAvatarState != 0)
		{
			AvAvatar.pState = mPreviousAvatarState;
		}
		AvAvatar.SetUIActive(inActive: true);
		KAUI.RemoveExclusive(this);
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnPromoPanelClosed", SendMessageOptions.DontRequireReceiver);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
