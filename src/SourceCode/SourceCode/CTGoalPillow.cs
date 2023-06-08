using UnityEngine;

public class CTGoalPillow : PhysicsObject
{
	public ParticleSystem _Particle;

	private ParticleSystem.EmissionModule mEmissionModule;

	private void Start()
	{
		UtDebug.Assert(_Particle != null, "No particle assigned for goa;l pillow");
		if ((bool)_Particle)
		{
			_Particle.Stop();
			mEmissionModule = _Particle.emission;
			mEmissionModule.enabled = false;
		}
	}

	public override void OnTriggerEnter2D(Collider2D other)
	{
		if (GoalManager.pInstance.IsSourceObject(other.gameObject))
		{
			if ((bool)_Particle)
			{
				mEmissionModule.enabled = true;
			}
			_Particle.Play();
		}
		base.OnTriggerEnter2D(other);
	}

	public override void Reset()
	{
		base.Reset();
		if ((bool)_Particle)
		{
			mEmissionModule.enabled = false;
			_Particle.Stop();
		}
	}
}
