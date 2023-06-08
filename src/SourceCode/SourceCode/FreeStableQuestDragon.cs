using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStableQuestDragon : MonoBehaviour
{
	public delegate void OnDragonFree();

	private KAUIGenericDB mGenericDB;

	private static FreeStableQuestDragon mInstance;

	private OnDragonFree mCallback;

	private TimedMissionSlotData mCachedTimedMissionSlotData;

	private bool mWinLostStatus;

	private List<AchievementReward> mRewards;

	private int mFreeDragonCost;

	public static FreeStableQuestDragon pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new GameObject
				{
					name = "FreeStableQuestDragon"
				}.AddComponent<FreeStableQuestDragon>();
			}
			return mInstance;
		}
	}

	public TimedMissionSlotData pCachedTimedMissionSlotData => mCachedTimedMissionSlotData;

	public bool pWinLostStatus => mWinLostStatus;

	public List<AchievementReward> pRewards => mRewards;

	public void PurchaseStableQuest(int petID, OnDragonFree callback)
	{
		mCallback = callback;
		TimedMissionSlotData slotFromEngagedPet = TimedMissionManager.pInstance.GetSlotFromEngagedPet(petID);
		if (slotFromEngagedPet != null)
		{
			mCachedTimedMissionSlotData = slotFromEngagedPet;
			string text = "";
			string text2 = "";
			if (slotFromEngagedPet.State == TimedMissionState.Ended || slotFromEngagedPet.State == TimedMissionState.Won || slotFromEngagedPet.State == TimedMissionState.Lost)
			{
				text = TimedMissionManager.pInstance._DragonBusyAfterQuestCompleteText.GetLocalizedString();
				text2 = TimedMissionManager.pInstance._DragonFreeHeaderText.GetLocalizedString();
				mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", text, text2, base.gameObject, "FinishStableMission", "OnDBClose", "", "");
			}
			else
			{
				mFreeDragonCost = TimedMissionManager.pInstance.GetCostForCompletion(slotFromEngagedPet);
				text = TimedMissionManager.pInstance._DragonBusyText.GetLocalizedString().Replace("{{GEMS}}", mFreeDragonCost.ToString());
				text2 = TimedMissionManager.pInstance._DragonBusyHeaderText.GetLocalizedString();
				StartCoroutine(UpdateDragonBusyText(TimedMissionManager.pInstance.GetPetEngageTime(petID).TotalSeconds));
				mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", text, text2, base.gameObject, "CompleteStableMission", "OnDBClose", "", "");
			}
		}
	}

	private IEnumerator UpdateDragonBusyText(double updateAfterSeconds)
	{
		yield return new WaitForSeconds((float)updateAfterSeconds);
		string localizedString = TimedMissionManager.pInstance._DragonBusyAfterQuestCompleteText.GetLocalizedString();
		mGenericDB.SetText(localizedString, interactive: false);
		string localizedString2 = TimedMissionManager.pInstance._DragonFreeHeaderText.GetLocalizedString();
		mGenericDB.SetTitle(localizedString2);
	}

	private void CompleteStableMission()
	{
		StopCoroutine("UpdateDragonBusyText");
		if (Money.pCashCurrency < mFreeDragonCost)
		{
			DestroyDB();
			mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", TimedMissionManager.pInstance._NotEnoughFeeText.GetLocalizedString(), TimedMissionManager.pInstance._InsufficientGemsText.GetLocalizedString(), base.gameObject, "BuyGemsOnline", "OnDBClose", "", "", inDestroyOnClick: true);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			TimedMissionManager.pInstance.ForceComplete(mCachedTimedMissionSlotData, TimeMissionCompleteCallBack);
		}
	}

	private void FinishStableMission()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		TimedMissionManager.pInstance.CompleteMission(mCachedTimedMissionSlotData, TimeMissionCompleteCallBack);
	}

	private void BuyGemsOnline()
	{
		DestroyDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		DestroyDB();
	}

	private void DestroyDB()
	{
		StopCoroutine("UpdateDragonBusyText");
		if (mGenericDB != null)
		{
			Object.Destroy(mGenericDB.gameObject);
			mGenericDB = null;
		}
	}

	private void TimeMissionCompleteCallBack(bool success, bool winStatus, AchievementReward[] reward)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (success)
		{
			mWinLostStatus = winStatus;
			mRewards = new List<AchievementReward>();
			mRewards.AddRange(reward);
			LoadStableQuestCompleteAsset();
		}
		else
		{
			DestroyDB();
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", TimedMissionManager.pInstance._ForceCompleteMissionFailText.GetLocalizedString(), null, "");
		}
	}

	private void LoadStableQuestCompleteAsset()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = GameConfig.GetKeyData("StableQuestCompleteAsset").Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], StableCompleteBundleReady, typeof(GameObject));
	}

	private void StableCompleteBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE && inObject != null)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			DestroyDB();
			Object.Instantiate((GameObject)inObject);
			RsResourceManager.ReleaseBundleData(inURL);
		}
		if (inEvent == RsResourceLoadEvent.ERROR)
		{
			DestroyDB();
			KAUICursorManager.SetDefaultCursor("Arrow");
			TimedMissionManager.pInstance.ResetSlot(mCachedTimedMissionSlotData);
			mGenericDB = GameUtilities.DisplayOKMessage("PfKAUIGenericDB", TimedMissionManager.pInstance._StableQuestBundleFailedText.GetLocalizedString(), base.gameObject, "UpdateAndClose");
		}
	}

	private void OnDBClose()
	{
		DestroyDB();
		mInstance = null;
		Object.Destroy(base.gameObject);
	}

	public void UpdateAndClose()
	{
		if (mCallback != null)
		{
			mCallback();
		}
		DestroyDB();
		Object.Destroy(base.gameObject);
	}
}
