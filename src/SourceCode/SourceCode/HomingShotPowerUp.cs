public class HomingShotPowerUp : PowerUp
{
	public string _WeaponName = "Fireball";

	public override void Activate()
	{
		base.Activate();
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pWeaponManager != null)
		{
			SanctuaryManager.pCurPetInstance.pWeaponManager._PowerUpWeapon = _WeaponName;
		}
	}

	public override void DeActivate()
	{
		base.DeActivate();
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pWeaponManager != null)
		{
			SanctuaryManager.pCurPetInstance.pWeaponManager._PowerUpWeapon = null;
		}
	}
}
