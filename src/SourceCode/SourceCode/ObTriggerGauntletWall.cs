using UnityEngine;

public class ObTriggerGauntletWall : KAMonoBase
{
	public float _WallCollapseDelay;

	public float _AvatarAnimDelay;

	public float _AvatarAnimTime = 1f;

	public string _WallCollapseAnimName = string.Empty;

	public string _AvatarAnimName = string.Empty;

	public string _WallCollapseEvent = string.Empty;

	public SnSound _WallCollapseSound;

	public GameObject _WallCollapsePrtEffect;

	private float mWallCollapseDelay = -1f;

	private float mAvatarAnimDelay = -1f;

	private float mAvatarAnimTime = -1f;

	private void OnTriggerEnter(Collider inCollider)
	{
		if (AvAvatar.IsCurrentPlayer(inCollider.gameObject))
		{
			mWallCollapseDelay = _WallCollapseDelay;
			mAvatarAnimDelay = _AvatarAnimDelay;
		}
	}

	private void OnTriggerExit(Collider inCollider)
	{
		if (AvAvatar.IsCurrentPlayer(inCollider.gameObject) && !string.IsNullOrEmpty(_WallCollapseEvent) && GauntletRailShootManager.pInstance != null)
		{
			GauntletRailShootManager.pInstance.SendMessage(_WallCollapseEvent);
		}
	}

	private void Update()
	{
		if (mWallCollapseDelay != -1f)
		{
			mWallCollapseDelay -= Time.deltaTime;
			if (mWallCollapseDelay <= 0f)
			{
				mWallCollapseDelay = -1f;
				if (!string.IsNullOrEmpty(_WallCollapseAnimName) && base.animation != null)
				{
					base.animation.Play(_WallCollapseAnimName);
				}
				if (_WallCollapseSound != null)
				{
					_WallCollapseSound.Play();
				}
				if (_WallCollapsePrtEffect != null)
				{
					ParticleSystem[] componentsInChildren = Object.Instantiate(_WallCollapsePrtEffect, base.transform.position, base.transform.rotation).GetComponentsInChildren<ParticleSystem>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].Play();
					}
				}
			}
		}
		if (mAvatarAnimDelay != -1f)
		{
			mAvatarAnimDelay -= Time.deltaTime;
			if (mAvatarAnimDelay <= 0f && !string.IsNullOrEmpty(_AvatarAnimName))
			{
				mAvatarAnimDelay = -1f;
				mAvatarAnimTime = _AvatarAnimTime;
				SanctuaryManager.pCurPetInstance.pAnimToPlay = _AvatarAnimName;
			}
		}
		if (mAvatarAnimTime != -1f)
		{
			mAvatarAnimTime -= Time.deltaTime;
			if (mAvatarAnimTime <= 0f)
			{
				mAvatarAnimTime = -1f;
				SanctuaryManager.pCurPetInstance.pAnimToPlay = string.Empty;
			}
		}
	}
}
