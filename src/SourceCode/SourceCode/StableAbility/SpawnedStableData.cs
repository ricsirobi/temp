using System.Collections.Generic;

namespace StableAbility;

public class SpawnedStableData
{
	public class SpawnData
	{
		public int g;

		public int p;

		public int t;

		public SpawnData(int inG, int inP, int inT)
		{
			g = inG;
			p = inP;
			t = inT;
		}
	}

	public List<SpawnData> SpawnedFishData = new List<SpawnData>();

	public List<SpawnData> SpawnedChestInfoData = new List<SpawnData>();
}
