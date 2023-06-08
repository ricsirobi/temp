using System;

public class PlayerData : IComparable
{
	public MMOAvatar mAvatar;

	public AvatarRacing mAvatarRacing;

	public string mUserName;

	public string mName;

	public RaceState mResultState;

	public string mUserId;

	public bool mIsReadyDisplay;

	public int mTrophyCount;

	public bool mReadyToRace;

	public PlayerData(MMOAvatar avatar, string name, AvatarRacing avatarRacing, RaceState state)
	{
		mName = name;
		mAvatar = avatar;
		mAvatarRacing = avatarRacing;
		mResultState = state;
		if (avatar != null)
		{
			mUserId = mAvatar.pUserID;
			if (avatar.pProfileData == null)
			{
				avatar.UpdateProfileData();
			}
		}
		else
		{
			mUserId = UserInfo.pInstance.UserID;
		}
	}

	public int CompareTo(object obj)
	{
		if (obj is PlayerData)
		{
			PlayerData playerData = (PlayerData)obj;
			return mAvatarRacing.pDistanceCovered.CompareTo(playerData.mAvatarRacing.pDistanceCovered);
		}
		return 0;
	}

	public int CompareTo(PlayerData racing, RacingComparer.ComparisonMethod comparisonType)
	{
		return comparisonType switch
		{
			RacingComparer.ComparisonMethod.DISTANCE_COVERED => mAvatarRacing.pDistanceCovered.CompareTo(racing.mAvatarRacing.pDistanceCovered) * -1, 
			RacingComparer.ComparisonMethod.TOKEN_ID => mName.CompareTo(racing.mName), 
			_ => 0, 
		};
	}
}
