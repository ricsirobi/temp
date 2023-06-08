using UnityEngine;

public class UiStableQuestMain : KAUI
{
	public delegate void OnStablesUIClosed();

	public static UiStableQuestMain pInstance;

	public UiStableQuestSlots _StableQuestSlotsUI;

	public UiStableQuestDetail _StableQuestDetailsUI;

	public UiStableQuestResult _StableQuestResultsUI;

	public static event OnStablesUIClosed OnStableQuestUIHandler;

	public void DestroyUI()
	{
		TimedMissionManager.pInstance.pCanRegisterNotification = true;
		UiStableQuestMain.OnStableQuestUIHandler?.Invoke();
		Object.Destroy(base.gameObject);
		if (SanctuaryManager.IsPetLocked(SanctuaryManager.pCurPetInstance.pData))
		{
			UiDragonsStable.OpenDragonListUI(base.gameObject, UiDragonsStable.Mode.DragonSelection, setExclusive: true, forceSelection: true);
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
		else
		{
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (pInstance != null)
		{
			pInstance.DestroyUI();
		}
		pInstance = this;
		TimedMissionManager.pInstance.pCanRegisterNotification = false;
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
	}

	public string GetTimerString(int timeInSeconds)
	{
		int num = timeInSeconds / 3600;
		int num2 = timeInSeconds / 60 % 60;
		int num3 = timeInSeconds % 60;
		return num.ToString("d2") + ":" + num2.ToString("d2") + ":" + num3.ToString("d2");
	}

	public void SetSlotDifficulty(int DifficultyValue, KAWidget Widget)
	{
		KAWidget[] componentsInChildren = Widget.GetComponentsInChildren<KAWidget>();
		foreach (KAWidget kAWidget in componentsInChildren)
		{
			string text = kAWidget.name;
			if (text.Contains("Star"))
			{
				int num = int.Parse(text.Split('_')[1]);
				kAWidget.SetVisibility(num <= DifficultyValue);
			}
		}
	}

	private void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		KAWidget kAWidget = (KAWidget)inUserData;
		if (!(kAWidget == null))
		{
			CoBundleItemData coBundleItemData = new CoBundleItemData(dataItem.IconName, "");
			kAWidget.SetUserData(coBundleItemData);
			coBundleItemData.LoadResource();
		}
	}
}
