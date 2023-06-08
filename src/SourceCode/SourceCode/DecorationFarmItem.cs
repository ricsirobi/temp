public class DecorationFarmItem : FarmItem
{
	protected override bool CanProcessUpdateData()
	{
		return CanActivate();
	}
}
