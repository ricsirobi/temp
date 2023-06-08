using UnityEngine;

public class LeadingParticle
{
	public GameObject _Particle;

	public Transform _StartObject;

	public Transform _TargetObject;

	public float _Interval;

	public float _StartDistance;

	private float mTimer;

	public void DoUpdate()
	{
		if (_Particle != null && _StartObject.gameObject.activeInHierarchy)
		{
			mTimer -= Time.deltaTime;
			if (mTimer <= 0f)
			{
				mTimer = _Interval;
				ObLeadingParticle.Initialize(_StartObject, _TargetObject, _Particle, _StartDistance);
			}
		}
	}

	public void Download(string inAssetName)
	{
		string[] array = inAssetName.Split('/');
		if (array.Length == 3)
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnParticleLoaded, typeof(GameObject));
		}
	}

	public void OnParticleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if ((uint)(inEvent - 2) <= 1u)
		{
			_Particle = (GameObject)inObject;
		}
	}
}
