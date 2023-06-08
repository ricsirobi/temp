using UnityEngine;

public class ObParticleEmitRandom : ObActivateRandom
{
	private ParticleSystem mParticleSystem;

	public override void ActivateObject(int index)
	{
		if (base.enabled && _Objects != null && _Objects.Length != 0 && _Objects.Length > index)
		{
			if (mParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission = mParticleSystem.emission;
				emission.enabled = false;
			}
			mActiveObject = _Objects[index];
			mActiveObject.SetActive(value: true);
			if (mActiveObject != null)
			{
				mParticleSystem = mActiveObject.GetComponent<ParticleSystem>();
			}
			if (mParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission2 = mParticleSystem.emission;
				emission2.enabled = true;
			}
		}
	}

	public override void DeactivateObject()
	{
		if (mParticleSystem != null)
		{
			ParticleSystem.EmissionModule emission = mParticleSystem.emission;
			emission.enabled = false;
			mParticleSystem = null;
		}
		mActiveObject = null;
	}
}
