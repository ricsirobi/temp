public class UiDragonAgeUpItem : KAWidget
{
	public KAUIMenu _UiPowersMenu;

	public KAUIMenu _UiClassMenu;

	public LocaleString _PowerUpsHeaderText = new LocaleString("Dragon Power Ups");

	public LocaleString _ClassUnlockedHeaderText = new LocaleString("Classes Unlocked");

	private int mAgeUpItemCost;

	private PetAgeUpData mPetAgeUpData;

	private KAUIGenericDB mKAUIGenericDB;

	private KAWidget mBuyBtn;

	private KAWidget mIconGems;

	private KAWidget mUseTicketText;

	private KAWidget mDisableBg;

	private KAWidget mTxtComingSoon;

	private UiDragonAgeUp mAgeUpParent;

	private SanctuaryPetTypeInfo mTypeInfo;

	private bool mIsAgeUpDone;

	private bool mAgeUpDisabled;

	public int pAgeUpItemCost => mAgeUpItemCost;

	public bool pAgeUpDisabled
	{
		get
		{
			return mAgeUpDisabled;
		}
		set
		{
			mAgeUpDisabled = value;
		}
	}

	public PetAgeUpData pPetAgeUpData => mPetAgeUpData;

	public void Init(SanctuaryPetTypeInfo inTypeInfo, PetAgeUpData inAgeUpData, int itemCost, UiDragonAgeUp inParent, bool isAgeUpDone)
	{
		mTypeInfo = inTypeInfo;
		mPetAgeUpData = inAgeUpData;
		mAgeUpItemCost = itemCost;
		mAgeUpParent = inParent;
		mIsAgeUpDone = isAgeUpDone;
	}

	private void Start()
	{
		mIconGems = FindChildItem("IconGems");
		if (mIconGems != null)
		{
			mIconGems.SetVisibility(inVisible: false);
		}
		mBuyBtn = FindChildItem("BuyBtn");
		if (mBuyBtn != null)
		{
			mBuyBtn.SetVisibility(inVisible: false);
		}
		mUseTicketText = FindChildItem("TxtUseTicket");
		if (mUseTicketText != null)
		{
			mUseTicketText.SetVisibility(inVisible: false);
		}
		mDisableBg = FindChildItem("AniDisabled");
		if (mDisableBg != null)
		{
			mDisableBg.SetVisibility(inVisible: false);
		}
		mTxtComingSoon = FindChildItem("AniComingSoon");
		if (mTxtComingSoon != null)
		{
			mTxtComingSoon.SetVisibility(inVisible: false);
		}
		KAWidget kAWidget = FindChildItem("TxtDragonPowersHeader");
		if (kAWidget != null && mPetAgeUpData != null)
		{
			kAWidget.SetTextByID(mPetAgeUpData._AgeUpText._ID, mPetAgeUpData._AgeUpText._Text);
		}
		kAWidget = FindChildItem("");
		Refresh();
	}

	public void Refresh()
	{
		if (mPetAgeUpData == null)
		{
			return;
		}
		UpdateAgeUpProperties();
		if ((mAgeUpParent.pRaisedPetData.pStage == RaisedPetStage.BABY || mAgeUpParent.pRaisedPetData.pStage == RaisedPetStage.TEEN) && mPetAgeUpData._ToPetStage == RaisedPetStage.TITAN)
		{
			SetDisabled(isDisabled: true);
			return;
		}
		int ageIndex = RaisedPetData.GetAgeIndex(mAgeUpParent.pRaisedPetData.pStage);
		int ageIndex2 = RaisedPetData.GetAgeIndex(mPetAgeUpData._ToPetStage);
		if (ageIndex2 >= mTypeInfo._AgeData.Length)
		{
			SetDisabled(isDisabled: true);
			if (mTxtComingSoon != null)
			{
				mTxtComingSoon.SetVisibility(inVisible: true);
			}
			return;
		}
		if (ageIndex >= ageIndex2)
		{
			if (mIsAgeUpDone)
			{
				SetDisabled(isDisabled: false);
				mAgeUpParent.ResetUI();
				UpdateWidget();
			}
			else
			{
				SetDisabled(isDisabled: true);
			}
			return;
		}
		if (!mAgeUpParent.HasTicket(mPetAgeUpData._AgeUpItemID) && !mAgeUpParent.HasTicket(mPetAgeUpData._AgeUpTicketID))
		{
			KAWidget kAWidget = FindChildItem("TxtGemsAmount");
			if (kAWidget != null)
			{
				kAWidget.SetText(mAgeUpItemCost.ToString());
				kAWidget.SetVisibility(inVisible: true);
			}
			if (mIconGems != null)
			{
				mIconGems.SetVisibility(inVisible: true);
			}
			if (mUseTicketText != null)
			{
				mUseTicketText.SetVisibility(inVisible: false);
			}
		}
		else
		{
			if (mIconGems != null)
			{
				mIconGems.SetVisibility(inVisible: false);
			}
			if (mUseTicketText != null)
			{
				mUseTicketText.SetVisibility(inVisible: true);
			}
		}
		mBuyBtn.SetVisibility(inVisible: true);
		SetDisabled(mAgeUpDisabled);
	}

	public override void SetDisabled(bool isDisabled)
	{
		base.SetDisabled(isDisabled);
		mDisableBg.SetVisibility(isDisabled);
	}

	public void UpdateWidget()
	{
		KAWidget kAWidget = FindChildItem("TxtDragonPowersHeader");
		if (kAWidget != null)
		{
			kAWidget.SetTextByID(_PowerUpsHeaderText._ID, _PowerUpsHeaderText._Text);
		}
		kAWidget = FindChildItem("TxtDragonClassesHeader");
		if (kAWidget != null)
		{
			kAWidget.SetTextByID(_ClassUnlockedHeaderText._ID, _ClassUnlockedHeaderText._Text);
		}
		if (mBuyBtn != null)
		{
			mBuyBtn.SetVisibility(inVisible: false);
		}
		if (mUseTicketText != null)
		{
			mUseTicketText.SetVisibility(inVisible: false);
		}
		kAWidget = FindChildItem("IconGems");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		kAWidget = FindChildItem("TxtGems");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		kAWidget = FindChildItem("TxtGemsAmount");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
	}

	public void UpdateAgeUpProperties()
	{
		_UiPowersMenu.ClearItems();
		_UiClassMenu.ClearItems();
		LocaleString[] unlockPowers = mPetAgeUpData._UnlockPowers;
		foreach (LocaleString localeString in unlockPowers)
		{
			KAWidget kAWidget = _UiPowersMenu.AddWidget(_UiPowersMenu._Template.name);
			kAWidget.name = localeString._Text;
			kAWidget.SetTextByID(localeString._ID, localeString._Text);
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.SetState(KAUIState.INTERACTIVE);
		}
		unlockPowers = mPetAgeUpData._UnlockClasses;
		foreach (LocaleString localeString2 in unlockPowers)
		{
			KAWidget kAWidget2 = _UiClassMenu.AddWidget(_UiClassMenu._Template.name);
			kAWidget2.name = localeString2._Text;
			kAWidget2.SetTextByID(localeString2._ID, localeString2._Text);
			kAWidget2.SetVisibility(inVisible: true);
			kAWidget2.SetState(KAUIState.INTERACTIVE);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBuyBtn)
		{
			ConfirmYes();
		}
	}

	public void ConfirmYes()
	{
		mAgeUpParent.DestroyDB();
		mAgeUpParent.BuyAgeUp(this);
	}

	public void DestroyDB()
	{
		mAgeUpParent.DestroyDB();
	}
}
