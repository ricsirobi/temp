public class CmRK
{
	public uint pContentItemID;

	public string pSkillAddress;

	public uint pSubLevelID;

	public uint pLevel;

	public CmRK(uint inCI, string inSA, uint inSL, uint inL)
	{
		pContentItemID = inCI;
		pSkillAddress = inSA;
		pSubLevelID = inSL;
		pLevel = inL;
	}

	public override bool Equals(object obj)
	{
		CmRK cmRK = (CmRK)obj;
		if (cmRK == null)
		{
			return false;
		}
		if (pSkillAddress == cmRK.pSkillAddress && pSubLevelID == cmRK.pSubLevelID && pLevel == cmRK.pLevel)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
