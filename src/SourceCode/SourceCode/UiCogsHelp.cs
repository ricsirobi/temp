public class UiCogsHelp : KAUI
{
	private KAWidget mBtnClose;

	protected override void Start()
	{
		base.Start();
		mBtnClose = FindItem("BtnClose");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnClose)
		{
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
			KAUI.RemoveExclusive(this);
			SetVisibility(inVisible: false);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible && MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
	}
}
