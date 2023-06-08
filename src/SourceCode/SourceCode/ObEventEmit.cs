using UnityEngine;

public class ObEventEmit : MonoBehaviour
{
	public Transform[] _Particles;

	public SnSound _Sound;

	public SpawnPool _Pool;

	private bool mSpawn;

	private void Update()
	{
		if (mSpawn)
		{
			for (int i = 0; i < _Particles.Length; i++)
			{
				SpawnTransform(_Particles[i]);
			}
			if (_Sound != null && _Sound._AudioClip != null)
			{
				_Sound.Play();
			}
			mSpawn = false;
		}
	}

	private void OnEmitEvent()
	{
		mSpawn = true;
	}

	private void SpawnTransform(Transform Obj)
	{
		Transform transform = null;
		bool flag = true;
		if (_Pool != null)
		{
			transform = _Pool.Spawn(Obj, base.transform.position, base.transform.rotation);
		}
		if (transform == null)
		{
			transform = Object.Instantiate(Obj, base.transform.position, base.transform.rotation);
			flag = false;
		}
		ParticleSystem[] componentsInChildren = transform.gameObject.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != null)
			{
				componentsInChildren[i].time = 0f;
				componentsInChildren[i].Play();
			}
		}
		ObDespawnEmitter[] componentsInChildren2 = transform.gameObject.GetComponentsInChildren<ObDespawnEmitter>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j]._Pool = _Pool;
			componentsInChildren2[j].enabled = flag;
		}
	}
}
