public class UiWorldEventConsumables : UiConsumable
{
	public string _HealthConsumableItemName = "HealthPotion";

	public UiPowerupSelect _UIPowerUpBuyPopUp;

	private AvAvatarController mAvatarController;

	private KAWidget mHealthPotion;

	protected bool mIsEventOn;

	public bool pIsEventOn
	{
		get
		{
			return mIsEventOn;
		}
		set
		{
			mIsEventOn = value;
			if (mIsEventOn)
			{
				UpdateCarryConsumableUI();
			}
			else if (_UIPowerUpBuyPopUp.GetVisibility())
			{
				OnCloseUI();
			}
			SetVisibility(mIsEventOn);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (AvAvatar.pObject != null)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if (AvAvatar.pToolbar != null)
		{
			base.transform.parent = AvAvatar.pToolbar.transform;
		}
		_DefaultButtonClickable = true;
		_UIPowerUpBuyPopUp._MessageObject = base.gameObject;
	}

	protected override void Update()
	{
		base.Update();
		if (!pIsEventOn)
		{
			return;
		}
		if (mHealthPotion == null)
		{
			mHealthPotion = FindItem(_HealthConsumableItemName);
		}
		else if (mAvatarController != null)
		{
			KAWidget kAWidget = mHealthPotion.FindChildItem("TxtQuanity");
			if (!(kAWidget != null))
			{
				return;
			}
			int result = 0;
			int.TryParse(kAWidget.GetText(), out result);
			ConsumableUserData consumableUserData = (ConsumableUserData)mHealthPotion.GetUserData();
			if (consumableUserData != null && consumableUserData._CoolDown < 0.001f)
			{
				if (SanctuaryManager.pCurPetInstance != null)
				{
					mHealthPotion.SetDisabled(SanctuaryManager.pCurPetInstance.IsMeterFull(SanctuaryPetMeterType.HEALTH) && result != 0);
				}
				else
				{
					mHealthPotion.SetDisabled(mAvatarController._Stats._CurrentHealth >= mAvatarController._Stats._MaxHealth && result != 0);
				}
			}
		}
		else
		{
			UtDebug.LogError("Avatar Controller is null");
		}
	}

	protected void OnCloseUI()
	{
		SetState(enable: true);
	}

	protected void OnShowGenericDB()
	{
		_UIPowerUpBuyPopUp.SetVisibility(visibility: false);
	}

	protected void OnIAPStoreClosed()
	{
		SetState(enable: false);
	}

	protected void OnNotEnoughGems()
	{
		SetState(enable: true);
	}

	protected void OnPurchaseSuccess()
	{
		SetState(enable: true);
	}

	protected void OnPurchaseFailed()
	{
		SetState(enable: true);
	}

	protected override void OnConsumablesEmpty()
	{
		SetState(enable: false);
	}

	protected void SetState(bool enable)
	{
		if (enable)
		{
			if (AvAvatar.pPrevState != AvAvatarState.PAUSED)
			{
				AvAvatar.pState = AvAvatar.pPrevState;
				KAUI.RemoveExclusive(_UIPowerUpBuyPopUp);
			}
		}
		else if (AvAvatar.pState != AvAvatarState.PAUSED)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUI.SetExclusive(_UIPowerUpBuyPopUp);
		}
		_UIPowerUpBuyPopUp.SetVisibility(!enable);
	}
}
