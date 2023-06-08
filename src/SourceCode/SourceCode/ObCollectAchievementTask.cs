using System.Collections.Generic;

public class ObCollectAchievementTask : ObCollect
{
	public int _AchievementTaskID = -1;

	protected override void Collected()
	{
		base.Collected();
		if (_AchievementTaskID != -1)
		{
			UserAchievementTask.Set(new List<AchievementTask>
			{
				new AchievementTask(_AchievementTaskID)
			}.ToArray());
		}
	}
}
