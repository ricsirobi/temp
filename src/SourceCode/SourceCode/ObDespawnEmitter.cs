using UnityEngine;

public class ObDespawnEmitter : MonoBehaviour
{
	public SpawnPool _Pool;

	public float _TimeToWaitAfterNoParticles = 0.8f;

	private ParticleSystem[] mParticleSystems;

	private float mElapsedTime;

	private void Start()
	{
		if (mParticleSystems == null)
		{
			mParticleSystems = GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < mParticleSystems.Length; i++)
			{
				_TimeToWaitAfterNoParticles = ((mParticleSystems[i].main.startLifetime.constantMax > _TimeToWaitAfterNoParticles) ? mParticleSystems[i].main.startLifetime.constantMax : _TimeToWaitAfterNoParticles);
			}
		}
	}

	private void OnSpawned()
	{
		mElapsedTime = _TimeToWaitAfterNoParticles;
	}

	private void Update()
	{
		if (mParticleSystems == null)
		{
			return;
		}
		mElapsedTime -= Time.deltaTime;
		if (ShouldBeDespawned())
		{
			for (int i = 0; i < mParticleSystems.Length; i++)
			{
				mParticleSystems[i].Stop();
			}
			if (_Pool != null)
			{
				_Pool.Despawn(base.transform);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private bool ShouldBeDespawned()
	{
		if (mElapsedTime <= 0f)
		{
			return true;
		}
		for (int i = 0; i < mParticleSystems.Length; i++)
		{
			if (mParticleSystems[i].particleCount > 0)
			{
				return false;
			}
		}
		return true;
	}
}
