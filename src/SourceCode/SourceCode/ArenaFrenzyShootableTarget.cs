using UnityEngine;

public class ArenaFrenzyShootableTarget : ArenaFrenzyTarget
{
	protected override bool ProcessTargetHit(Collider inOther)
	{
		ObAmmo component = inOther.GetComponent<ObAmmo>();
		if (component != null)
		{
			OnAmmoHit(component);
			return true;
		}
		return false;
	}

	private void OnAmmoHit(ObAmmo projectile)
	{
		UserProfileData inProfileData = null;
		if (projectile.pCreator != null)
		{
			PetWeaponManager component = projectile.pCreator.GetComponent<PetWeaponManager>();
			if (component != null && component.SanctuaryPet != null && component.SanctuaryPet.mAvatar != null)
			{
				if (component.SanctuaryPet.mAvatar.gameObject == AvAvatar.pObject)
				{
					inProfileData = UserProfile.pProfileData;
				}
				else
				{
					MMOAvatar component2 = component.SanctuaryPet.mAvatar.GetComponent<MMOAvatar>();
					if (component2 != null)
					{
						inProfileData = component2.pProfileData;
					}
				}
			}
		}
		PlayHitAnim();
		if (mParentMapElement != null)
		{
			mParentMapElement.HandleTargetHit(this, projectile.IsLocal, inProfileData);
		}
	}
}
