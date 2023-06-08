public class SEToolboxContextSensitive : LabToolContextSensitive
{
	public ContextSensitiveState[] _Menus;

	private SEToolboxClickable mClickable;

	public static SEToolboxContextSensitive pInstance;

	public SEToolboxClickable pClickable
	{
		get
		{
			if (mClickable == null)
			{
				mClickable = GetComponent<SEToolboxClickable>();
			}
			return mClickable;
		}
	}

	public static UiContextSensitive CSMUI
	{
		get
		{
			if (!(pInstance != null))
			{
				return null;
			}
			return pInstance.mCSMUI;
		}
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		if (!(_MainUI == null) && !LabCrucible.TestItemLoader.IsLoading() && !_MainUI.pLoadingJournal && !(_MainUI.pJournal != null) && _MainUI.GetVisibility() && _MainUI._Manager.pCrucible != null && !LabCrucible.pIsMixing)
		{
			base.UpdateData(ref inStatesArrData);
		}
	}

	protected override void SetContextTargetMessageobject(ContextData inContextData)
	{
		if (_MainUI != null)
		{
			inContextData.pTarget = _MainUI.gameObject;
		}
	}

	protected override void ProcessDestroyMenu()
	{
		if (pClickable != null)
		{
			pClickable.Close();
		}
	}

	protected override void ProcessMenuActive()
	{
		pClickable.SetState(SEToolboxClickable.ToolState.OPENED);
		if (_MainUI != null && _MainUI._Manager != null && _MainUI._Manager._ToolboxClickSFX != null)
		{
			SnChannel.Play(_MainUI._Manager._ToolboxClickSFX, "SFX_Pool", inForce: true, null);
		}
	}

	protected override void Start()
	{
		base.Start();
		pInstance = this;
	}
}
