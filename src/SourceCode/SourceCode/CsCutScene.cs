using UnityEngine;

public class CsCutScene : CoAnimController
{
	public void OnMountDragon()
	{
		if (SanctuaryManager.pCurPetInstance != null && !SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			SanctuaryManager.pMountedState = true;
			SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, SanctuaryManager.pCurPetInstance.pCurrentSkillType);
			if (SanctuaryManager.pCurPetInstance.pCurrentSkillType == PetSpecialSkillType.FLY)
			{
				component.SetFlyingState(FlyingState.Normal);
			}
		}
	}

	public void OnDismountDragon()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pMountedState = false;
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
	}

	public void OnDragonPlayAnimOnce(string aname)
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			TransformPlayAnimRecursively(SanctuaryManager.pCurPetInstance.transform, aname, WrapMode.Once);
		}
	}

	public void OnDragonPlayAnim(string aname)
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			TransformPlayAnimRecursively(SanctuaryManager.pCurPetInstance.transform, aname, WrapMode.Loop);
		}
	}
}
