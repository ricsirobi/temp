public class AgeUpUserData : KAWidgetUserData
{
	public RaisedPetData pData;

	public bool pFreeAgeUp;

	public AgeUpUserData(RaisedPetData data)
	{
		pData = data;
	}

	public void SetFreeAgeup()
	{
		pFreeAgeUp = AllowFreeAgeUp(pData);
	}

	private bool AllowFreeAgeUp(RaisedPetData data)
	{
		bool result = false;
		RaisedPetStage nextGrowthStage = RaisedPetData.GetNextGrowthStage(data.pStage);
		if (nextGrowthStage != data.pStage)
		{
			int levelFromPetStage = SanctuaryData.pInstance.GetLevelFromPetStage(nextGrowthStage);
			if (levelFromPetStage > 0 && PetRankData.GetUserRank(data).RankID >= levelFromPetStage)
			{
				result = true;
			}
		}
		return result;
	}
}
