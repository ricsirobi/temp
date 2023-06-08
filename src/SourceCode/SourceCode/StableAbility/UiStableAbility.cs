using System;
using UnityEngine;

namespace StableAbility;

public class UiStableAbility : KAUI
{
	public KAWidget _AbilityTitleWidget;

	public KAWidget _AbilityImageWidget;

	public KAWidget _AbilityDescriptionWidget;

	public KAWidget _AbilityRewardDescriptionWidget;

	public KAWidget _AbilityRechargingWidget;

	public KAButton _ActivateBtn;

	public KAButton _CloseBtn;

	public LocaleString _TimeRemainingText = new LocaleString("Time Remaining:\n{0}");

	public LocaleString _AbilityReadyText = new LocaleString("Ability Ready");

	public LocaleString _ActivateBtnText = new LocaleString("Activate");

	public LocaleString _ActivateNowText = new LocaleString("Activate now: \n{0} Gems");

	public LocaleString _ConfirmActiveAbilityPurchaseText = new LocaleString("This ability is currently active. Refill this ability for {0} Gems?");

	public LocaleString _InsufficientFundsText = new LocaleString("Insufficient gems. Would you like to buy more?");

	private BaseAbility mCurrentAbility;

	public Action UiStableAbilityClosed;

	public void Init(ABILITY inAbility)
	{
		KAUI.SetExclusive(this);
		if ((bool)StableAbilityManager.pInstance)
		{
			mCurrentAbility = StableAbilityManager.pInstance._Abilities.Find((BaseAbility t) => t._Ability == inAbility);
			_AbilityTitleWidget.SetText(mCurrentAbility._NameText.GetLocalizedString());
			_AbilityDescriptionWidget.SetText(mCurrentAbility._DescriptionText.GetLocalizedString());
			_AbilityRewardDescriptionWidget.SetText(mCurrentAbility._RewardText.GetLocalizedString());
			UpdateTimeRemainingText();
			InvokeRepeating("UpdateTimeRemainingText", 1f, 1f);
			if (mCurrentAbility._Image != null)
			{
				_AbilityImageWidget.SetTexture(mCurrentAbility._Image);
			}
		}
	}

	private void UpdateTimeRemainingText()
	{
		if (!(mCurrentAbility == null))
		{
			TimeSpan abilityTimeRemaining = StableAbilityManager.pInstance.GetAbilityTimeRemaining(mCurrentAbility);
			string text = string.Format(_ActivateNowText.GetLocalizedString(), StableAbilityManager.pInstance.GetAbilityCost(mCurrentAbility).ToString());
			string text2 = GameUtilities.FormatTimeHHMMSS((int)abilityTimeRemaining.TotalSeconds);
			string text3 = string.Format(_TimeRemainingText.GetLocalizedString(), (abilityTimeRemaining <= TimeSpan.Zero) ? _AbilityReadyText.GetLocalizedString() : text2);
			_ActivateBtn.SetText((abilityTimeRemaining > TimeSpan.Zero) ? text : _ActivateBtnText.GetLocalizedString());
			_AbilityRechargingWidget.SetText(text3);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget == _ActivateBtn && mCurrentAbility != null && StableAbilityManager.pInstance != null)
		{
			string activateAbilityFailReason = StableAbilityManager.pInstance.GetActivateAbilityFailReason(mCurrentAbility);
			if (!string.IsNullOrEmpty(activateAbilityFailReason))
			{
				SetVisibility(inVisible: false);
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", activateAbilityFailReason, base.gameObject, "OnCloseDB").SetDestroyOnClick(isDestroy: true);
				return;
			}
			if (StableAbilityManager.pInstance.AbilityCooldownReady(mCurrentAbility))
			{
				OnClose();
				mCurrentAbility.ActivateAbility();
				return;
			}
			if (StableAbilityManager.pInstance.CanAffordAbility(mCurrentAbility))
			{
				string inText = string.Format(_ConfirmActiveAbilityPurchaseText.GetLocalizedString(), StableAbilityManager.pInstance.GetAbilityCost(mCurrentAbility).ToString());
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", inText, "", base.gameObject, "OnConfirmationAccept", "OnCloseDB", null, null).SetDestroyOnClick(isDestroy: true);
			}
			else
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _InsufficientFundsText.GetLocalizedString(), "", base.gameObject, "OnOpenStoreConfirm", "OnCloseDB", null, null).SetDestroyOnClick(isDestroy: true);
			}
			SetVisibility(inVisible: false);
		}
		else if (inWidget == _CloseBtn)
		{
			OnClose();
		}
	}

	private void OnOpenStoreConfirm()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
		SetVisibility(inVisible: false);
	}

	private void OnConfirmationAccept()
	{
		OnClose();
		StableAbilityManager.pInstance.PurchaseAbility(mCurrentAbility);
	}

	private void OnCloseDB()
	{
		SetVisibility(inVisible: true);
	}

	public void OnIAPStoreClosed()
	{
		SetVisibility(inVisible: true);
	}

	private void OnClose()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUI.RemoveExclusive(this);
		UiStableAbilityClosed?.Invoke();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
