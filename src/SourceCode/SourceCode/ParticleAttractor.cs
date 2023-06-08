using UnityEngine;

public class ParticleAttractor : MonoBehaviour
{
	[SerializeField]
	private Transform _attractorTransform;

	[SerializeField]
	private ParticleSystem[] _particlesList = new ParticleSystem[7];

	private ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[1000];

	public void LateUpdate()
	{
		ParticleSystem[] particlesList = _particlesList;
		foreach (ParticleSystem particleSystem in particlesList)
		{
			if (particleSystem.isPlaying)
			{
				int particles = particleSystem.GetParticles(_particles);
				Vector3 position = _attractorTransform.position;
				for (int j = 0; j < particles; j++)
				{
					_particles[j].position = _particles[j].position + (position - _particles[j].position) / _particles[j].remainingLifetime * Time.deltaTime;
				}
				particleSystem.SetParticles(_particles, particles);
			}
		}
	}
}
