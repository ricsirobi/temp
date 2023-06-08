public class HatchSlotWidget : KAButton
{
	private KAWidget mTimerText;

	private Incubator mIncubator;

	private void Start()
	{
		mTimerText = FindChildItem("TxtTimer");
	}

	public void UpdateIncubator(IncubatorWidgetData data)
	{
		if (data != null)
		{
			mIncubator = data.Incubator;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mIncubator != null && mIncubator.pMyState == Incubator.IncubatorStates.HATCHING && mTimerText.GetVisibility() && mTimerText != null)
		{
			mTimerText.SetText(mIncubator.GetStatusText(mIncubator.GetHatchTimeLeft()));
		}
	}
}
