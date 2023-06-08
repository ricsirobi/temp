using System.Collections.Generic;
using UnityEngine;

public class ObClickableDragonEgg : ObClickableCommon
{
	public LocaleString _PicksUpEggText = new LocaleString("");

	public LocaleString _ChooseEggText = new LocaleString("Do you want to choose this egg?");

	public int _DragonType = 5;

	public HatcheryManager _HatcheryManager;

	public ObProximityHatch _ObProximityHatch;

	[SerializeField]
	private ObProximityStableHatch m_ObProximityStableHatch;

	private bool QuestActive()
	{
		if (!MissionManager.IsTaskActive("Action", "Name", "HatchDragon"))
		{
			return MissionManager.IsTaskActive("Action", "Name", "ChooseEgg");
		}
		return true;
	}

	public override bool IsActive()
	{
		if (base.IsActive() && !ObProximityHatch.pIsHatching && QuestActive())
		{
			return base.enabled;
		}
		return false;
	}

	public override bool CanPurchase()
	{
		return false;
	}

	public override void OnActivate()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		ShowDB(MissionManager.IsTaskActive("Action", "Name", "ChooseEgg") ? _ChooseEggText : _PicksUpEggText, "OnClickYes", "OnClickNo", null, null);
	}

	private void OnClickYes()
	{
		if (MissionManager.IsTaskActive("Action", "Name", "ChooseEgg"))
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "ChooseEgg", base.gameObject.name);
		}
		else if (_HatcheryManager != null)
		{
			StableManager.pCurIncubatorID = _HatcheryManager.pIncubatorStartSlotIndex;
			_HatcheryManager.AttachDragonEgg(_DragonType, AvAvatar.pObject, base.gameObject);
		}
		if (FUEManager.pIsFUERunning)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("name", SanctuaryData.FindSanctuaryPetTypeInfo(_DragonType)._Name);
			if ((bool)FUEManager.pInstance && FUEManager.pIsFUERunning)
			{
				AnalyticAgent.LogFTUEEvent(FTUEEvent.DRAGON_EGG_SELECTED, dictionary);
			}
		}
		ProcessCloseDB();
	}

	protected void OnClickNo()
	{
		ProcessCloseDB();
	}

	public void StartHatching()
	{
		if (_ObProximityHatch != null)
		{
			_ObProximityHatch.HatchDragonEgg(_DragonType);
		}
		else if (m_ObProximityStableHatch != null)
		{
			m_ObProximityStableHatch.HatchDragonEgg(_DragonType, pickedEgg: true);
		}
	}
}
