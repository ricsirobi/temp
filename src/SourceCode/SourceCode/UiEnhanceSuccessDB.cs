public class UiEnhanceSuccessDB : KAUI
{
	public UiEnhance _UiEnhance;

	private KAWidget mMessageDisplay;

	private KAWidget mStatValue;

	private KAWidget mBtnOk;

	private string mNewStat;

	protected override void Start()
	{
		base.Start();
		mMessageDisplay = FindItem("TxtDialog");
		mStatValue = FindItem("Stat");
	}

	public void DisplayStat(string displayText, string oldStat, string newStat)
	{
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
		mNewStat = newStat;
		if (mStatValue != null)
		{
			mStatValue.SetText(oldStat);
		}
		if (mMessageDisplay != null)
		{
			mMessageDisplay.SetText(displayText);
		}
		if (_UiEnhance != null && mStatValue != null)
		{
			_UiEnhance.ShowEffects("Stat", delegate
			{
				mStatValue.SetText(mNewStat);
			});
		}
	}

	public override void OnClick(KAWidget widget)
	{
		base.OnClick(widget);
		if (widget.name == _BackButtonName)
		{
			if (_UiEnhance != null)
			{
				_UiEnhance.StopEffects();
			}
			KAUI.RemoveExclusive(this);
			SetVisibility(inVisible: false);
		}
	}
}
