public class UiGameModeScreen : KAUI
{
	private KAWidget mBtnNonMathContent;

	private KAWidget mBtnBack;

	protected override void Start()
	{
		base.Start();
		mBtnBack = FindItem("BtnBack");
		mBtnNonMathContent = FindItem("BtnNonMathContent");
	}

	public override void OnClick(KAWidget item)
	{
		if (item == mBtnNonMathContent)
		{
			GauntletRailShootManager.pInstance.GenerateLevelByGameType();
			SetVisibility(inVisible: false);
		}
		else if (item == mBtnBack)
		{
			SetVisibility(inVisible: false);
			GauntletRailShootManager.pInstance.OnExit();
		}
	}
}
