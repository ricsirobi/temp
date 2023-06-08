public class FarmCreativePointTutorial : FarmingTutorialBase
{
	public KAWidget[] _DisableWidget;

	protected override void RestoreUI()
	{
		base.RestoreUI();
		KAWidget[] disableWidget = _DisableWidget;
		foreach (KAWidget kAWidget in disableWidget)
		{
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
		}
	}
}
