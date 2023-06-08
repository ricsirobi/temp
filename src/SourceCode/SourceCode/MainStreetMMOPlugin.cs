using UnityEngine;

public class MainStreetMMOPlugin
{
	public static GameObject mObject;

	public static void LoadLevel(string level)
	{
		if (mObject != null)
		{
			mObject.SendMessage("LoadLevel", level);
		}
	}

	public static void SetDisplayName(string inName)
	{
		if (mObject != null)
		{
			mObject.SendMessage("SetDisplayName", inName);
		}
	}

	public static void SetGroup(Group inGroup)
	{
		if (mObject != null)
		{
			if (inGroup == null)
			{
				mObject.SendMessage("UnSetGroup");
			}
			else
			{
				mObject.SendMessage("SetGroup", inGroup);
			}
		}
	}

	public static void SetMember(bool isMember)
	{
		if (mObject != null)
		{
			mObject.SendMessage("SetMember", isMember);
		}
	}

	public static void SetJoinAllowed(int canBeJoined)
	{
		if (mObject != null)
		{
			mObject.SendMessage("SetJoinAllowed", canBeJoined);
		}
	}

	public static void UpdateAvatar()
	{
		if (mObject != null)
		{
			mObject.SendMessage("UpdateAvatar");
		}
	}

	public static void JoinBuddy(BuddyLocation location)
	{
		if (mObject != null)
		{
			mObject.SendMessage("JoinBuddy", location);
		}
	}

	public static void BroadcastDismount()
	{
		if (mObject != null)
		{
			mObject.SendMessage("SetRide");
		}
	}
}
