using System.Collections.Generic;

internal class IncubatorListSort : IComparer<Incubator>
{
	public int Compare(Incubator inFirst, Incubator inSecond)
	{
		if (inFirst.pID > inSecond.pID)
		{
			return 1;
		}
		if (inSecond.pID > inFirst.pID)
		{
			return -1;
		}
		return 0;
	}
}
