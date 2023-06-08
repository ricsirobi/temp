using System.Collections;
using UnityEngine;

public class FogArea : MonoBehaviour
{
	public FogController _FogController;

	public Color _FogColor;

	public GameObject _FogParticle;

	public float _ParticleFadeTimer = 1f;

	private ParticleSystem mFogParticleSystem;

	private void Start()
	{
		mFogParticleSystem = _FogParticle.GetComponent<ParticleSystem>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (mFogParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission = mFogParticleSystem.emission;
				emission.enabled = false;
				StartCoroutine(RunParticleFade(mFogParticleSystem.main.startColor.color, new Color(0f, 0f, 0f, 0f)));
			}
			if (_FogController != null)
			{
				_FogController.SetFogColor(_FogColor);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (mFogParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission = mFogParticleSystem.emission;
				emission.enabled = true;
				StartCoroutine(RunParticleFade(new Color(0f, 0f, 0f, 0f), mFogParticleSystem.main.startColor.color));
			}
			if (_FogController != null)
			{
				_FogController.ResetFogColor();
			}
		}
	}

	private IEnumerator RunParticleFade(Color startColor, Color endColor)
	{
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[mFogParticleSystem.particleCount];
		mFogParticleSystem.GetParticles(particles);
		float lerpPercentage = 0f;
		float currentTimer = 0f;
		while (lerpPercentage <= 1f)
		{
			lerpPercentage = currentTimer / _ParticleFadeTimer;
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].startColor = Color32.Lerp(startColor, endColor, lerpPercentage);
			}
			currentTimer += Time.deltaTime;
			mFogParticleSystem.SetParticles(particles, particles.Length);
			yield return null;
		}
	}
}
