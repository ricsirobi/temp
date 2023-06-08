using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "GP", IsNullable = true, Namespace = "")]
public class Group
{
	public class GetCallbackData
	{
		public OnGroupGetEventHandler Callback;

		public object UserData;

		public GetCallbackData(OnGroupGetEventHandler callback, object userData)
		{
			Callback = callback;
			UserData = userData;
		}
	}

	[XmlElement(ElementName = "G")]
	public string GroupID;

	[XmlElement(ElementName = "N")]
	public string Name;

	[XmlElement(ElementName = "D")]
	public string Description;

	[XmlElement(ElementName = "T")]
	public GroupType Type;

	[XmlElement(ElementName = "O", IsNullable = true)]
	public string OwnerID;

	[XmlElement(ElementName = "L", IsNullable = true)]
	public string Logo;

	[XmlElement(ElementName = "C")]
	public string Color;

	[XmlElement(ElementName = "M", IsNullable = true)]
	public int? MemberLimit;

	[XmlElement(ElementName = "TC", IsNullable = true)]
	public int? TotalMemberCount;

	[XmlElement(ElementName = "A")]
	public bool Active;

	[XmlElement(ElementName = "P", IsNullable = true)]
	public string ParentGroupID;

	[XmlElement(ElementName = "PS", IsNullable = true)]
	public int? Points;

	[XmlElement(ElementName = "RK", IsNullable = true)]
	public int? Rank;

	[XmlElement(ElementName = "RT", IsNullable = true)]
	public int? RankTrend;

	private static bool mInitialized = false;

	private static List<Group> mGroups = null;

	private static List<RolePermission> mRolePermissions = null;

	private static List<Group> mSearchGroups = null;

	private static List<Group> mTopGroups = null;

	private static bool mIsSearching = false;

	private static bool mSearchCaseSensitive;

	private static string mSearchKey = "";

	private static string mLastSearchKey = "";

	private static int mLastSearchCount = 0;

	public static OnGroupSearchEventHandler pOnGroupSearchHandler = null;

	public static OnGroupTopListEventHandler pOnGroupTopListHandler = null;

	private static bool mIsReady = false;

	private static bool mIsAllGroupsReady = false;

	public static List<Group> pGroups => mGroups;

	public static List<Group> pSearchGroups => mSearchGroups;

	public static List<Group> pTopGroups => mTopGroups;

	public static bool pIsSearching => mIsSearching;

	public static string pLastSearchKey => mLastSearchKey;

	public static int pLastSearchCount => mLastSearchCount;

	public static bool pIsReady => mIsReady;

	public static bool pIsAllGroupsReady => mIsAllGroupsReady;

	public static void Init(string forUserID = null)
	{
		Init(includeMemberCount: false, forUserID);
	}

	public static void Init(bool includeMemberCount, string forUserID = null, bool skipServerCall = false)
	{
		if (!mInitialized)
		{
			mInitialized = true;
			if (!skipServerCall)
			{
				GetTopList(1, 100, forUserID, includeMemberCount, null);
			}
			else
			{
				mIsReady = true;
			}
		}
	}

	public static void Reset()
	{
		mInitialized = false;
		mIsReady = false;
		mIsAllGroupsReady = false;
		mLastSearchKey = null;
		mSearchGroups = null;
		mTopGroups = null;
	}

	public static void Get(string inUserID, OnGroupGetEventHandler eventHandler, object inUserData = null)
	{
		WsWebService.GetGroups(new GetGroupsRequest
		{
			ForUserID = inUserID
		}, GetEventHandler, new GetCallbackData(eventHandler, inUserData));
	}

	public static void GetTopList(int pageNo, int pageSize, string forUserID, bool includeMemberCount, OnGroupTopListEventHandler eventHandler)
	{
		pOnGroupTopListHandler = eventHandler;
		WsWebService.GetGroups(new GetGroupsRequest
		{
			IncludeMemberCount = includeMemberCount,
			PageSize = pageSize,
			PageNo = pageNo,
			ForUserID = forUserID
		}, GetTopListEventHandler, forUserID == null);
	}

	public static void Search(string key, OnGroupSearchEventHandler eventHandler, int count = 100, bool caseSensitive = false)
	{
		pOnGroupSearchHandler = eventHandler;
		if (!string.IsNullOrEmpty(mLastSearchKey) && key.Contains(mLastSearchKey) && (mSearchGroups == null || mLastSearchCount <= mSearchGroups.Count))
		{
			pOnGroupSearchHandler(FilterClans(mSearchGroups, key, caseSensitive), key);
			return;
		}
		mIsSearching = true;
		GetGroupsRequest getGroupsRequest = new GetGroupsRequest
		{
			IncludeMemberCount = true,
			Name = key
		};
		mSearchKey = key;
		mSearchCaseSensitive = caseSensitive;
		WsWebService.GetGroups(getGroupsRequest, GetSearchEventHandler, false);
	}

	public static List<Group> FilterClans(List<Group> groups, string key, bool caseSensitive)
	{
		List<Group> list = new List<Group>();
		if (groups != null)
		{
			foreach (Group group in groups)
			{
				if (!caseSensitive)
				{
					if (group.Name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						list.Add(group);
					}
				}
				else if (group.Name.Contains(key))
				{
					list.Add(group);
				}
			}
		}
		return list;
	}

	private static void GetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE && inEvent != WsServiceEvent.ERROR)
		{
			return;
		}
		GetGroupsResult getGroupsResult = (GetGroupsResult)inObject;
		if (getGroupsResult != null && getGroupsResult.Groups != null)
		{
			Group[] groups = getGroupsResult.Groups;
			for (int i = 0; i < groups.Length; i++)
			{
				AddGroup(groups[i]);
			}
		}
		GetCallbackData getCallbackData = (GetCallbackData)inUserData;
		if (getCallbackData != null && getCallbackData.Callback != null)
		{
			getCallbackData.Callback(getGroupsResult, getCallbackData.UserData);
		}
		mIsReady = true;
	}

	private static void GetTopListEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR)
		{
			GetGroupsResult getGroupsResult = (GetGroupsResult)inObject;
			if (getGroupsResult != null && getGroupsResult.Groups != null)
			{
				mTopGroups = new List<Group>(getGroupsResult.Groups);
				mRolePermissions = new List<RolePermission>(getGroupsResult.RolePermissions);
			}
			mIsReady = true;
			mIsAllGroupsReady = (bool)inUserData;
			if (pOnGroupTopListHandler != null)
			{
				pOnGroupTopListHandler(mTopGroups);
			}
		}
	}

	private static void GetSearchEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE && inEvent != WsServiceEvent.ERROR)
		{
			return;
		}
		mLastSearchKey = mSearchKey;
		mIsSearching = false;
		GetGroupsResult getGroupsResult = (GetGroupsResult)inObject;
		if (getGroupsResult != null && getGroupsResult.Groups != null)
		{
			mSearchGroups = new List<Group>(getGroupsResult.Groups);
		}
		else
		{
			mSearchGroups = null;
		}
		if (pOnGroupSearchHandler != null)
		{
			if (mSearchCaseSensitive)
			{
				pOnGroupSearchHandler(FilterClans(mSearchGroups, mSearchKey, caseSensitive: true), mSearchKey);
			}
			else
			{
				pOnGroupSearchHandler(mSearchGroups, mSearchKey);
			}
		}
	}

	public static void AddGroup(Group inGroup)
	{
		if (mGroups == null)
		{
			mGroups = new List<Group>();
		}
		else
		{
			Group group = mGroups.Find((Group g) => g.GroupID == inGroup.GroupID);
			if (group != null)
			{
				mGroups.Remove(group);
			}
		}
		if (mSearchGroups == null)
		{
			mSearchGroups = new List<Group>();
		}
		else
		{
			Group group2 = mSearchGroups.Find((Group g) => g.GroupID == inGroup.GroupID);
			if (group2 != null)
			{
				mSearchGroups.Remove(group2);
			}
		}
		mGroups.Add(inGroup);
		mSearchGroups.Add(inGroup);
	}

	public static Group GetGroup(string inGroupID)
	{
		Group group = null;
		if (mGroups != null)
		{
			group = mGroups.Find((Group g) => g.GroupID == inGroupID);
		}
		if (group == null && mSearchGroups != null)
		{
			group = mSearchGroups.Find((Group g) => g.GroupID == inGroupID);
		}
		if (group == null && mTopGroups != null)
		{
			group = mTopGroups.Find((Group g) => g.GroupID == inGroupID);
		}
		return group;
	}

	public static void UpdateMemberCount(Group inGroup, int count)
	{
		int finalCount = 0;
		if (inGroup != null && inGroup.TotalMemberCount.HasValue)
		{
			finalCount = inGroup.TotalMemberCount.Value;
		}
		UpdateMemberCount(mGroups, inGroup, finalCount);
		UpdateMemberCount(mSearchGroups, inGroup, finalCount);
		UpdateMemberCount(mTopGroups, inGroup, finalCount);
	}

	private static void UpdateMemberCount(List<Group> groups, Group inGroup, int finalCount)
	{
		if (groups != null)
		{
			Group group = groups.Find((Group g) => g.GroupID == inGroup.GroupID);
			if (group != null && group.TotalMemberCount.HasValue)
			{
				group.TotalMemberCount = finalCount;
			}
		}
	}

	public static void SetNewOwner(Group inGroup, string inLeaderID)
	{
		SetNewOwner(mGroups, inGroup, inLeaderID);
		SetNewOwner(mSearchGroups, inGroup, inLeaderID);
		SetNewOwner(mTopGroups, inGroup, inLeaderID);
	}

	private static void SetNewOwner(List<Group> groups, Group inGroup, string inLeaderID)
	{
		if (groups != null)
		{
			Group group = groups.Find((Group g) => g.GroupID == inGroup.GroupID);
			if (group != null)
			{
				group.OwnerID = inLeaderID;
			}
		}
	}

	public bool HasPermission(UserRole inRole, string permission)
	{
		if (mRolePermissions == null)
		{
			return false;
		}
		foreach (RolePermission mRolePermission in mRolePermissions)
		{
			if (mRolePermission.GroupType == Type && mRolePermission.Role == inRole && mRolePermission.Permissions.Contains(permission))
			{
				return true;
			}
		}
		return false;
	}

	public bool GetFGColor(out Color color)
	{
		color = UnityEngine.Color.white;
		string[] array = Color.Split(',');
		if (array.Length >= 2)
		{
			return HexUtil.HexToColor(array[0], out color);
		}
		return false;
	}

	public bool GetBGColor(out Color color)
	{
		color = UnityEngine.Color.white;
		string[] array = Color.Split(',');
		if (array.Length >= 2)
		{
			return HexUtil.HexToColor(array[1], out color);
		}
		return false;
	}
}
