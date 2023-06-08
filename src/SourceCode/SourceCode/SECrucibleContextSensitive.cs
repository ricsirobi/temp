public class SECrucibleContextSensitive : LabToolContextSensitive
{
	public static SECrucibleContextSensitive pInstance;

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
		if (!(_MainUI == null) && _MainUI.pObjectInHand == null && _MainUI.pCurrentCursor != UiScienceExperiment.Cursor.SCOOP && _MainUI.pCurrentCursor != UiScienceExperiment.Cursor.SCOOP_WITH_ICE && _MainUI._Manager.pCrucible != null && !_MainUI._Manager.pCrucible.pFreezing && !LabCrucible.TestItemLoader.IsLoading() && !_MainUI.pLoadingJournal && !(_MainUI.pJournal != null) && !_MainUI.pUserPromptOn && _MainUI.GetVisibility() && !LabCrucible.pIsMixing && !(UICursorManager.pCursorManager._DefaultCursorName == "Loading") && !_MainUI._Manager._Gronckle.pIsBellyPopupShown)
		{
			base.UpdateData(ref inStatesArrData);
		}
	}

	public void OnContextAction(string inName)
	{
		if (_MainUI != null)
		{
			_MainUI.OnContextAction(inName);
		}
	}

	protected override void ProcessMenuActive()
	{
		_MainUI.ActivateCursor(UiScienceExperiment.Cursor.NONE);
		if (_MainUI != null && _MainUI._Manager != null && _MainUI._Manager._CrucibleClickSFX != null)
		{
			SnChannel.Play(_MainUI._Manager._CrucibleClickSFX, "SFX_Pool", inForce: true, null);
		}
	}

	protected override void ProcessDestroyMenu()
	{
		if (_MainUI.pCurrentCursor == UiScienceExperiment.Cursor.NONE)
		{
			_MainUI.ActivateCursor(UiScienceExperiment.Cursor.DEFAULT);
		}
	}

	protected override void Start()
	{
		base.Start();
		pInstance = this;
	}
}
