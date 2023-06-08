using System.Collections.Generic;

public class RacingComparer : IComparer<PlayerData>
{
	public enum ComparisonMethod
	{
		DISTANCE_COVERED,
		POINTS,
		LAP_TIME,
		RACE_TIME,
		POSITION,
		TOKEN_ID,
		END
	}

	private ComparisonMethod mComparisonMethod;

	public ComparisonMethod pComparisonMethod
	{
		get
		{
			return mComparisonMethod;
		}
		set
		{
			mComparisonMethod = value;
		}
	}

	public int Compare(PlayerData p1, PlayerData p2)
	{
		return p1.CompareTo(p2, mComparisonMethod);
	}
}
