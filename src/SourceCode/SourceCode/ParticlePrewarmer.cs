using UnityEngine;

public class ParticlePrewarmer : MonoBehaviour
{
	public ParticleSystem _ParticleSystem;

	private void OnBecameVisible()
	{
		if (_ParticleSystem != null)
		{
			_ParticleSystem.Play();
		}
	}

	private void OnBecameInvisible()
	{
		if (_ParticleSystem != null)
		{
			_ParticleSystem.Stop();
		}
	}
}
