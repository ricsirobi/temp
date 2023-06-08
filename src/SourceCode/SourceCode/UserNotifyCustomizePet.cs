using System;

public class UserNotifyCustomizePet : UserNotify
{
	public string _DragonCreationAsset = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	private bool mInitialized;

	private bool mUiDragonCustomizationLoaded;

	private static bool mHatchDragonQuestValidated;

	public static bool pHatchDragonQuestValidated
	{
		get
		{
			return mHatchDragonQuestValidated;
		}
		set
		{
			mHatchDragonQuestValidated = value;
		}
	}

	public override void OnWaitBeginImpl()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mInitialized = true;
	}

	private void Update()
	{
		if (!mInitialized || mUiDragonCustomizationLoaded)
		{
			return;
		}
		if (SanctuaryManager.pCurrentPetType != -1)
		{
			if (SanctuaryManager.IsActivePetDataReady())
			{
				RaisedPetData currentInstance = RaisedPetData.GetCurrentInstance(SanctuaryManager.pCurrentPetType);
				if (currentInstance != null && SanctuaryManager.IsPetInstanceAllowed(currentInstance) && SanctuaryManager.pInstance._CreateInstance)
				{
					LoadUiDragonCustomization();
				}
				else
				{
					OnWaitEnd();
				}
			}
		}
		else
		{
			OnWaitEnd();
		}
	}

	private void LoadUiDragonCustomization()
	{
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if (!(pCurPetInstance != null))
		{
			return;
		}
		int result = -1;
		bool flag = false;
		RaisedPetAttribute raisedPetAttribute = pCurPetInstance.pData.FindAttrData("_LastCustomizedStage");
		if (raisedPetAttribute != null && !string.IsNullOrEmpty(raisedPetAttribute.Value))
		{
			int.TryParse(raisedPetAttribute.Value, out result);
		}
		else
		{
			flag = true;
		}
		bool flag2 = result < 0;
		if (!flag2)
		{
			flag2 = MissionManager.IsTaskActive("Action", "Name", "GrowDragon") || (int)pCurPetInstance.pData.pStage > result;
		}
		if (flag2)
		{
			if (SanctuaryManager.pInstance != null && SanctuaryData.GetPetCustomizationType(pCurPetInstance.pTypeInfo._TypeID) != PetCustomizationType.None)
			{
				mUiDragonCustomizationLoaded = true;
				SanctuaryManager pInstance = SanctuaryManager.pInstance;
				pInstance.OnNameSelectionDone = (PetNameSelectionDoneEvent)Delegate.Combine(pInstance.OnNameSelectionDone, new PetNameSelectionDoneEvent(OnCustomizationDone));
				SanctuaryManager.pInstance.LoadPetCustomizationScreen(flag ? _DragonCreationAsset : "");
			}
			else
			{
				ValidateHatchDragonQuest();
				OnWaitEnd();
			}
		}
		else
		{
			ValidateHatchDragonQuest();
			OnWaitEnd();
		}
	}

	private void ValidateHatchDragonQuest()
	{
		if (mHatchDragonQuestValidated)
		{
			return;
		}
		mHatchDragonQuestValidated = true;
		bool flag = MissionManager.IsTaskActive("Action", "Name", "HatchDragon");
		if (RaisedPetData.pActivePets == null)
		{
			return;
		}
		foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				bool flag2 = false;
				RaisedPetAttribute raisedPetAttribute = raisedPetData.FindAttrData("HatchQuestPending");
				if (raisedPetAttribute != null && !string.IsNullOrEmpty(raisedPetAttribute.Value))
				{
					flag2 = UtStringUtil.Parse(raisedPetAttribute.Value, inDefault: false);
				}
				if (!flag2)
				{
					continue;
				}
				if (flag)
				{
					if (MissionManager.pInstance != null)
					{
						MissionManager.pInstance.CheckForTaskCompletion("Action", "HatchDragon");
					}
					return;
				}
				raisedPetData.SetAttrData("HatchQuestPending", false.ToString(), DataType.BOOL);
				raisedPetData.SaveDataReal();
			}
		}
	}

	private void OnCustomizationDone()
	{
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager pInstance = SanctuaryManager.pInstance;
			pInstance.OnNameSelectionDone = (PetNameSelectionDoneEvent)Delegate.Remove(pInstance.OnNameSelectionDone, new PetNameSelectionDoneEvent(OnCustomizationDone));
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "HatchDragon");
			MissionManager.pInstance.CheckForTaskCompletion("Action", "GrowDragon");
		}
		OnWaitEnd();
	}

	protected new void OnWaitEnd()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		base.OnWaitEnd();
	}
}
