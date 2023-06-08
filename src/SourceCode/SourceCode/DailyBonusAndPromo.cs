using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "DailyBonusAndPromo", Namespace = "")]
public class DailyBonusAndPromo
{
	[XmlElement(ElementName = "Days")]
	public int MaxDayCount = 5;

	[XmlElement(ElementName = "DailyBonus")]
	public DailyBonus[] BonusData;

	[XmlElement(ElementName = "PromoData")]
	public Promo[] PromoData;

	[XmlElement(ElementName = "PromoPackage")]
	public PromoPackage[] PromoPackages;

	public const string PROMO_PACKAGE_KEYNAME = "PromoPackageOffer";

	public const string PROMO_PACKAGE_PURCHASED = "PURCHASED";

	public const string LAST_PLAYED_KEY = "LP";

	public const string LAST_PLAYED_DAY_COUNT_KEY = "LPC";

	public const int _PairDataID = 1216;

	private PairData mPairData;

	private bool mIsChildDataLoaded;

	private int mDayCount;

	private static bool mIsReady;

	private int mInitCount;

	public static DailyBonusAndPromo pInstance;

	[XmlIgnore]
	public int RewardSequenceDay { get; set; }

	[XmlIgnore]
	public static bool pIsReady => mIsReady;

	public bool CanAward()
	{
		if (mIsReady)
		{
			return mDayCount != 0;
		}
		return false;
	}

	public DealOfTheDayPromo GetBestDeal(Promo promo)
	{
		if (promo == null || promo.Deal == null)
		{
			return null;
		}
		Gender type = AvatarData.GetGender();
		DealOfTheDayPromo[] array = Array.FindAll(promo.Deal, (DealOfTheDayPromo t) => t.GenderType.HasValue && t.GenderType.Value.Equals(type));
		if (array == null || array.Length == 0)
		{
			array = Array.FindAll(promo.Deal, (DealOfTheDayPromo t) => !t.GenderType.HasValue);
		}
		if (UserInfo.pInstance.CreationDate.HasValue)
		{
			Array.Sort(array, delegate(DealOfTheDayPromo a, DealOfTheDayPromo b)
			{
				if (a.DaysOlder.HasValue && b.DaysOlder.HasValue)
				{
					return b.DaysOlder.Value - a.DaysOlder.Value;
				}
				if (a.DaysOlder.HasValue)
				{
					return -1;
				}
				return b.DaysOlder.HasValue ? 1 : 0;
			});
			TimeSpan difference = ServerTime.pCurrentTime - UserInfo.pInstance.CreationDate.Value;
			return Array.Find(array, (DealOfTheDayPromo t) => !t.DaysOlder.HasValue || difference.Days >= t.DaysOlder.Value);
		}
		return array[0];
	}

	public static void Init()
	{
		if (pInstance == null)
		{
			pInstance = new DailyBonusAndPromo();
			PairData.Load(1216, pInstance.OnGamePlayDataReady, null, !UserNotifyDailyBonus.pDoneOnce, ParentData.pInstance.pUserInfo.UserID);
			RsResourceManager.Load(GameConfig.GetKeyData("DailyBonusAndPromoFile"), pInstance.XMLDownloaded);
		}
	}

	private void XMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			_ = 3;
			return;
		}
		using (StringReader textReader = new StringReader((string)inFile))
		{
			DailyBonusAndPromo dailyBonusAndPromo = (DailyBonusAndPromo)new XmlSerializer(typeof(DailyBonusAndPromo)).Deserialize(textReader);
			if (dailyBonusAndPromo != null)
			{
				MaxDayCount = dailyBonusAndPromo.MaxDayCount;
				BonusData = dailyBonusAndPromo.BonusData;
				PromoData = dailyBonusAndPromo.PromoData;
				PromoPackages = dailyBonusAndPromo.PromoPackages;
			}
		}
		mInitCount++;
		if (mInitCount >= 2)
		{
			mIsReady = true;
		}
	}

	public void OnGamePlayDataReady(bool success, PairData inData, object inUserData)
	{
		mInitCount++;
		if (mInitCount >= 2)
		{
			mIsReady = true;
		}
		int num = -1;
		mPairData = inData;
		if (mPairData == null)
		{
			return;
		}
		string value = mPairData.GetValue("LP");
		if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
		{
			DateTime minValue = DateTime.MinValue;
			try
			{
				minValue = DateTime.Parse(value, UtUtilities.GetCultureInfo("en-US"));
			}
			catch (Exception exception)
			{
				minValue = UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime);
				Debug.LogException(exception);
			}
			TimeSpan timeSpan = UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime) - minValue;
			if (mIsChildDataLoaded)
			{
				timeSpan = ServerTime.pCurrentTime - minValue;
			}
			if (timeSpan.Days == 1)
			{
				num = Mathf.Max(0, mPairData.GetIntValue("LPC", 0)) + 1;
				if (num > pInstance.MaxDayCount)
				{
					num = 1;
				}
			}
			else if (Mathf.Abs(timeSpan.Days) > 1)
			{
				num = 1;
			}
		}
		else if (!mIsChildDataLoaded)
		{
			mInitCount--;
			mIsReady = false;
			PairData.Load(1216, pInstance.OnGamePlayDataReady, null, forceLoad: true);
			mIsChildDataLoaded = true;
		}
		else
		{
			num = 1;
		}
		RewardSequenceDay = num;
		if (num > 0)
		{
			mPairData.SetValue("LP", UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).Date.ToString(UtUtilities.GetCultureInfo("en-US")));
			mPairData.SetValue("LPC", num.ToString());
			PairData.Save(1216, ParentData.pInstance.pUserInfo.UserID);
		}
		else
		{
			RewardSequenceDay = Mathf.Max(0, mPairData.GetIntValue("LPC", 0));
		}
		mDayCount = Mathf.Max(0, num);
	}

	public DailyBonus GetBonusInfoForToday()
	{
		return GetBonusInfo(mDayCount);
	}

	public DailyBonus GetBonusInfo(int inDay)
	{
		if (pInstance == null || pInstance.BonusData == null || pInstance.BonusData.Length == 0)
		{
			return null;
		}
		DailyBonus[] bonusData = pInstance.BonusData;
		foreach (DailyBonus dailyBonus in bonusData)
		{
			if (dailyBonus != null && dailyBonus.Day == inDay)
			{
				return dailyBonus;
			}
		}
		return null;
	}

	public Promo GetPromoBySlotName(string inSlotName)
	{
		if (string.IsNullOrEmpty(inSlotName) || PromoData == null || PromoData.Length == 0)
		{
			return null;
		}
		Promo[] promoData = PromoData;
		foreach (Promo promo in promoData)
		{
			if (promo != null && promo.SlotName == inSlotName)
			{
				return promo;
			}
		}
		return null;
	}

	public static void Reset()
	{
		mIsReady = false;
		pInstance = null;
		UserNotifyDailyBonus.pDoneOnce = false;
	}

	public static void ResetShowcasedOffers()
	{
		if (pInstance == null)
		{
			return;
		}
		PromoPackage[] promoPackages = pInstance.PromoPackages;
		if (promoPackages != null)
		{
			PromoPackage[] array = promoPackages;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].pOfferShowcased = false;
			}
		}
	}

	public List<PromoPackage> CheckForPromoPackageOffers(List<PromoPackageTriggerType> types)
	{
		List<PromoPackage> list = new List<PromoPackage>();
		foreach (PromoPackageTriggerType type in types)
		{
			foreach (PromoPackage item in CheckForPromoPackageOffers(type, null))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<PromoPackage> CheckForPromoPackageOffers(PromoPackageTriggerType type, int? param)
	{
		List<PromoPackage> list = new List<PromoPackage>();
		PromoPackage[] promoPackages = PromoPackages;
		if (promoPackages != null)
		{
			for (int i = 0; i < promoPackages.Length; i++)
			{
				bool flag = false;
				PromoPackageTrigger[] triggerData = promoPackages[i].TriggerData;
				for (int j = 0; j < triggerData.Length; j++)
				{
					PromoPackageTriggerType type2 = triggerData[j].Type;
					if (type == type2)
					{
						switch (type2)
						{
						case PromoPackageTriggerType.Session:
							flag = CheckLoginSessions(promoPackages[i], triggerData[j]);
							break;
						case PromoPackageTriggerType.Scene:
							flag = IsPackageOfferValid(promoPackages[i]);
							break;
						case PromoPackageTriggerType.Mission:
						case PromoPackageTriggerType.Task:
							flag = GetAllTasksOrMissions(promoPackages[i], triggerData[j]);
							break;
						case PromoPackageTriggerType.StoreEnter:
							flag = GetPromoForInventoryItems(promoPackages[i], triggerData[j]);
							break;
						case PromoPackageTriggerType.StorePurchase:
							flag = GetPromoForItem(promoPackages[i], triggerData[j], param);
							break;
						}
						if (!flag)
						{
							break;
						}
					}
				}
				if (flag)
				{
					list.Add(promoPackages[i]);
				}
			}
		}
		return list;
	}

	private bool GetPromoForItem(PromoPackage package, PromoPackageTrigger triggerData, int? param)
	{
		if (!param.HasValue)
		{
			return false;
		}
		string value = triggerData.Value;
		if (!string.IsNullOrEmpty(value))
		{
			if (!int.TryParse(value, out var result))
			{
				UtDebug.LogError("@@@@@ Failed to read the itemID from xml @@@@@@ ");
				return false;
			}
			if (param == result && IsPackageOfferValid(package))
			{
				return true;
			}
		}
		return false;
	}

	private bool GetPromoForInventoryItems(PromoPackage package, PromoPackageTrigger triggerData)
	{
		string value = triggerData.Value;
		if (!string.IsNullOrEmpty(value))
		{
			if (!int.TryParse(value, out var result))
			{
				UtDebug.LogError("@@@@@ Failed to read the itemID from xml @@@@@@ ");
				return false;
			}
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(result);
			bool flag = triggerData.Owns.HasValue && triggerData.Owns.Value;
			if (((userItemData != null && flag) || (!flag && userItemData == null)) && IsPackageOfferValid(package))
			{
				return true;
			}
		}
		return false;
	}

	private bool GetAllTasksOrMissions(PromoPackage package, PromoPackageTrigger triggerData)
	{
		string value = triggerData.Value;
		if (!string.IsNullOrEmpty(value))
		{
			if (!int.TryParse(value, out var result))
			{
				UtDebug.LogError("@@@@@ Failed to read the mission or task from xml @@@@@@ ");
				return false;
			}
			if (CheckCompletedTasksOrMissions(result, triggerData.Type) && IsPackageOfferValid(package))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckCompletedTasksOrMissions(int objectiveId, PromoPackageTriggerType type)
	{
		switch (type)
		{
		case PromoPackageTriggerType.Mission:
		{
			Mission mission = MissionManager.pInstance.GetMission(objectiveId);
			if (mission != null && mission.pCompleted)
			{
				return true;
			}
			break;
		}
		case PromoPackageTriggerType.Task:
		{
			Task task = MissionManager.pInstance.GetTask(objectiveId);
			if (task != null && task.pCompleted)
			{
				return true;
			}
			break;
		}
		}
		return false;
	}

	private bool CheckLoginSessions(PromoPackage package, PromoPackageTrigger triggerData)
	{
		int intValue = ProductData.pPairData.GetIntValue("SessionCount", 1);
		int result = 1;
		if (int.TryParse(triggerData.Value, out result))
		{
			if (intValue >= result && IsPackageOfferValid(package))
			{
				return true;
			}
		}
		else
		{
			UtDebug.LogError("Could not parse login session data from xml");
		}
		return false;
	}

	public bool IsPackageOfferValid(PromoPackage package)
	{
		DateTime pCurrentTime = ServerTime.pCurrentTime;
		DateTime result = DateTime.MinValue;
		DateTime minValue = DateTime.MinValue;
		string offerStartTime = package.GetOfferStartTime();
		Gender gender = AvatarData.GetGender();
		if (package.GenderType.HasValue && package.GenderType.Value != 0 && package.GenderType.Value != gender)
		{
			return false;
		}
		if (UserInfo.pInstance.CreationDate.HasValue)
		{
			TimeSpan timeSpan = ServerTime.pCurrentTime - UserInfo.pInstance.CreationDate.Value;
			if (package.DaysOlder.HasValue && package.DaysOlder.Value < timeSpan.Days)
			{
				return false;
			}
		}
		if (package.MemberType.HasValue)
		{
			if (package.MemberType == MembershipType.Member && !SubscriptionInfo.pIsMember)
			{
				return false;
			}
			if (package.MemberType == MembershipType.NonMember && SubscriptionInfo.pIsMember)
			{
				return false;
			}
		}
		if (offerStartTime != null)
		{
			if (!DateTime.TryParse(offerStartTime, out result))
			{
				return false;
			}
			if (package.Duration.HasValue)
			{
				minValue = result.AddHours(package.Duration.Value);
				if (DateTime.Compare(pCurrentTime, minValue) > 0)
				{
					return false;
				}
			}
		}
		return true;
	}
}
