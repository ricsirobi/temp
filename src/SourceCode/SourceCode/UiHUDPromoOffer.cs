using System;
using System.Collections.Generic;
using UnityEngine;

public class UiHUDPromoOffer : KAUI
{
	public List<PromoPackageTriggerType> _PromoTriggerTypes = new List<PromoPackageTriggerType>();

	public float _OfferIgnoreTimeInMinutes = 1f;

	public float _ButtonScaleDuration = 0.5f;

	public float _OfferShowcaseDuration = 6f;

	public float _AlertTimeInMinutes = 5f;

	public Color _AlertTimeColor = Color.red;

	public Texture _DefaultTexture;

	public string _PanelResourcePath = "RS_DATA/PfUiPromoPanelDO.unity3d/PfUiPromoPanelDO";

	private KAWidget mHUDPromoOfferBtn;

	private KAWidget mTimer;

	private bool mDataInitialized;

	private bool mCheckPackages;

	private bool mShowcaseOffers = true;

	private bool mButtonReady;

	private bool mIconLoading;

	private List<PromoPackage> mPackagesToShow = new List<PromoPackage>();

	private List<PromoPackage> mLastCachedPackages = new List<PromoPackage>();

	private PromoPackage mCurrentPackage;

	private Vector3 mOriginalScale = Vector3.one;

	private Texture mTexture;

	private Color mTimerOriginalColor = Color.white;

	private float mShowcaseTime;

	private int mPackagesToLoad;

	private readonly float mOfferUpdateInterval = 0.5f;

	private void OnSceneReady()
	{
		mCheckPackages = true;
	}

	protected override void Awake()
	{
		base.Awake();
		mTexture = _DefaultTexture;
		mShowcaseTime = _OfferShowcaseDuration;
		CoCommonLevel.WaitListCompleted += OnSceneReady;
	}

	protected override void Start()
	{
		base.Start();
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		mHUDPromoOfferBtn = FindItem("BtnHUDPromoOffer");
		mTimer = FindItem("TxtTimer");
		if (mHUDPromoOfferBtn != null)
		{
			mOriginalScale = mHUDPromoOfferBtn.GetScale();
			mHUDPromoOfferBtn.SetScale(Vector3.zero);
		}
		if (mTimer != null)
		{
			mTimerOriginalColor = mTimer.GetLabel().color;
		}
		SetIcon();
	}

	private void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent == MissionEvent.TASK_COMPLETE || inEvent == MissionEvent.MISSION_COMPLETE)
		{
			if (!mShowcaseOffers)
			{
				mShowcaseOffers = true;
				mShowcaseTime = _OfferShowcaseDuration;
			}
			mCheckPackages = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		SetVisibility(!FUEManager.pIsFUERunning && InteractiveTutManager._CurrentActiveTutorialObject == null);
		if (mCheckPackages && !DailyBonusAndPromo.pIsReady && !mDataInitialized)
		{
			mDataInitialized = true;
			DailyBonusAndPromo.Init();
		}
		if (DailyBonusAndPromo.pIsReady && GetVisibility())
		{
			if (mCheckPackages && !RsResourceManager.pLevelLoading)
			{
				mCheckPackages = false;
				PopulatePackages();
			}
			if (mPackagesToShow.Count > 0)
			{
				if (mPackagesToLoad <= 0 && !mIconLoading)
				{
					PromoPackage promoPackage = mCurrentPackage;
					if (mShowcaseOffers)
					{
						if (mShowcaseTime >= _OfferShowcaseDuration)
						{
							mShowcaseTime = 0f;
							promoPackage = GetNextPackage();
							if (promoPackage == null)
							{
								mShowcaseTime = 0f;
								mShowcaseOffers = false;
								promoPackage = GetActivePackage();
							}
							ShowPackage(promoPackage);
						}
					}
					else if (mShowcaseTime >= mOfferUpdateInterval)
					{
						mShowcaseTime = 0f;
						ShowPackage(GetActivePackage());
					}
					mShowcaseTime += Time.deltaTime;
				}
			}
			else if (mCurrentPackage != null)
			{
				ShowPackage(GetActivePackage());
			}
		}
		if (mTimer != null && mCurrentPackage != null && mCurrentPackage.EndDate != DateTime.MinValue && mCurrentPackage.EndDate >= ServerTime.pCurrentTime)
		{
			if (!mTimer.GetVisibility() && mButtonReady)
			{
				mTimer.SetVisibility(inVisible: true);
			}
			mTimer.SetText(UtUtilities.GetFormattedTime(mCurrentPackage.EndDate - ServerTime.pCurrentTime, "D ", "H ", "M ", "S"));
			if (mTimer.GetLabel().color != _AlertTimeColor && (mCurrentPackage.EndDate - ServerTime.pCurrentTime).TotalMinutes <= (double)_AlertTimeInMinutes)
			{
				mTimer.GetLabel().color = _AlertTimeColor;
			}
		}
	}

	private void ShowPackage(PromoPackage package)
	{
		if (package != mCurrentPackage)
		{
			mCurrentPackage = package;
			if (package != null)
			{
				LoadImage(package.IconRes);
			}
			TransitOfferButton();
			if (mTimer != null)
			{
				mTimer.GetLabel().color = mTimerOriginalColor;
				mTimer.SetVisibility(inVisible: false);
			}
		}
	}

	private void PopulatePackages()
	{
		List<PromoPackage> list = DailyBonusAndPromo.pInstance.CheckForPromoPackageOffers(_PromoTriggerTypes);
		if (SameList(list, mLastCachedPackages))
		{
			return;
		}
		mLastCachedPackages = list;
		mPackagesToShow.Clear();
		mPackagesToLoad = 0;
		if (mLastCachedPackages == null || mLastCachedPackages.Count <= 0)
		{
			return;
		}
		foreach (PromoPackage mLastCachedPackage in mLastCachedPackages)
		{
			if (mLastCachedPackage.EndDate != DateTime.MinValue)
			{
				if (mLastCachedPackage.pOfferShowcased || (mLastCachedPackage.EndDate - ServerTime.pCurrentTime).TotalMinutes >= (double)_OfferIgnoreTimeInMinutes)
				{
					mPackagesToShow.Add(mLastCachedPackage);
				}
			}
			else if (mLastCachedPackage.Duration.HasValue && mLastCachedPackage.Duration.GetValueOrDefault(0) > 0)
			{
				if (mLastCachedPackage.IsNewPackage())
				{
					mLastCachedPackage.StartPackage();
				}
				string offerStartTime = mLastCachedPackage.GetOfferStartTime();
				if (offerStartTime != null)
				{
					DateTime result = DateTime.MinValue;
					if (DateTime.TryParse(offerStartTime, out result))
					{
						mLastCachedPackage.EndDate = result.AddHours(mLastCachedPackage.Duration.Value);
						mPackagesToShow.Add(mLastCachedPackage);
					}
				}
			}
			else
			{
				mPackagesToShow.Add(mLastCachedPackage);
			}
		}
		LoadPackages();
	}

	private void LoadPackages()
	{
		List<PromoPackage> list = mPackagesToShow.FindAll((PromoPackage p) => p.ItemID > 0);
		if (list != null)
		{
			mPackagesToLoad = list.Count;
			{
				foreach (PromoPackage item in list)
				{
					item.LoadPackageContent(OnPackageLoaded, forceLoad: true);
				}
				return;
			}
		}
		SortPackagesToShow();
	}

	private void OnPackageLoaded(int itemID, bool userHasItem)
	{
		if (userHasItem)
		{
			mPackagesToShow.RemoveAll((PromoPackage p) => p.ItemID == itemID);
		}
		mPackagesToLoad--;
		SortPackagesToShow();
	}

	private void SortPackagesToShow()
	{
		if (mPackagesToLoad <= 0)
		{
			List<PromoPackage> list = mPackagesToShow.FindAll((PromoPackage t) => t.EndDate != DateTime.MinValue);
			List<PromoPackage> list2 = mPackagesToShow.FindAll((PromoPackage p) => p.EndDate == DateTime.MinValue);
			mPackagesToShow.Clear();
			if (list2 != null)
			{
				list2.Sort(OrderByPriority);
				mPackagesToShow.AddRange(list2);
			}
			if (list != null)
			{
				list.Sort(OrderByTime);
				mPackagesToShow.AddRange(list);
			}
		}
	}

	private int OrderByPriority(PromoPackage p1, PromoPackage p2)
	{
		return p2.Priority.CompareTo(p1.Priority);
	}

	private int OrderByTime(PromoPackage p1, PromoPackage p2)
	{
		return p2.EndDate.CompareTo(p1.EndDate);
	}

	private bool SameList(List<PromoPackage> list1, List<PromoPackage> list2)
	{
		if (list1 == null || list2 == null || list1.Count != list2.Count)
		{
			return false;
		}
		return list1.Find((PromoPackage x) => !list2.Contains(x)) == null;
	}

	private PromoPackage GetNextPackage()
	{
		if (mPackagesToShow.Count == 0)
		{
			return null;
		}
		foreach (PromoPackage item in mPackagesToShow)
		{
			if (TimeValid(item) && !item.pOfferShowcased)
			{
				item.pOfferShowcased = true;
				return item;
			}
		}
		return null;
	}

	private PromoPackage GetActivePackage()
	{
		if (mPackagesToShow.Count == 0)
		{
			return null;
		}
		for (int num = mPackagesToShow.Count - 1; num >= 0; num--)
		{
			if (TimeValid(mPackagesToShow[num]))
			{
				return mPackagesToShow[num];
			}
		}
		return null;
	}

	private List<PromoPackage> GetPackages()
	{
		List<PromoPackage> list = new List<PromoPackage>();
		foreach (PromoPackage item in mPackagesToShow)
		{
			if (TimeValid(item))
			{
				list.Insert(0, item);
			}
		}
		return list;
	}

	private bool TimeValid(PromoPackage package)
	{
		if (package.StartDate != DateTime.MinValue && package.StartDate > ServerTime.pCurrentTime)
		{
			return false;
		}
		if (package.EndDate != DateTime.MinValue && package.EndDate <= ServerTime.pCurrentTime)
		{
			return false;
		}
		return true;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mHUDPromoOfferBtn && GetPackages().Count > 0)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _PanelResourcePath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnPanelLoaded, typeof(GameObject));
		}
	}

	private void OnPanelLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			UiPromoPanel component = UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<UiPromoPanel>();
			List<PromoPackage> packages = GetPackages();
			component.ShowPackages(packages.IndexOf(mCurrentPackage), packages, base.gameObject);
			SetVisibility(inVisible: false);
		}
	}

	private void TransitOfferButton()
	{
		if (mHUDPromoOfferBtn != null)
		{
			mButtonReady = false;
			mHUDPromoOfferBtn.SetVisibility(inVisible: true);
			mHUDPromoOfferBtn.ScaleTo(mHUDPromoOfferBtn.GetScale(), Vector3.zero, _ButtonScaleDuration);
		}
	}

	public override void EndScaleTo(KAWidget widget)
	{
		base.EndScaleTo(widget);
		if (widget == mHUDPromoOfferBtn)
		{
			if (mHUDPromoOfferBtn.GetScale() != Vector3.zero)
			{
				mButtonReady = true;
			}
			if (mCurrentPackage == null)
			{
				mHUDPromoOfferBtn.SetVisibility(inVisible: false);
			}
			else if (!mIconLoading && mHUDPromoOfferBtn.GetScale() == Vector3.zero)
			{
				SetIcon();
				mHUDPromoOfferBtn.ScaleTo(Vector3.zero, mOriginalScale, _ButtonScaleDuration);
			}
		}
	}

	private void LoadImage(string inURL)
	{
		mIconLoading = true;
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
			mIconLoading = false;
			mTexture = inFile as Texture;
			TransitOfferButton();
			break;
		case RsResourceLoadEvent.ERROR:
			mIconLoading = false;
			mTexture = _DefaultTexture;
			TransitOfferButton();
			break;
		}
	}

	private void SetIcon()
	{
		if (mHUDPromoOfferBtn != null && mTexture != null)
		{
			mHUDPromoOfferBtn.SetTexture(mTexture);
		}
	}

	private void OnPromoPanelClosed()
	{
		mCheckPackages = true;
		mLastCachedPackages.Clear();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CoCommonLevel.WaitListCompleted -= OnSceneReady;
	}
}
