using UnityEngine;

public class ObAvatarRespawnDelayed : ObAvatarRespawn
{
	private enum SpawnState
	{
		NONE,
		DEATH,
		SPAWN,
		FLASH
	}

	public AvAvatarRespawn _Respawn = new AvAvatarRespawn();

	private SpawnState mSpawnState;

	private float mRespawnTimer;

	private Animator mAvatarAnimator;

	private AvAvatarController mAvController;

	public override bool DoRespawn(GameObject obj)
	{
		if (AvAvatar.IsCurrentPlayer(obj) && mSpawnState != SpawnState.DEATH)
		{
			bool flag = true;
			if (mAvController == null)
			{
				mAvController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			}
			if (mAvController != null)
			{
				mAvController.ResetAvatarState();
			}
			if (_IgnoreFlying && AvAvatar.pSubState == AvAvatarSubState.FLYING && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetData.pStage >= RaisedPetStage.ADULT)
			{
				flag = mAvController != null && mAvController.pFlyingGlidingMode;
			}
			if (flag)
			{
				if (AvAvatar.pSubState == AvAvatarSubState.GLIDING)
				{
					mAvController.pIsPlayerGliding = false;
					mAvController.OnGlideEnd();
					AvAvatar.pSubState = AvAvatarSubState.NORMAL;
				}
				mRespawnTimer = _Respawn._DeathDelay;
				if (_Respawn._PrtDeath != null)
				{
					PlayParticles(_Respawn._PrtDeath, AvAvatar.pObject.transform.position);
				}
				AvAvatar.pState = AvAvatarState.PAUSED;
				if (_Respawn._DeathSFX != null)
				{
					SnChannel.Play(_Respawn._DeathSFX, "SFX_Pool", inForce: false);
				}
				HideAvatar(hide: true);
				mSpawnState = SpawnState.DEATH;
				AvAvatar.pInputEnabled = false;
				return true;
			}
		}
		return false;
	}

	private void HideAvatar(bool hide)
	{
		if (!mAvatarAnimator)
		{
			mAvatarAnimator = mAvController.GetComponentInChildren<Animator>();
		}
		if ((bool)mAvatarAnimator)
		{
			mAvatarAnimator.gameObject.SetActive(!hide);
		}
	}

	private void Update()
	{
		switch (mSpawnState)
		{
		case SpawnState.DEATH:
			UpdateDeath();
			break;
		case SpawnState.SPAWN:
			UpdateSpawn();
			break;
		case SpawnState.FLASH:
			UpdateFlash();
			break;
		}
	}

	private void UpdateDeath()
	{
		mRespawnTimer -= Time.deltaTime;
		if (!(mRespawnTimer <= 0f))
		{
			return;
		}
		if (_DefaultMarker != null)
		{
			AvAvatar.TeleportToObject(_DefaultMarker, 0f, _Respawn._DoTeleportFX);
		}
		else if (ObAvatarRespawn._Marker != null)
		{
			AvAvatar.TeleportToObject(ObAvatarRespawn._Marker, 0f, _Respawn._DoTeleportFX);
		}
		else
		{
			GameObject gameObject = GameObject.FindWithTag("Respawn");
			if (gameObject != null)
			{
				AvAvatar.TeleportToObject(gameObject, 0f, _Respawn._DoTeleportFX);
			}
			else
			{
				UtDebug.LogError("SafeZone marker not found");
			}
		}
		mRespawnTimer = _Respawn._SpawnDelay;
		mSpawnState = SpawnState.SPAWN;
	}

	private void UpdateSpawn()
	{
		mRespawnTimer -= Time.deltaTime;
		if (!(mRespawnTimer <= 0f))
		{
			return;
		}
		mSpawnState = SpawnState.FLASH;
		HideAvatar(hide: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (_Respawn._PrtRevive != null)
		{
			PlayParticles(_Respawn._PrtRevive, AvAvatar.pObject.transform.position);
		}
		if (_Respawn._ReviveSFX != null)
		{
			SnChannel.Play(_Respawn._ReviveSFX, "SFX_Pool", inForce: false);
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.pInputEnabled = true;
		mRespawnTimer = _Respawn._FlashTimer;
		if (mAvController != null)
		{
			mAvController.StartFlashing(_Respawn._FlashInterval);
			if (_Respawn._ImmuneAvatar)
			{
				mAvController.SetImmune(isImmune: true, _Respawn._FlashTimer);
			}
		}
		if (_Respawn._EnablePet && SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.EnableAllPets(enable: true);
		}
	}

	private void UpdateFlash()
	{
		mRespawnTimer -= Time.deltaTime;
		if (mRespawnTimer <= 0f)
		{
			if (mAvController != null)
			{
				mAvController.StopFlashing();
			}
			mSpawnState = SpawnState.NONE;
		}
	}

	private GameObject PlayParticles(GameObject pfx, Vector3 pos)
	{
		GameObject result = null;
		if (pfx != null)
		{
			result = Object.Instantiate(pfx, pos, Quaternion.identity);
		}
		return result;
	}

	private void OnDestroy()
	{
		mAvController?.StopFlashing();
	}

	private void OnDisable()
	{
		mAvController?.StopFlashing();
	}
}
