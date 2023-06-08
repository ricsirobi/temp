using System;

public class NPCPetCSM : ObContextSensitive
{
	public ContextSensitiveState[] _Menus;

	public string _MountCSItemName = "Mount";

	[NonSerialized]
	public MountableNPCPet _NpcPet;

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		inStatesArrData = _Menus;
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (inMenuType == ContextSensitiveStateType.ONCLICK && _NpcPet == null && base.pUI != null && !string.IsNullOrEmpty(_MountCSItemName))
		{
			ContextData contextData = GetContextData(_MountCSItemName);
			if (contextData != null)
			{
				base.pUI.RemoveContextDataFromList(contextData, enableRefreshItems: true);
			}
		}
	}

	public virtual void OnContextAction(string inName)
	{
		if (!(inName == _MountCSItemName))
		{
			return;
		}
		if (_NpcPet != null)
		{
			if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
			}
			_NpcPet.StartMount();
		}
		CloseMenu();
	}
}
