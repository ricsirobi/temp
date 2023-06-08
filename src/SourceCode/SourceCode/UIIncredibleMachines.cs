public class UIIncredibleMachines : KAUI
{
	private KAWidget mPlay;

	private KAWidget mReset;

	private KAWidget mBack;

	private KAWidget mHelp;

	private KAWidget mInventory;

	public UiIncredibleMachinesInventory _UiIncredibleMachinesInventory;

	protected override void Start()
	{
		base.Start();
		mPlay = FindItem("BtnPlay");
		mReset = FindItem("BtnReset");
		mBack = FindItem("BtnBack");
		mHelp = FindItem("BtnHelp");
		mInventory = FindItem("BtnInventory");
		mReset.SetDisabled(isDisabled: true);
	}

	public void DisableButtons(bool Play, bool Reset, bool Inventory, bool Help)
	{
		mPlay.SetDisabled(Play);
		mReset.SetDisabled(Reset);
		mInventory.SetDisabled(Inventory);
		mHelp.SetDisabled(Help);
		if (Inventory)
		{
			_UiIncredibleMachinesInventory.CloseUI();
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mInventory)
		{
			_UiIncredibleMachinesInventory.ToggleVisibility();
		}
		if (item == mPlay)
		{
			IMGLevelManager.pInstance.EnableScene();
		}
		if (item == mReset)
		{
			IMGLevelManager.pInstance.ResetScene();
		}
		if (item == mBack)
		{
			_UiIncredibleMachinesInventory.CloseUI();
			CTLevelManager cTLevelManager = (CTLevelManager)IMGLevelManager.pInstance;
			if (cTLevelManager.pSingleLevelGame)
			{
				cTLevelManager.QuitGame();
			}
			else
			{
				cTLevelManager.ClearScene();
			}
		}
		if (item == mHelp)
		{
			((CTLevelManager)IMGLevelManager.pInstance).ShowHelpScreen();
		}
	}
}
