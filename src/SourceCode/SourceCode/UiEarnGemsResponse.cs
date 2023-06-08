public class UiEarnGemsResponse : KAUI
{
	public UiEarnGems _EarnGemsUI;

	private KAWidget mTxtSubHeading;

	protected override void Start()
	{
		base.Start();
		mTxtSubHeading = FindItem("TxtSubHeading");
	}

	public void SetText(string text)
	{
		if (mTxtSubHeading != null)
		{
			mTxtSubHeading.SetText(text);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "CloseBtn")
		{
			SetVisibility(inVisible: false);
			if (_EarnGemsUI != null)
			{
				_EarnGemsUI.SetVisibility(inVisible: true);
			}
		}
	}
}
