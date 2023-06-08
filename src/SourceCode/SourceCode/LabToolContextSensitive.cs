using System.Collections.Generic;

public class LabToolContextSensitive : ObContextSensitive
{
	public List<CrucibleContextMenuData> _MenuDataList;

	public UiContextSensitive mCSMUI;

	public UiScienceExperiment _MainUI;

	public bool AllowDestroy = true;

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		inStatesArrData = GetMenuDataForType(ScientificExperiment.pInstance.pExperimentType)?._Menus;
	}

	public CrucibleContextMenuData GetMenuDataForType(ExperimentType type)
	{
		return _MenuDataList.Find((CrucibleContextMenuData data) => data._Type == type);
	}

	protected override void DestroyMenu(bool checkProximity)
	{
		if (AllowDestroy)
		{
			base.DestroyMenu(checkProximity);
			mCSMUI = null;
			ProcessDestroyMenu();
		}
	}

	protected virtual void ProcessDestroyMenu()
	{
	}

	public void DestroyMe()
	{
		DestroyMenu(checkProximity: false);
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		mCSMUI = base.pUI;
		ProcessMenuActive();
	}

	protected virtual void ProcessMenuActive()
	{
	}
}
