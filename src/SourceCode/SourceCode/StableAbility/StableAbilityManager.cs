using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StableAbility;

public class StableAbilityManager : MonoBehaviour
{
	public static StableAbilityManager pInstance;

	public int _PairDataID = 2014;

	public List<BaseAbility> _Abilities;

	[Tooltip("Activation time in minutes.")]
	public float _ActivateTimespan = 15f;

	public LocaleString _AbilityPurchaseFailedText = new LocaleString("Purchase failed!");

	public LocaleString _UnclaimedRewardsText = new LocaleString("This Ability cannot be Activated as you have Unclaimed Rewards");

	public LocaleString _ServerErrorTitleText = new LocaleString("An Error Occurred");

	public LocaleString _ServerErrorBodyText = new LocaleString("Something went wrong while attempting ");

	private List<StoreData> mStoreDatas = new List<StoreData>();

	private int mCurrentAbilityIndex;

	private PairData mAbilityPairData;

	private bool mLastStoreItem;

	private int mTicketsToBuy;

	private ObStatus mObStatus;

	private void Start()
	{
		if (!pInstance)
		{
			pInstance = this;
		}
		mObStatus = GetComponent<ObStatus>();
		StartCoroutine("SetObStatus");
		PairData.Load(_PairDataID, AbilityDataLoadCallback, null);
		List<int> list = new List<int>();
		foreach (BaseAbility ability in _Abilities)
		{
			list.Add(ability._TicketStoreID);
		}
		ItemStoreDataLoader.Load(list.ToArray(), OnStoreLoaded, null);
	}

	public void OnStoreLoaded(List<StoreData> inStoreData, object inUserData)
	{
		mStoreDatas = inStoreData;
	}

	private IEnumerator SetObStatus()
	{
		while (mStoreDatas == null && mAbilityPairData == null)
		{
			yield return null;
		}
		mObStatus.pIsReady = true;
	}

	private void AbilityDataLoadCallback(bool success, PairData inData, object inUserData)
	{
		if (success)
		{
			StartCoroutine(InitAbilities(success: true, inData, inUserData));
		}
		else
		{
			UtDebug.LogError("PairData failed to load!!");
		}
	}

	private IEnumerator InitAbilities(bool success, PairData inData, object inUserData)
	{
		mAbilityPairData = inData;
		foreach (BaseAbility ability in _Abilities)
		{
			while (ability.gameObject == null)
			{
				yield return new WaitForEndOfFrame();
			}
			ability.OnPairDataLoaded(success, inData, inUserData);
			if (mAbilityPairData.FindByKey(ability._Ability.ToString()) != null && AbilityCooldownReady(ability) && ability._SceneObject != null)
			{
				ability._SceneObject.GetComponentInChildren<ParticleSystem>()?.Play();
			}
		}
		yield return null;
	}

	private ItemData GetItemData(int inItemID)
	{
		return mStoreDatas.SelectMany((StoreData sd) => sd._Items).FirstOrDefault((ItemData id) => id.ItemID == inItemID);
	}

	public TimeSpan GetAbilityTimeRemaining(BaseAbility inAbility)
	{
		Pair pair = mAbilityPairData?.FindByKey(inAbility._Ability.ToString());
		if (pair == null)
		{
			return TimeSpan.Zero;
		}
		return DateTime.Parse(pair.PairValue, UtUtilities.GetCultureInfo("en-US")).AddMinutes(inAbility._CooldownTime) - ServerTime.pCurrentTime;
	}

	private TimeSpan GetTimeSinceAbilityActivated(BaseAbility inAbility)
	{
		Pair pair = mAbilityPairData?.FindByKey(inAbility._Ability.ToString());
		if (pair == null)
		{
			return TimeSpan.Zero;
		}
		DateTime dateTime = DateTime.Parse(pair.PairValue, UtUtilities.GetCultureInfo("en-US"));
		return ServerTime.pCurrentTime - dateTime;
	}

	public string GetActivateAbilityFailReason(BaseAbility inAbility)
	{
		switch (inAbility._Ability)
		{
		case ABILITY.FISH_SPRAY:
			if (!inAbility.CanActivate())
			{
				return _UnclaimedRewardsText.GetLocalizedString();
			}
			break;
		case ABILITY.KING_OF_DRAGONS:
			if (!inAbility.CanActivate())
			{
				return _UnclaimedRewardsText.GetLocalizedString();
			}
			break;
		}
		return string.Empty;
	}

	public bool CanAffordAbility(BaseAbility inAbility)
	{
		return Money.pCashCurrency > GetAbilityCost(inAbility);
	}

	public int GetAbilityCost(BaseAbility inAbility)
	{
		return GetTicketsForCooldown(inAbility) * GetItemData(inAbility._TicketItemID).CashCost;
	}

	public bool AbilityCooldownReady(BaseAbility inAbility)
	{
		TimeSpan abilityTimeRemaining = GetAbilityTimeRemaining(inAbility);
		if (!inAbility)
		{
			return false;
		}
		return abilityTimeRemaining <= TimeSpan.Zero;
	}

	private int GetTicketsForCooldown(BaseAbility inAbility)
	{
		double totalMinutes = GetTimeSinceAbilityActivated(inAbility).TotalMinutes;
		if (!(totalMinutes > 0.0))
		{
			return 0;
		}
		return Mathf.CeilToInt((float)(((double)inAbility._CooldownTime - totalMinutes) / (double)_ActivateTimespan));
	}

	public void SaveAbility(BaseAbility inAbility)
	{
		mAbilityPairData.SetValue(inAbility._Ability.ToString(), ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US")));
		PairData.Save(_PairDataID);
		mCurrentAbilityIndex = -1;
		if (!string.IsNullOrEmpty(inAbility._ActionName))
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", inAbility._ActionName, base.gameObject.name);
		}
	}

	public void PurchaseAbility(BaseAbility inAbility)
	{
		mCurrentAbilityIndex = _Abilities.IndexOf(inAbility);
		if (mAbilityPairData != null && !(inAbility == null) && mCurrentAbilityIndex != -1)
		{
			mTicketsToBuy = GetTicketsForCooldown(inAbility);
			if (mTicketsToBuy <= 0)
			{
				inAbility.ActivateAbility();
			}
			else
			{
				CommonInventoryPurchaseAbility(inAbility, mTicketsToBuy);
			}
		}
	}

	private void CommonInventoryPurchaseAbility(BaseAbility inAbility, int ticketsToBuy)
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(inAbility._TicketItemID, ticketsToBuy);
		CommonInventoryData.pInstance.DoPurchase(2, inAbility._TicketStoreID, OnPurchaseAbilityTicket);
	}

	public void OnPurchaseAbilityTicket(CommonInventoryResponse invResponse)
	{
		if (invResponse != null && invResponse.Success)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (mCurrentAbilityIndex > -1 && mCurrentAbilityIndex <= _Abilities.Count)
			{
				_Abilities[mCurrentAbilityIndex].ActivateAbility();
				mTicketsToBuy = 0;
			}
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _ServerErrorBodyText.GetLocalizedString(), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, "OnAttemptRetry", "OnCloseDB", "", "", inDestroyOnClick: true);
		}
	}

	private void OnAttemptRetry()
	{
		PurchaseAbility(_Abilities[mCurrentAbilityIndex]);
	}

	private void OnCloseDB()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}
}
