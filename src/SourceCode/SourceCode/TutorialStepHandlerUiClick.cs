using System.Collections.Generic;

public class TutorialStepHandlerUiClick : TutorialStepHandler
{
	protected List<KAWidget> mClickableWidgets;

	protected List<KAUI> mClickableKAUI;

	public override void SetupTutorialStep()
	{
		base.SetupTutorialStep();
		mClickableKAUI = new List<KAUI>();
		mClickableWidgets = new List<KAWidget>();
		RefreshInterfaceLists();
	}

	private void RefreshInterfaceLists()
	{
		if (mTutStep == null || mTutStep._StepDetails._WaitForClickableUi.Length == 0)
		{
			return;
		}
		ClickableUiProperties[] waitForClickableUi = mTutStep._StepDetails._WaitForClickableUi;
		foreach (ClickableUiProperties clickableUiProperties in waitForClickableUi)
		{
			if (!string.IsNullOrEmpty(clickableUiProperties._WaitForClickableUi) && !string.IsNullOrEmpty(clickableUiProperties._WaitForClickableUiItem))
			{
				KAUI kAUI = TutorialStepHandler.ResolveInterface(clickableUiProperties._WaitForClickableUi);
				if (kAUI != null)
				{
					RefreshKAUIData(kAUI, clickableUiProperties);
				}
			}
		}
	}

	private void RefreshKAUIData(KAUI lClickableKAUI, ClickableUiProperties uiProps)
	{
		if (!(lClickableKAUI != null) || string.IsNullOrEmpty(uiProps._WaitForClickableUiItem))
		{
			return;
		}
		mClickableKAUI.Add(lClickableKAUI);
		lClickableKAUI.pEvents.OnClick += OnClick;
		string[] array = uiProps._WaitForClickableUiItem.Split('/');
		KAWidget kAWidget = null;
		if (array.Length > 1)
		{
			KAWidget kAWidget2 = lClickableKAUI.FindItem(array[0]);
			if (kAWidget2 != null)
			{
				kAWidget = kAWidget2.FindChildItem(array[1]);
			}
		}
		else
		{
			kAWidget = lClickableKAUI.FindItem(uiProps._WaitForClickableUiItem);
		}
		if (lClickableKAUI.GetType().IsSubclassOf(typeof(KAUIMenu)) && kAWidget == null)
		{
			int result = 0;
			if (int.TryParse(uiProps._WaitForClickableUiItem, out result))
			{
				kAWidget = lClickableKAUI.FindItemAt(result);
			}
		}
		mClickableWidgets.Add(kAWidget);
	}

	public override void StepUpdate()
	{
		base.StepUpdate();
	}

	public override void StepLateUpdate()
	{
		base.StepLateUpdate();
		if (false && _StepProgressCallback != null)
		{
			_StepProgressCallback(0f, 0f);
		}
	}

	public void OnClick(KAWidget inClickedItem)
	{
		bool flag = false;
		if (mClickableWidgets != null && mClickableKAUI != null)
		{
			for (int i = 0; i < mClickableKAUI.Count; i++)
			{
				if (mClickableKAUI[i] != null && inClickedItem != null && mClickableWidgets[i] == inClickedItem)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag && _StepProgressCallback != null)
		{
			_StepProgressCallback(0f, 0f);
		}
	}

	private void CleanupKAUI()
	{
		for (int i = 0; i < mClickableKAUI.Count; i++)
		{
			KAUI kAUI = mClickableKAUI[i];
			if (kAUI != null)
			{
				kAUI.pEvents.OnClick -= OnClick;
			}
		}
	}

	public override void FinishTutorialStep()
	{
		base.FinishTutorialStep();
		CleanupKAUI();
	}
}
