using System;
using System.Collections.Generic;

public class BuddyStatusComparer : IComparer<BuddyStatusMessage>
{
	public int Compare(BuddyStatusMessage a, BuddyStatusMessage b)
	{
		return -DateTime.Compare(a.StatusMessage.CreateTime, b.StatusMessage.CreateTime);
	}
}
