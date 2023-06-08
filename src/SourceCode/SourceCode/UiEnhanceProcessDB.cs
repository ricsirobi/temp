using System;
using SquadTactics;
using UnityEngine;

public class UiEnhanceProcessDB : KAUI
{
	public LocaleString _DisplayText;

	public LocaleString _ConfirmationText;

	public LocaleString _FailText;

	public LocaleString _RetryRerollText;

	public LocaleString _RerollSuccessText;

	public LocaleString _ServerCallFailText;

	public UiItemStats _UiItemStats;

	public UiEnhanceSuccessDB _UiStatSuccessDB;

	private KAWidget mBtnYes;

	private KAWidget mBtnNo;

	private KAWidget mBtnConfirm;

	private KAWidget mDisplayMessage;

	private KAWidget mConfirmMessage;

	private UserItemData mUserItemData;

	private EnhanceInfo mEnhanceInfo;

	private GameObject mMessageObject;

	private int mSelectedStatID = -1;

	private bool pRerollItem => mSelectedStatID == -1;

	protected override void Start()
	{
		base.Start();
		mConfirmMessage = FindItem("MessageConfirm");
		mDisplayMessage = FindItem("MessageDisplay");
		mBtnConfirm = FindItem("BtnConfirm");
		mBtnYes = FindItem("BtnYes");
		mBtnNo = FindItem("BtnNo");
		if (mBtnConfirm != null)
		{
			mBtnConfirm.SetDisabled(isDisabled: true);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _BackButtonName || inWidget == mBtnNo)
		{
			SetVisibility(inVisible: false);
			return;
		}
		if (inWidget == mBtnYes)
		{
			Reroll();
			return;
		}
		if (inWidget == mBtnConfirm)
		{
			SetVisibility(inVisible: false);
			ShowConfirmationDB();
			return;
		}
		KAWidgetUserData userData = inWidget.GetUserData();
		if (userData != null && userData._Index > 0)
		{
			mSelectedStatID = userData._Index;
			if (mBtnConfirm != null)
			{
				mBtnConfirm.SetDisabled(isDisabled: false);
			}
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			if (mBtnConfirm != null && mSelectedStatID < 0)
			{
				mBtnConfirm.SetDisabled(isDisabled: true);
			}
			KAUI.SetExclusive(this);
		}
		else
		{
			KAUI.RemoveExclusive(this);
		}
	}

	public void ShowEnhanceDB(UserItemData itemData, EnhanceInfo enhanceInfo, GameObject messageObject)
	{
		mUserItemData = itemData;
		mEnhanceInfo = enhanceInfo;
		mMessageObject = messageObject;
		if (mBtnConfirm != null)
		{
			mBtnConfirm.SetDisabled(isDisabled: true);
		}
		if (mDisplayMessage != null && !string.IsNullOrEmpty(_DisplayText._Text))
		{
			mDisplayMessage.SetText(_DisplayText.GetLocalizedString().Replace("[COUNT]", enhanceInfo.pCountText));
		}
		if (mConfirmMessage != null && !string.IsNullOrEmpty(_ConfirmationText._Text))
		{
			mConfirmMessage.SetText(_ConfirmationText.GetLocalizedString().Replace("[COUNT]", enhanceInfo.pCountText));
		}
		SetVisibility(inVisible: true);
	}

	private string GetStatName(int statId)
	{
		ItemStat[] itemStats = mUserItemData.ItemStats;
		foreach (ItemStat itemStat in itemStats)
		{
			if (itemStat.ItemStatID == statId)
			{
				return itemStat.Name;
			}
		}
		return "";
	}

	private string GetLocalizedStatName(int statId)
	{
		if (Settings.pInstance != null)
		{
			return Settings.pInstance.GetStatEffectName(statId);
		}
		return "";
	}

	private int GetStatChance(int statId)
	{
		foreach (Stat stat in mUserItemData.Item.PossibleStatsMap.Stats)
		{
			if (stat.ItemStatsID != statId)
			{
				continue;
			}
			ItemStat[] itemStats = mUserItemData.ItemStats;
			foreach (ItemStat itemStat in itemStats)
			{
				if (itemStat.ItemStatID == statId && !string.IsNullOrEmpty(itemStat.Value))
				{
					int num = int.Parse(itemStat.Value);
					StatRangeMap statRangeMap = GetStatRangeMap(stat, (int)mUserItemData.ItemTier.Value);
					if (statRangeMap != null)
					{
						return (int)ComputeHigherValueProbability(statRangeMap.StartRange, statRangeMap.EndRange, num);
					}
				}
			}
			return -1;
		}
		return -1;
	}

	private StatRangeMap GetStatRangeMap(Stat possibleStat, int tier)
	{
		return possibleStat.ItemStatsRangeMaps.Find((StatRangeMap m) => m.ItemTierID == tier);
	}

	private float ComputeHigherValueProbability(float startRange, float endRange, float value)
	{
		if (startRange > endRange)
		{
			return -1f;
		}
		if (startRange == endRange || value >= endRange)
		{
			return 0f;
		}
		if (value < startRange)
		{
			return 100f;
		}
		if (endRange - startRange == 1f)
		{
			return 50f;
		}
		float num = ComputeMean(startRange, endRange);
		float num2 = ComputeSigma(startRange, endRange);
		float z = (value - num) / num2;
		float num3 = ComputeArea(z);
		num3 = 1f - num3;
		num3 *= 100f;
		return Mathf.Round(num3);
	}

	private float ComputeMean(float startRange, float endRange)
	{
		return (startRange + endRange) / 2f;
	}

	private float ComputeSigma(float startRange, float endRange)
	{
		return (endRange - startRange + 1f) / 6f;
	}

	private float ComputeArea(float z)
	{
		float num = 0.3275911f;
		float num2 = -244f / (273f * MathF.PI);
		float num3 = 1.4214138f;
		float num4 = -1.4531521f;
		float num5 = 1.0614054f;
		float num6 = Mathf.Abs(z) / Mathf.Sqrt(2f);
		float num7 = 1f / (1f + num * num6);
		float num8 = 0.2548296f * num7;
		float num9 = num2 * Mathf.Pow(num7, 2f);
		float num10 = num3 * Mathf.Pow(num7, 3f);
		float num11 = num4 * Mathf.Pow(num7, 4f);
		float num12 = num5 * Mathf.Pow(num7, 5f);
		float num13 = 1f - (num8 + num9 + num10 + num11 + num12) * Mathf.Exp((0f - num6) * num6);
		return Mathf.Round(0.5f * (1f + Mathf.Sign(z) * num13) * 100000f) / 100000f;
	}

	private void Reroll()
	{
		if (!mEnhanceInfo.pEnhance && mMessageObject != null)
		{
			SetVisibility(inVisible: false);
			mMessageObject.SendMessage(pRerollItem ? "BuyItemEnhance" : "BuyStatEnhance");
			return;
		}
		RollUserItemRequest rollUserItemRequest = new RollUserItemRequest();
		rollUserItemRequest.UserInventoryID = mUserItemData.UserInventoryID;
		rollUserItemRequest.ContainerID = 1;
		CommonInventoryRequest commonInventoryRequest = new CommonInventoryRequest();
		commonInventoryRequest.CommonInventoryID = mEnhanceInfo.pUserItemData.UserInventoryID;
		commonInventoryRequest.ItemID = mEnhanceInfo.pUserItemData.Item.ItemID;
		commonInventoryRequest.Quantity = -mEnhanceInfo.pConsumeCount;
		commonInventoryRequest.Uses = mEnhanceInfo.pUserItemData.Uses;
		commonInventoryRequest.InventoryMax = mEnhanceInfo.pUserItemData.Item.InventoryMax;
		rollUserItemRequest.InventoryItems = new CommonInventoryRequest[1] { commonInventoryRequest };
		string statName = GetStatName(mSelectedStatID);
		rollUserItemRequest.ItemStatNames = (string.IsNullOrEmpty(statName) ? null : new string[1] { statName });
		KAUICursorManager.SetExclusiveLoadingGear(status: true);
		WsWebService.RerollUserItem(rollUserItemRequest, ServiceEventHandler, null);
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.REROLL_USER_ITEM)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			RollUserItemResponse rollUserItemResponse = (RollUserItemResponse)inObject;
			if (rollUserItemResponse.Status == Status.Success)
			{
				CommonInventoryData.pInstance.RemoveItem(mEnhanceInfo.pUserItemData, mEnhanceInfo.pConsumeCount);
				if (pRerollItem)
				{
					mUserItemData.ItemStats = rollUserItemResponse.ItemStats;
					if (mMessageObject != null)
					{
						mMessageObject.SendMessage("OnItemRerollDone");
					}
				}
				else
				{
					ItemStat[] itemStats = rollUserItemResponse.ItemStats;
					foreach (ItemStat newStat in itemStats)
					{
						ItemStat itemStat = Array.Find(mUserItemData.ItemStats, (ItemStat i) => i.ItemStatID == newStat.ItemStatID);
						if (itemStat != null)
						{
							string displayText = _RerollSuccessText.GetLocalizedString().Replace("[STAT]", GetLocalizedStatName(newStat.ItemStatID));
							StStatInfo statInfoByID = Settings.pInstance.GetStatInfoByID(itemStat.ItemStatID);
							int result = 0;
							int num = 1;
							if (statInfoByID._Stat == SquadTactics.Stat.HEALTH)
							{
								num = (int)statInfoByID._Value;
							}
							int.TryParse(itemStat.Value, out result);
							string oldStat = (result * num).ToString();
							int.TryParse(newStat.Value, out result);
							string newStat2 = (result * num).ToString();
							if (_UiStatSuccessDB != null)
							{
								_UiStatSuccessDB.DisplayStat(displayText, oldStat, newStat2);
							}
							itemStat.Value = newStat.Value;
						}
					}
					if (mMessageObject != null)
					{
						mMessageObject.SendMessage("OnStatRerollDone");
					}
				}
				string partName = AvatarData.GetPartName(mUserItemData.Item);
				if (MissionManager.pInstance != null)
				{
					MissionManager.pInstance.CheckForTaskCompletion("Action", "EnhanceItem", partName);
				}
			}
			else if (rollUserItemResponse.Status == Status.Failure)
			{
				if (!pRerollItem)
				{
					CommonInventoryData.pInstance.RemoveItem(mEnhanceInfo.pUserItemData, mEnhanceInfo.pConsumeCount);
					if (mMessageObject != null)
					{
						mMessageObject.SendMessage("OnStatRerollDone");
					}
					ShowFailDB();
				}
				else
				{
					ShowServerFailDB();
				}
			}
			else
			{
				ShowServerFailDB();
			}
			SetVisibility(inVisible: false);
			KAUICursorManager.SetExclusiveLoadingGear(status: false);
			break;
		}
		case WsServiceEvent.ERROR:
			ShowServerFailDB();
			KAUICursorManager.SetExclusiveLoadingGear(status: false);
			break;
		}
	}

	private void ShowConfirmationDB()
	{
		string text = _ConfirmationText.GetLocalizedString().Replace("[COUNT]", mEnhanceInfo.pCountText);
		text = text.Replace("[CHANCE]", GetStatChance(mSelectedStatID).ToString());
		text = text.Replace("[STAT]", GetLocalizedStatName(mSelectedStatID));
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", text, "", base.gameObject, "Reroll", "OnDBClose", "", "", inDestroyOnClick: true);
	}

	private void OnDBClose()
	{
		SetVisibility(inVisible: true);
	}

	private void ShowFailDB()
	{
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _RetryRerollText.GetLocalizedString(), "", base.gameObject, "ShowConfirmationDB", "OnDBClose", "", "", inDestroyOnClick: true);
	}

	private void ShowServerFailDB()
	{
		GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ServerCallFailText.GetLocalizedString(), base.gameObject, "OnDBClose");
	}
}
