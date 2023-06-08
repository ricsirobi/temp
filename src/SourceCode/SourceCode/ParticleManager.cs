using UnityEngine;

public class ParticleManager : MonoBehaviour
{
	private static SpawnPool mSpawnPool;

	public SpawnPool _WebPool;

	public SpawnPool _MobilePool;

	private void Awake()
	{
		mSpawnPool = GetComponent<SpawnPool>();
		if (UtPlatform.IsMobile())
		{
			mSpawnPool = _MobilePool;
			if (_WebPool != null)
			{
				Object.Destroy(_WebPool.gameObject);
			}
		}
		else
		{
			mSpawnPool = _WebPool;
			if (_MobilePool != null)
			{
				Object.Destroy(_MobilePool.gameObject);
			}
		}
		if (mSpawnPool != null)
		{
			mSpawnPool.gameObject.SetActive(value: true);
		}
	}

	public static ParticleSystem PlayParticle(ParticleSystem particleSystem, Vector3 pos, Quaternion quat)
	{
		if (mSpawnPool != null)
		{
			return mSpawnPool.Spawn(particleSystem, pos, quat);
		}
		UtDebug.LogError("Particle could not be instantiated");
		return null;
	}

	public static Transform PlayParticle(Transform particleTransform)
	{
		if (mSpawnPool != null)
		{
			return mSpawnPool.Spawn(particleTransform);
		}
		UtDebug.LogError("Particle could not be instantiated");
		return null;
	}

	public static void Despawn(Transform t)
	{
		if (mSpawnPool != null)
		{
			mSpawnPool.Despawn(t);
		}
	}
}
