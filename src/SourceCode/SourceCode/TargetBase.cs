using UnityEngine;

public class TargetBase : MonoBehaviour
{
	public GameObject _HitPrefab;

	public AudioClip _HitSFX;

	public string _SoundPool = "GSTarget_Pool";

	protected virtual void OnAmmoHit(ObAmmo projectile)
	{
		PlayParticle(projectile.pHitPos);
		PlayAudio(_HitSFX);
	}

	protected virtual void PlayParticle(Vector3 point)
	{
		if (_HitPrefab != null)
		{
			Object.Instantiate(_HitPrefab, point, Quaternion.identity);
		}
	}

	protected virtual void PlayAudio(AudioClip inClip)
	{
		if (inClip != null)
		{
			SnChannel.Play(inClip, _SoundPool, inForce: true);
		}
	}
}
