public class ObProximityFarm : ObProximity
{
	public override void LoadLevel(string level)
	{
		FarmManager.pCurrentFarmData = null;
		base.LoadLevel(level);
	}
}
