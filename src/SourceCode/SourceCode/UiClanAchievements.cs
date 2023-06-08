public class UiClanAchievements : UiAchievements
{
	public override string GetAchievementOwnerID()
	{
		return UserProfile.pProfileData.GetGroupID();
	}
}
