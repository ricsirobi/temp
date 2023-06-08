using UnityEngine;

public class TestEmitter : MonoBehaviour
{
	public Transform[] particle;

	public AudioClip sound;

	public string _key;

	private void Update()
	{
		if (!Input.GetKeyUp(_key))
		{
			return;
		}
		for (int i = 0; i < particle.Length; i++)
		{
			ParticleSystem[] componentsInChildren = Object.Instantiate(particle[i], base.transform.position, base.transform.rotation).GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[i].Play();
			}
		}
		if (sound != null)
		{
			AudioSource.PlayClipAtPoint(sound, Camera.main.transform.position);
		}
	}
}
