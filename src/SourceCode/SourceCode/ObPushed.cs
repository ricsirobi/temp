using UnityEngine;

public class ObPushed : MonoBehaviour
{
	public AudioClip _Sound;

	public Transform[] _ParticleEffect;

	private void OnPushed(GameObject go)
	{
		Transform[] particleEffect = _ParticleEffect;
		for (int i = 0; i < particleEffect.Length; i++)
		{
			Object.Instantiate(particleEffect[i], base.transform.position, base.transform.rotation).parent = base.transform;
		}
		if ((bool)_Sound)
		{
			SnChannel.Play(_Sound, "SFX_Pool", inForce: true, null);
		}
	}
}
