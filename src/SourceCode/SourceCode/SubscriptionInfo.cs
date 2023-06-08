using System;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot(Namespace = "http://api.jumpstart.com/", IsNullable = true)]
public class SubscriptionInfo
{
	[XmlElement(ElementName = "BillFrequency", IsNullable = true)]
	public short? BillFrequency;

	[XmlElement(ElementName = "CardExpirationDate", IsNullable = true)]
	public DateTime? CardExpirationDate;

	[XmlElement(ElementName = "CardReferenceNumber")]
	public string CardReferenceNumber;

	[XmlElement(ElementName = "IsActive", IsNullable = true)]
	public bool? IsActive;

	[XmlElement(ElementName = "LastBillDate", IsNullable = true)]
	public DateTime? LastBillDate;

	[XmlElement(ElementName = "MembershipID", IsNullable = true)]
	public int? MembershipID;

	[XmlElement(ElementName = "ProfileCurrency")]
	public string ProfileCurrency;

	[XmlElement(ElementName = "ProfileID")]
	public string ProfileID;

	[XmlElement(ElementName = "Recurring", IsNullable = true)]
	public bool? Recurring;

	[XmlElement(ElementName = "RecurringAmount", IsNullable = true)]
	public float? RecurringAmount;

	[XmlElement(ElementName = "Status")]
	public string Status;

	[XmlElement(ElementName = "SubscriptionDisplayName")]
	public string SubscriptionDisplayName;

	[XmlElement(ElementName = "SubscriptionEndDate", IsNullable = true)]
	public DateTime? SubscriptionEndDate;

	[XmlElement(ElementName = "SubscriptionID", IsNullable = true)]
	public int? SubscriptionID;

	[XmlElement(ElementName = "SubscriptionPlanID", IsNullable = true)]
	public int? SubscriptionPlanID;

	[XmlElement(ElementName = "SubscriptionProvider")]
	public UserSubscriptionProvider SubscriptionProvider;

	[XmlElement(ElementName = "SubscriptionTypeID", IsNullable = true)]
	public int? SubscriptionTypeID;

	[XmlElement(ElementName = "UserID", IsNullable = true)]
	public string UserID;

	[XmlElement(ElementName = "UserSubscriptionNotification", IsNullable = true)]
	public SubscriptionNotification UserSubscriptionNotification;

	private static SubscriptionInfo mInstance;

	public static bool _MembershipPurchased;

	public static SubscriptionInfo pInstance => mInstance;

	public static bool pIsReady => mInstance != null;

	public static bool pIsMember
	{
		get
		{
			if (mInstance == null || mInstance.SubscriptionTypeID != 1)
			{
				return false;
			}
			return true;
		}
	}

	public static bool pIsTrialMember
	{
		get
		{
			if (pIsMember && !string.IsNullOrEmpty(mInstance.Status) && mInstance.Status.Equals("Trial"))
			{
				return true;
			}
			return false;
		}
	}

	public static bool pIsMembershipExpired
	{
		get
		{
			if (mInstance == null || !ServerTime.pIsReady || !mInstance.SubscriptionEndDate.HasValue)
			{
				return false;
			}
			return DateTime.Compare(mInstance.SubscriptionEndDate.Value, ServerTime.pCurrentTime) <= 0;
		}
	}

	public static void ExtendMembership(string parentUserID, int numDays)
	{
		WsWebService.ExtendMembership(parentUserID, memberOnly: false, numDays, ServiceEventHandler, null);
	}

	public static void Init()
	{
		if (pInstance == null)
		{
			WsWebService.GetSubscriptionInfo(ServiceEventHandler, null);
		}
	}

	public static void Init(SubscriptionInfo info)
	{
		mInstance = info;
		MainStreetMMOPlugin.SetMember(mInstance.SubscriptionTypeID == 1);
	}

	public static void Reset()
	{
		mInstance = null;
	}

	public static void SetType(int type)
	{
		if (mInstance != null)
		{
			mInstance.SubscriptionTypeID = type;
		}
		AvatarData.AddMemberToDisplayName(AvAvatar.pObject, AvatarData.pDisplayYourName, pIsMember);
	}

	public static short GetBillFrequency()
	{
		if (mInstance == null || !mInstance.BillFrequency.HasValue)
		{
			return -1;
		}
		return mInstance.BillFrequency.Value;
	}

	public static bool IsOneMonthMembership()
	{
		return GetBillFrequency() == 1;
	}

	public static int GetProviderProductID()
	{
		if (mInstance == null || mInstance.SubscriptionProvider == null)
		{
			return -1;
		}
		return mInstance.SubscriptionProvider.ProductID;
	}

	public static SubscriptionNotificationType GetSubscriptionNotificationType()
	{
		if (mInstance == null || mInstance.UserSubscriptionNotification == null)
		{
			return SubscriptionNotificationType.NONE;
		}
		return mInstance.UserSubscriptionNotification.Type;
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.EXTEND_MEMBERSHIP:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					if ((bool)inObject)
					{
						Debug.LogError("membership extend successfully");
					}
					else
					{
						Debug.LogError("membership extend failed");
					}
				}
				else
				{
					Debug.LogError("membership extend return null");
				}
				break;
			case WsServiceEvent.ERROR:
				Debug.LogError("WEB SERVICE CALL ExtendMembership FAILED!!!");
				break;
			}
			break;
		case WsServiceType.GET_SUBSCRIPTION_INFO:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					mInstance = (SubscriptionInfo)inObject;
					if (mInstance.SubscriptionTypeID == 1)
					{
						UtDebug.Log(">>>>>>>>>>> User is a member!");
					}
					else if (mInstance.SubscriptionTypeID == 2)
					{
						UtDebug.Log(">>>>>>>>>>> User is NOT a member!");
					}
					else
					{
						UtDebug.Log(">>>>>>>>>>> User type undefined!!");
					}
				}
				else
				{
					UtDebug.Log("WEB SERVICE CALL GetSubscriptionInfo RETURNED NO DATA!!!");
					if (mInstance == null || mInstance.SubscriptionTypeID != 1)
					{
						mInstance = new SubscriptionInfo();
						mInstance.SubscriptionTypeID = 2;
					}
				}
				AvatarData.AddMemberToDisplayName(AvAvatar.pObject, AvatarData.pDisplayYourName, pIsMember);
				MainStreetMMOPlugin.SetMember(mInstance.SubscriptionTypeID == 1);
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetSubscriptionInfo FAILED!!!");
				if (mInstance == null || mInstance.SubscriptionTypeID != 1)
				{
					mInstance = new SubscriptionInfo();
					mInstance.SubscriptionTypeID = 2;
					MainStreetMMOPlugin.SetMember(isMember: false);
				}
				break;
			}
			break;
		}
	}
}
