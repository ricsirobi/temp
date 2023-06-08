using UnityEngine;

public class ObCollisionEmitterSn : MonoBehaviour
{
	public Transform[] particle;

	public SnSound sound;

	public GameObject _MessageObject;

	private void OnTriggerEnter(Collider other)
	{
		for (int i = 0; i < particle.Length; i++)
		{
			ParticleSystem[] componentsInChildren = Object.Instantiate(particle[i], base.transform.position, base.transform.rotation).GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].Play();
			}
		}
		if (sound != null)
		{
			sound.Play();
		}
		if (_MessageObject != null)
		{
			_MessageObject.BroadcastMessage("OnCollisionEmitterComplete", SendMessageOptions.DontRequireReceiver);
		}
	}
}
