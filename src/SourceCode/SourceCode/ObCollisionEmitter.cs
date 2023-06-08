using UnityEngine;

public class ObCollisionEmitter : MonoBehaviour
{
	public ParticleSystem[] _Particle;

	public AudioClip sound;

	public GameObject _MessageObject;

	private void OnTriggerEnter(Collider other)
	{
		ParticleSystem[] particle = _Particle;
		for (int i = 0; i < particle.Length; i++)
		{
			ParticleSystem.EmissionModule emission = particle[i].emission;
			emission.enabled = true;
		}
		if (sound != null)
		{
			SnChannel.Play(sound);
		}
		if (_MessageObject != null)
		{
			_MessageObject.BroadcastMessage("OnCollisionEmitterComplete", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		ParticleSystem[] particle = _Particle;
		for (int i = 0; i < particle.Length; i++)
		{
			ParticleSystem.EmissionModule emission = particle[i].emission;
			emission.enabled = false;
		}
	}
}
