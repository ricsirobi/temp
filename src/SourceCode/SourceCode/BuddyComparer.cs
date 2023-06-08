using System.Collections.Generic;

public class BuddyComparer : IComparer<Buddy>
{
	public int Compare(Buddy a, Buddy b)
	{
		if (a.BestBuddy && !b.BestBuddy)
		{
			return -1;
		}
		if (b.BestBuddy && !a.BestBuddy)
		{
			return 1;
		}
		return string.Compare(a.DisplayName, b.DisplayName);
	}
}
