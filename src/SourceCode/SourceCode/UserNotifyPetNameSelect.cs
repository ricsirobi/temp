using System;

public class UserNotifyPetNameSelect : UserNotify
{
	private bool mInitialized;

	private bool mPetNameSelectionUILoaded;

	public override void OnWaitBeginImpl()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mInitialized = true;
	}

	private void Update()
	{
		if (!mInitialized || mPetNameSelectionUILoaded)
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
					LoadPetNameSelectionScreen();
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

	private void LoadPetNameSelectionScreen()
	{
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if (pCurPetInstance != null)
		{
			if ((!pCurPetInstance.pData.pIsNameCustomized || string.IsNullOrEmpty(pCurPetInstance.pData.Name.Trim())) && SanctuaryManager.pInstance != null)
			{
				mPetNameSelectionUILoaded = true;
				SanctuaryManager pInstance = SanctuaryManager.pInstance;
				pInstance.OnNameSelectionDone = (PetNameSelectionDoneEvent)Delegate.Combine(pInstance.OnNameSelectionDone, new PetNameSelectionDoneEvent(OnWaitEnd));
				SanctuaryManager.pInstance.LoadPetNameSelectionScreen();
			}
			else
			{
				OnWaitEnd();
			}
		}
	}

	protected new void OnWaitEnd()
	{
		if (SanctuaryManager.pInstance != null)
		{
			SanctuaryManager pInstance = SanctuaryManager.pInstance;
			pInstance.OnNameSelectionDone = (PetNameSelectionDoneEvent)Delegate.Remove(pInstance.OnNameSelectionDone, new PetNameSelectionDoneEvent(OnWaitEnd));
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		base.OnWaitEnd();
	}
}
